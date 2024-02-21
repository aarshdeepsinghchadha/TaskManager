using Application.Common;
using Application.Interface;
using Application.TaskCategory;
using Application.TaskManager;
using Domain;
using log4net;
using log4net.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class TaskCategoryService : ITaskCategoryService
    {
        private readonly IResponseGeneratorService _responseGeneratorService;
        private readonly DataContext _dbContext;
        private readonly ITokenService _tokenService;
        private readonly ILog _log;
        
        public TaskCategoryService(IResponseGeneratorService responseGeneratorService, DataContext dbContext, ITokenService tokenService)
        {
            _responseGeneratorService = responseGeneratorService;
            _dbContext = dbContext;
            _tokenService = tokenService;
            _log = LogManager.GetLogger(typeof(TaskCategoryService));
           
        }

        public async Task<PagedResponse<List<GetTaskCategoryListingDto>>> GetTaskCategoryListing(string sortColumn, int sortDirection, int pageNumber, int pageSize, string authorizationToken)
        {
            try
            {
                ReturnResponse<HeaderReturnDto> headerCheckResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
                if(headerCheckResponse.Data.Status)
                {
                    _log.Info($"Filtering TaskCategory List based on {sortColumn}");
                    var taskCategoryEntities = _dbContext.TaskCategories.Include(t => t.Tasks).Where(t => !t.IsDeleted).Skip((pageNumber - 1) * pageSize).Take(pageSize);

                    pageNumber = Math.Max(1, pageNumber);
                    pageSize = Math.Min(5, pageSize);
                    int totalRecords = pageSize / pageNumber;

                    IOrderedQueryable<TaskCategory> sortedTaskCategories;

                    switch (sortColumn)
                    {
                        case "Category Name":

                            if (sortDirection == -1)
                            {
                                sortedTaskCategories = taskCategoryEntities.OrderBy(t => t.CategoryName);
                            }
                            else if (sortDirection == 1)
                            {
                                sortedTaskCategories = taskCategoryEntities.OrderByDescending(t => t.CategoryName);
                            }
                            else
                            {
                                return await _responseGeneratorService.GenerateResponseAsync<List<GetTaskCategoryListingDto>>(false, StatusCodes.Status404NotFound, "Invalid SortColumn", null, pageNumber, pageSize, 0);
                            }
                            break;

                        case "MostTaskCounts":
                            if (sortDirection == -1)
                            {
                                sortedTaskCategories = taskCategoryEntities.OrderBy(t => t.Tasks.Count).ThenBy(t => t.CategoryName);
                            }
                            else if (sortDirection == 1)
                            {
                                sortedTaskCategories = taskCategoryEntities.OrderByDescending(t => t.Tasks.Count).ThenByDescending(t => t.CategoryName);
                            }
                            else
                            {
                                return await _responseGeneratorService.GenerateResponseAsync<List<GetTaskCategoryListingDto>>(false, StatusCodes.Status404NotFound, "Invalid SortColumn", null, pageNumber, pageSize, 0);
                            }
                            break;

                        default:
                            return await _responseGeneratorService.GenerateResponseAsync<List<GetTaskCategoryListingDto>>(false, StatusCodes.Status404NotFound, "Invalid SortColumn", null, pageNumber, pageSize, 0);
                    }


                    var taskCategoryDto = await sortedTaskCategories.Select(t => new GetTaskCategoryListingDto
                    {
                        CategoryName = t.CategoryName,
                        MostTaskCounts = t.Tasks.Count,

                    }).ToListAsync();

                    _log.Info($"CategoryList filtered successfully based on {sortColumn}");
                    return await _responseGeneratorService.GenerateResponseAsync<List<GetTaskCategoryListingDto>>(true, StatusCodes.Status200OK, "Category List sorted succesfully", taskCategoryDto, pageNumber, pageSize, totalRecords);

                }
                else
                {
                    _log.Error($"Failed to filter Task Category based on {sortColumn}");
                    return await _responseGeneratorService.GenerateResponseAsync<List<GetTaskCategoryListingDto>>(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!",null,0,0,0);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error ocurred while filtering Task Category list : {ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync<List<GetTaskCategoryListingDto>>(false, StatusCodes.Status404NotFound, ex.Message, null, 0, 0, 0);
            }
        }

        public async Task<ReturnResponse> AddTaskCategoryAsync(AddTaskCategoryDto addTaskCategoryDto, string authorizationToken)
        {
            try
            {
                ReturnResponse<HeaderReturnDto> headerCheckResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
                if (headerCheckResponse.Data.Status)
                {
                    _log.Info("User adding Task Category");
                    // Create a new TaskCategory entity
                    var taskCategory = new TaskCategory
                    {
                        CategoryName = addTaskCategoryDto.CategoryName,
                        CategoryDescription = addTaskCategoryDto.CategoryDescription,
                        CreatedAt = DateTime.UtcNow,
                        CreatedById = headerCheckResponse.Data.UserId,
                        IsDeleted = false
                    };

                    // Add the entity to the database
                    _dbContext.TaskCategories.Add(taskCategory);
                    await _dbContext.SaveChangesAsync();
                    _log.Info($"Task category added successfully");
                    return await _responseGeneratorService.GenerateResponseAsync(
                        true, StatusCodes.Status201Created, "Task category added successfully.");
                }
                else
                {
                    _log.Error("User is not authorized to add category");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                _log.Error($"Error adding task category : {ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error adding task category : {ex.Message}");
            }
        }

        public async Task<ReturnResponse> EditTaskCategoryAsync(Guid categoryId, EditTaskCategoryDto editTaskCategoryDto, string authorizationToken)
        {
            try
            {
                ReturnResponse<HeaderReturnDto> headerCheckResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
                if (headerCheckResponse.Data.Status)
                {
                    _log.Info("User updating task");
                    // Find the task category by ID
                    var taskCategory = await _dbContext.TaskCategories.FindAsync(categoryId);

                    if (taskCategory == null)
                    {
                        _log.Warn("No category found to update");
                        return await _responseGeneratorService.GenerateResponseAsync(
                            false, StatusCodes.Status404NotFound, "Task category not found.");
                    }

                    // Update properties
                    taskCategory.CategoryName = editTaskCategoryDto.CategoryName;
                    taskCategory.CategoryDescription = editTaskCategoryDto.CategoryDescription;
                    taskCategory.UpdatedAt = DateTime.UtcNow;
                    taskCategory.UpdatedById = headerCheckResponse.Data.UserId;

                    // Save changes to the database
                    await _dbContext.SaveChangesAsync();
                    _log.Info("Task category updated successfully.");
                    return await _responseGeneratorService.GenerateResponseAsync(
                        true, StatusCodes.Status200OK, "Task category updated successfully.");
                }
                else
                {
                    _log.Error("User not authorized to update category");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                _log.Error($"Error updating task category :{ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error updating task category :{ex.Message}");
            }
        }

        public async Task<ReturnResponse> GetTaskCategoryByIdAsync(Guid categoryId, string authorizationToken)
        {
            try
            {
                ReturnResponse<HeaderReturnDto> headerCheckResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
                if (headerCheckResponse.Data.Status)
                {
                    _log.Info("Getting Category by userId");
                    // Find the non-deleted task category by ID
                    var taskCategory = await _dbContext.TaskCategories
                        .Include(tc => tc.CreatedByAppUser)
                        .Include(tc => tc.Tasks)
                        .FirstOrDefaultAsync(tc => tc.CategoryId == categoryId && !tc.IsDeleted);

                    if (taskCategory == null)
                    {
                        // Check if the category is deleted
                        var deletedCategory = await _dbContext.TaskCategories.AnyAsync(tc => tc.CategoryId == categoryId && tc.IsDeleted);

                        if (deletedCategory)
                        {
                            _log.Warn("Category is deleted");
                            return await _responseGeneratorService.GenerateResponseAsync(
                                false, StatusCodes.Status400BadRequest, "Error: Task category is deleted.");
                        }
                        _log.Warn("Task Category not found");
                        return await _responseGeneratorService.GenerateResponseAsync(
                            false, StatusCodes.Status404NotFound, "Task category not found.");
                    }

                    // Get all user IDs needed for the queries
                    var userIds = new List<string>
                {
                    taskCategory.CreatedById
                };

                    // Fetch users in a single query
                    var users = await _dbContext.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();

                    // Map users to a dictionary for easier access
                    var userDictionary = users.ToDictionary(u => u.Id);

                    var taskCategoryDto = new GetAllTaskCategoryDto
                    {
                        CategoryId = taskCategory.CategoryId,
                        CategoryName = taskCategory.CategoryName,
                        CategoryDescription = taskCategory.CategoryDescription,
                        CreatedByName = userDictionary.GetValueOrDefault(taskCategory.CreatedById)?.UserName,
                        UpdatedByName = userDictionary.GetValueOrDefault(taskCategory.UpdatedById ?? "")?.UserName,
                        isDeleted = taskCategory.IsDeleted,
                        Tasks = (taskCategory.Tasks ?? new List<TaskEntity>())
                            .Where(t => !t.IsDeleted) // Filter out deleted tasks
                            .Select(t => new GetAllTaskForTaskCategoryDto
                            {
                                TaskId = t.TaskId,
                                TaskName = t.TaskName,
                                TaskPriority = t.TaskPriority.ToString(),
                                Status = t.Status.ToString(),
                                TaskDescription = t.TaskDescription,
                                AssignedBy = t.AssignedById,
                                DueDate = t.DueDate
                            }).ToList() ?? new List<GetAllTaskForTaskCategoryDto>()
                    };

                    _log.Info($"Got a taskcategory successfully");
                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Got a taskcategory successfully", taskCategoryDto);
                }
                else
                {
                    _log.Error("User not authorized to get a category");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }

            }
            catch (Exception ex)
            {
                // Handle exceptions
                _log.Error($"Error occured while getting a category:{ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error getting task category by ID: {ex.Message}");
            }
        }

        public async Task<ReturnResponse> GetAllTaskCategoriesAsync(string authorizationToken)
        {
            try
            {
                ReturnResponse<HeaderReturnDto> headerCheckResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
                if (headerCheckResponse.Data.Status)
                {
                    _log.Info("Gettng all category");
                    // Get all non-deleted task categories
                    var taskCategories = await _dbContext.TaskCategories
                        .Include(tc => tc.CreatedByAppUser)
                        .Include(tc => tc.UpdatedByAppUser)
                        .Include(tc => tc.DeletedByAppUser)
                        .Include(tc => tc.Tasks)
                        .Where(tc => !tc.IsDeleted)
                        .ToListAsync();

                    if (!taskCategories.Any())
                    {
                        // No non-deleted task categories found
                        _log.Error("No category data found");
                        return await _responseGeneratorService.GenerateResponseAsync(
                            false, StatusCodes.Status404NotFound, "No Category data found.");
                    }

                    // Get all user IDs needed for the queries
                    var userIds = taskCategories
                        .SelectMany(tc => new List<string?> { tc.CreatedById, tc.UpdatedById, tc.DeletedById })
                        .Where(id => id != null)
                        .Distinct()
                        .ToList();

                    // Fetch users in a single query
                    var users = await _dbContext.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();

                    // Map users to a dictionary for easier access
                    var userDictionary = users.ToDictionary(u => u.Id);

                    // Convert entities to DTOs
                    var taskCategoriesDto = taskCategories.Select(tc => new GetAllTaskCategoryDto
                    {
                        CategoryId = tc.CategoryId,
                        CategoryName = tc.CategoryName,
                        CategoryDescription = tc.CategoryDescription,
                        CreatedByName = userDictionary.GetValueOrDefault(tc.CreatedById)?.UserName,
                        UpdatedByName = userDictionary.GetValueOrDefault(tc.UpdatedById ?? "")?.UserName,
                        isDeleted = tc.IsDeleted,
                        Tasks = (tc.Tasks ?? new List<TaskEntity>()).Where(t => !t.IsDeleted).Select(t => new GetAllTaskForTaskCategoryDto
                        {
                            TaskId = t.TaskId,
                            TaskName = t.TaskName,
                            TaskPriority = t.TaskPriority.ToString(),
                            Status = t.Status.ToString(),
                            TaskDescription = t.TaskDescription,
                            AssignedBy = t.AssignedById,
                            DueDate = t.DueDate
                        }).ToList()
                    }).ToList();
                    _log.Info($"Category List found successfully");
                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Category List found successfully", taskCategoriesDto);
                }
                else
                {
                    _log.Error($"User not authorized to get a list");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }

            }
            catch (Exception ex)
            {
                // Handle exceptions
                _log.Error($"Error occured while getting all categories:{ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error getting all non-deleted task categories: {ex.Message}");
            }
        }



        public async Task<ReturnResponse> DeleteTaskCategoryAsync(Guid categoryId, string authorizationToken)
        {    
            try
            {
                ReturnResponse<HeaderReturnDto> headerCheckResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
                if (headerCheckResponse.Data.Status)
                {
                    _log.Info("Deleting category");
                    // Find the task category by ID
                    var taskCategory = await _dbContext.TaskCategories.FindAsync(categoryId);

                    if (taskCategory == null)
                    {
                        _log.Warn("Task category not found");
                        return await _responseGeneratorService.GenerateResponseAsync(
                            false, StatusCodes.Status404NotFound, "Task category not found.");
                    }

                    // Soft delete by updating IsDeleted and related properties
                    taskCategory.IsDeleted = true;
                    taskCategory.UpdatedAt = DateTime.UtcNow;
                    taskCategory.UpdatedById = headerCheckResponse.Data.UserId;
                    taskCategory.DeletedById = headerCheckResponse.Data.UserId;

                    // Save changes to the database
                    await _dbContext.SaveChangesAsync();
                    _log.Info("CAtegory deleted successfully");
                    return await _responseGeneratorService.GenerateResponseAsync(
                        true, StatusCodes.Status200OK, "Task category deleted successfully.");
                }
                else
                {
                    _log.Error("User not authorized to delete categpory");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                _log.Error($"Error occured while deleting a category:{ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error deleting task category :{ex.Message}");
            }
        }

       

    }
}
