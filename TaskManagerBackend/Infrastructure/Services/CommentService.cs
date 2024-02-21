using Application.Comment;
using Application.Common;
using Application.Interface;
using Application.Task;
using Application.TaskManager;
using Domain;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistance;

namespace Infrastructure.Services
{
    public class CommentService : ICommentService
    {
        private readonly DataContext _dbContext;
        private readonly IResponseGeneratorService _responseGeneratorService;
        private readonly ITokenService _tokenService;
        public readonly UserManager<AppUser> _userManager;
        private readonly ILog _log;

        public CommentService(DataContext dbContext, IResponseGeneratorService responseGeneratorService, ITokenService tokenService, UserManager<AppUser> userManager)
        {
            _dbContext = dbContext;
            _responseGeneratorService = responseGeneratorService;
            _tokenService = tokenService;
            _log = LogManager.GetLogger(typeof(CommentService));
            _userManager = userManager;
        }

        public async Task<ReturnResponse> AddCommentAsync(AddCommentDto commentDto ,string authorizationToken)
        {
           
            ReturnResponse<HeaderReturnDto> headerCheckResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
            try
            {
                if (headerCheckResponse.Data.Status)
                {
                    _log.Info("Adding comment");
                    var commentEntity = new Comment
                    {
                        CommentDescription = commentDto.CommentDescription,
                        CommentedById = headerCheckResponse.Data.UserId,
                        TaskId = commentDto.TaskId,
                        CreatedAt = DateTime.Now,
                        CreatedById = headerCheckResponse.Data.UserId,
                        IsDeleted = false
                    };

                    _dbContext.Comments.Add(commentEntity);
                    await _dbContext.SaveChangesAsync();

                    // Return success response
                    _log.Info("Comment added successfully");
                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status201Created, "Comment added successfully");
                }
                else
                {
                    _log.Error($"User not authorized while adding comment");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                _log.Error($"Error occurred while adding comment {ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error adding comment: {ex.Message}");
            }
        }

        public async Task<ReturnResponse> UpdateCommentAsync(Guid commentId, EditCommentDto commentDto , string authorizationToken)
        {
            ReturnResponse<HeaderReturnDto> headerCheckResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
            try
            {
                if (headerCheckResponse.Data.Status)
                {
                    _log.Info("updating comment");
                    var commentEntity = await _dbContext.Comments
                   .FirstOrDefaultAsync(c => c.CommentId == commentId && !c.IsDeleted);

                    if (commentEntity == null)
                    {
                        return await _responseGeneratorService.GenerateResponseAsync(
                            false, StatusCodes.Status404NotFound, "Comment not found.");
                    }

                    commentEntity.CommentDescription = commentDto.CommentDescription;
                    commentEntity.CommentedById = headerCheckResponse.Data.UserId;
                    commentEntity.UpdatedAt = DateTime.Now;
                    commentEntity.UpdatedById = headerCheckResponse.Data.UserId;

                    await _dbContext.SaveChangesAsync();

                    // Return success response
                    _log.Info("Comment updated successfully");
                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Comment updated successfully");
                }
                else
                {
                    _log.Error("User not authorized while updating comment");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                _log.Error($"Error occurred while updating comment {ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error updating comment: {ex.Message}");
            }
        }

        public async Task<ReturnResponse> GetCommentByIdAsync(Guid commentId, string authorizationToken)
        {
            ReturnResponse<HeaderReturnDto> checkHeaderResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
            try
            {
                if (checkHeaderResponse.Data.Status)
                {
                    _log.Info("Getting a particular comment");
                    var commentEntity = await _dbContext.Comments
                   .Include(c => c.CommentedTask)
                       .ThenInclude(t => t.TaskCategory)
                       .Include(c => c.CreatedByAppUser)
                       .Include(c => c.UpdatedByAppUser)
                       .Include(c => c.CommentedAppUser)
                   .FirstOrDefaultAsync(c => c.CommentId == commentId && !c.IsDeleted && c.CommentedTask != null && !c.CommentedTask.IsDeleted);

                    if (commentEntity == null)
                    {
                        _log.Error($"Could not find comment");
                        return await _responseGeneratorService.GenerateResponseAsync(
                            false, StatusCodes.Status404NotFound, "Comment not found.");
                    }

                    // Fetch user IDs for the comment
                var userIds = new List<string>
                {
                    commentEntity.CommentedById,
                    commentEntity.CreatedById,
                    commentEntity.UpdatedById
                };

                    // Fetch users in a single query
                    var userDictionary = await GetUserDictionaryAsync(userIds);

                    var commentDetailsDto = new GetAllCommentDto
                    {
                        CommentDescription = commentEntity.CommentDescription,
                        CommentedById = commentEntity.CommentedById,
                        CommentedByName = userDictionary.GetValueOrDefault(commentEntity.CommentedById)?.UserName,
                        Tasks = commentEntity.CommentedTask != null
                            ? new List<TaskDetails>
                            {
                    new TaskDetails
                    {
                        TaskName = commentEntity.CommentedTask.TaskName,
                        TaskPriority = (int)commentEntity.CommentedTask.TaskPriority,
                        Status = (int)commentEntity.CommentedTask.Status,
                        TaskDescription = commentEntity.CommentedTask.TaskDescription,
                        CategoryId = commentEntity.CommentedTask.CategoryId,
                        AssignedBy = commentEntity.CommentedTask.AssignedById,
                        DueDate = commentEntity.CommentedTask.DueDate,
                        CreatedbyName = userDictionary.GetValueOrDefault(commentEntity.CommentedTask.CreatedById)?.UserName,
                        UpdatedbyName = userDictionary.GetValueOrDefault(commentEntity.CommentedTask.UpdatedById ?? "")?.UserName,
                        TaskCategory = new TaskCategoryDetailsDto
                        {
                            CategoryName = commentEntity.CommentedTask.TaskCategory?.CategoryName,
                            CategoryDescription = commentEntity.CommentedTask.TaskCategory?.CategoryDescription
                        }
                    }
                            }
                            : new List<TaskDetails>()
                    };


                    _log.Info("Found a comment");
                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Successfully got a comment", commentDetailsDto);
                }
                else
                {
                    _log.Error("User not authorized to get a comment");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                _log.Error($"Error ocurred while getting a comment {ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error getting comment by ID: {ex.Message}");
            }
        }




        public async Task<ReturnResponse> GetAllCommentsAsync(string authorizationToken)
        {
            ReturnResponse<HeaderReturnDto> checkHeaderResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
            try
            {
                if (checkHeaderResponse.Data.Status)
                {
                    _log.Info("Getting all comments");
                    var commentsEntities = await _dbContext.Comments
                  .Include(c => c.CommentedTask)
                      .ThenInclude(t => t.TaskCategory)
                      .Include(c => c.CommentedAppUser)
                      .Include(c => c.CreatedByAppUser)
                      .Include(c => c.UpdatedByAppUser)
                  .Where(c => !c.IsDeleted && c.CommentedTask != null && !c.CommentedTask.IsDeleted)
                  .ToListAsync();

                    // Fetch user IDs for comments
                    var userIds = commentsEntities.SelectMany(c => new[] { c.CommentedById, c.CreatedById, c.UpdatedById }).Distinct().ToList();

                    // Fetch users in a single query
                    var userDictionary = await GetUserDictionaryAsync(userIds);

                    var commentsDto = commentsEntities.Select(c => new GetAllCommentDto
                    {
                        CommentDescription = c.CommentDescription,
                        CommentedById = c.CommentedById,
                        CommentedByName = userDictionary.GetValueOrDefault(c.CommentedById)?.UserName,
                        Tasks = c.CommentedTask != null
                            ? new List<TaskDetails>
                            {
                    new TaskDetails
                    {
                        TaskName = c.CommentedTask.TaskName,
                        TaskPriority = (int)c.CommentedTask.TaskPriority,
                        Status = (int)c.CommentedTask.Status,
                        TaskDescription = c.CommentedTask.TaskDescription,
                        CategoryId = c.CommentedTask.CategoryId,
                        AssignedBy = c.CommentedTask.AssignedById,
                        DueDate = c.CommentedTask.DueDate,
                        CreatedbyName = userDictionary.GetValueOrDefault(c.CommentedTask.CreatedById)?.UserName,
                        UpdatedbyName = userDictionary.GetValueOrDefault(c.CommentedTask.UpdatedById ?? "")?.UserName,
                        TaskCategory = new TaskCategoryDetailsDto
                        {
                            CategoryName = c.CommentedTask.TaskCategory?.CategoryName,
                            CategoryDescription = c.CommentedTask.TaskCategory?.CategoryDescription
                        }
                    }
                            }
                            : new List<TaskDetails>()
                    }).ToList();
                    _log.Info("Successfully got all comments");
                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Successfully got all comments", commentsDto);
                }
                else
                {
                    _log.Error("User not authorized to get all comments");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                _log.Error($"Error ocurred while getting all comments {ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error getting all comments: {ex.Message}");
            }
        }

        private async Task<Dictionary<string, AppUser>> GetUserDictionaryAsync(List<string> userIds)
        {
            var users = await _dbContext.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            return users;
        }


        public async Task<ReturnResponse> DeleteCommentAsync(Guid commentId, string authorizationToken)
        {
            ReturnResponse<HeaderReturnDto> checkHeaderResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
            try
            {
                if (checkHeaderResponse.Data.Status)
                {
                    string requestingUserId = checkHeaderResponse.Data.UserId;
                    var requestingUser = await _userManager.FindByIdAsync(requestingUserId);

                    if (requestingUser == null)
                    {
                        _log.Warn("User not found.");
                        return await _responseGeneratorService.GenerateResponseAsync(
                            false, StatusCodes.Status404NotFound, "Requesting User not found.");
                    }

                    var isRequestingUserAdmin = await _userManager.IsInRoleAsync(requestingUser, "Admin");

                    _log.Info("Deleting comment");
                    var commentEntity = await _dbContext.Comments
                    .FirstOrDefaultAsync(c => c.CommentId == commentId && !c.IsDeleted);

                    if (commentEntity == null)
                    {
                        _log.Error("Comment not found");
                        return await _responseGeneratorService.GenerateResponseAsync(
                            false, StatusCodes.Status404NotFound, "Comment not found.");
                    }

                    if (!(commentEntity.CommentedById == requestingUserId || isRequestingUserAdmin))
                    {
                        return await _responseGeneratorService.GenerateResponseAsync(
                           false, StatusCodes.Status403Forbidden, "User not authorized to delete comment");
                    }

                  
                    commentEntity.IsDeleted = true;
                    commentEntity.UpdatedAt = DateTime.UtcNow;
                    commentEntity.UpdatedById = checkHeaderResponse.Data.UserId;
                    commentEntity.DeletedById = checkHeaderResponse.Data.UserId;

                    await _dbContext.SaveChangesAsync();

                    // Return success response
                    _log.Info("Comment deleted successfully");
                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Comment deleted successfully");
                }
                else
                {
                    _log.Error("User not authorized to delete comment , invalid token");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                _log.Error($"Error ocurred while deleting comment {ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error deleting comment: {ex.Message}");
            }
        }
    }

}
