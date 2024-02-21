using Application.AdminDto;
using Application.Common;
using Application.Interface;
using Application.TaskManager;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly DataContext _dataContext;
        private readonly IResponseGeneratorService _responseGeneratorService;
        private readonly ITokenService _tokenService;
        private readonly UserManager<AppUser> _userManager;
        public AdminService(DataContext dataContext, IResponseGeneratorService responseGeneratorService, ITokenService tokenService, UserManager<AppUser> userManager)
        {
            _dataContext = dataContext;
            _responseGeneratorService = responseGeneratorService;
            _tokenService = tokenService;
            _userManager = userManager;
        }
        public async Task<ReturnResponse<List<UserDetailsDTO>>> GetAllUser(string authorizationToken)
        {
            try
            {
                ReturnResponse<HeaderReturnDto> headerCheckResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
                if (headerCheckResponse.Data.Status)
                {
                    var tasks = await _dataContext.TaskEntities
                        .Include(t => t.AssignedByAppUser)
                        .Include(t => t.TaskCategory)
                        .Include(t => t.Comments)
                        .Where(t => !t.IsDeleted)
                        .ToListAsync();

                    var userDetailsDto = tasks.GroupBy(t => t.AssignedByAppUser)
                          .Select(group => new UserDetailsDTO
                          {
                              FirstName = group.Key.FirstName,
                              LastName = group.Key.LastName,
                              UserName = group.Key.UserName,
                              Email = group.Key.Email,
                              Tasks = group.Select(t => new TaskDTO
                              {
                                  TaskName = t.TaskName,
                                  TaskDescription = t.TaskDescription,
                                  DueDate = t.DueDate,
                                  AssignedBy = t.AssignedByAppUser.UserName,
                                  TaskCategory = new TaskCategoryDTO
                                  {
                                      TaskCategoryName = t.TaskCategory.CategoryName
                                  },
                                  Comments = t.Comments.Select(c => new CommentDTO
                                  {
                                      CommentedBy = c.CommentedAppUser.UserName,
                                      CommentDescription = c.CommentDescription
                                  }).ToList()

                              }).ToList()
                          }).ToList();

                    if (userDetailsDto == null)
                    {
                        return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status404NotFound, "User List not found", userDetailsDto);
                    }
                    else
                    {
                        return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "User List found sucessfully", userDetailsDto);
                    }
                }


                else
                {
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, $"Invalid token , or token is revoked ", new List<UserDetailsDTO>());
                }
            }
            catch (Exception ex)
            {
                return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status500InternalServerError, $"Error occured while getting user {ex.Message}",new List<UserDetailsDTO>());
            }
        }
        public async Task<ReturnResponse<UserDetailsDTO>> GetUserDetails(string authorizationToken)
        {
            try
            {
                ReturnResponse<HeaderReturnDto> headerCheckResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
                if (headerCheckResponse.Data.Status)
                {

                    var requestingUserId = headerCheckResponse.Data.UserId;

                    var userTasks = await _dataContext.TaskEntities
                        .Include(t => t.AssignedByAppUser)
                        .Include(t => t.TaskCategory)
                        .Include(t => t.Comments)
                        .Where(t => !t.IsDeleted && t.AssignedByAppUser.Id == requestingUserId)
                        .ToListAsync();

                    if (!userTasks.Any())
                    {
                        return await _responseGeneratorService.GenerateResponseAsync<UserDetailsDTO>(false, StatusCodes.Status404NotFound, "User not found or no tasks assigned", null);
                    }

                    var user = userTasks.First().AssignedByAppUser;
                    var userDetailsDto = new UserDetailsDTO
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserName = user.UserName,
                        Email = user.Email,
                        Tasks = userTasks.Select(t => new TaskDTO
                        {
                            TaskName = t.TaskName,
                            TaskDescription = t.TaskDescription,
                            DueDate = t.DueDate,
                            AssignedBy = t.AssignedByAppUser.UserName,
                            TaskCategory = new TaskCategoryDTO
                            {
                                TaskCategoryName = t.TaskCategory.CategoryName
                            },
                            Comments = t.Comments.Select(c => new CommentDTO
                            {
                                CommentedBy = c.CommentedAppUser.UserName,
                                CommentDescription = c.CommentDescription
                            }).ToList()
                        }).ToList()
                    };

                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "User details retrieved successfully", userDetailsDto);
                }
                else
                {
                    return await _responseGeneratorService.GenerateResponseAsync<UserDetailsDTO>(false, StatusCodes.Status401Unauthorized, $"Invalid token, or token is revoked", null);
                }
            }
            catch (Exception ex)
            {
                return await _responseGeneratorService.GenerateResponseAsync<UserDetailsDTO>(false, StatusCodes.Status500InternalServerError, $"Error occurred while getting user details: {ex.Message}", null);
            }
        }
        public async Task<ReturnResponse> UpdateUser(Guid userId ,EditUserDetailsDTO editUserDetailsDTO ,string authorizationToken)
        {
            try
            {
                ReturnResponse<HeaderReturnDto> headerCheckResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);

                if (headerCheckResponse.Data.Status)
                {
                    string requestingUserId = headerCheckResponse.Data.UserId;
      
                    if (!(userId.ToString() == requestingUserId))
                    {
                        return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status403Forbidden, "User not Authorized to update");

                    }

                    var existingUser = await _userManager.FindByIdAsync(userId.ToString());
                    if (existingUser == null)
                    {
                        return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status404NotFound, $" Existing User not found ");

                    }

                    existingUser.FirstName = editUserDetailsDTO.FirstName;
                    existingUser.LastName = editUserDetailsDTO.LastName;
                    existingUser.UserName = editUserDetailsDTO.UserName;

                    await _dataContext.SaveChangesAsync();

                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, $" User updated successfully ");
                }
                else
                {
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, $" Invalid token or revoked token ");
                }

            }
            catch (Exception ex)
            {
                return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status500InternalServerError, $"Error ocurred while updating user : {ex.Message}");
            }

        }

    }
}
