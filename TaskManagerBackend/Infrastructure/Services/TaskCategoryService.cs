using Application;
using Application.Common;
using Application.Interface;
using Application.TaskCategory;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
        private readonly ICheckHeaderService _checkHeaderService;

        public TaskCategoryService(IResponseGeneratorService responseGeneratorService, DataContext dbContext, ICheckHeaderService checkHeaderService)
        {
            _responseGeneratorService = responseGeneratorService;
            _dbContext = dbContext;
            _checkHeaderService = checkHeaderService;
        }

        public async Task<ReturnResponse> AddTaskCategoryAsync(AddTaskCategoryDto addTaskCategoryDto, string authorizationToken)
        {
            try
            {
                ReturnResponse<HeaderReturnDto> headerCheckResponse = await _checkHeaderService.CheckPassedUserToken(authorizationToken);
                if (headerCheckResponse.Data.Status)
                {
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

                    return await _responseGeneratorService.GenerateResponseAsync(
                        true, StatusCodes.Status201Created, "Task category added successfully.");
                }
                else
                {
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error adding task category : {ex.Message}");
            }
        }

        public async Task<ReturnResponse> EditTaskCategoryAsync(Guid categoryId, EditTaskCategoryDto editTaskCategoryDto)
        {
            try
            {
                // Find the task category by ID
                var taskCategory = await _dbContext.TaskCategories.FindAsync(categoryId);

                if (taskCategory == null)
                {
                    return await _responseGeneratorService.GenerateResponseAsync(
                        false, StatusCodes.Status404NotFound, "Task category not found.");
                }

                // Update properties
                taskCategory.CategoryName = editTaskCategoryDto.CategoryName;
                taskCategory.CategoryDescription = editTaskCategoryDto.CategoryDescription;
                taskCategory.UpdatedAt = DateTime.UtcNow;
                taskCategory.UpdatedById = editTaskCategoryDto.UserId;

                // Save changes to the database
                await _dbContext.SaveChangesAsync();

                return await _responseGeneratorService.GenerateResponseAsync(
                    true, StatusCodes.Status200OK, "Task category updated successfully.");
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error updating task category :{ex.Message}");
            }
        }

        public async Task<ReturnResponse> GetTaskCategoryByIdAsync(Guid categoryId)
        {
            try
            {
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
                        return await _responseGeneratorService.GenerateResponseAsync(
                            false, StatusCodes.Status400BadRequest, "Error: Task category is deleted.");
                    }

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
                            AssignedBy = t.AssignedBy,
                            DueDate = t.DueDate
                        }).ToList() ?? new List<GetAllTaskForTaskCategoryDto>()
                };

                return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Success", taskCategoryDto);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error getting task category by ID: {ex.Message}");
            }
        }




        public async Task<ReturnResponse> GetAllTaskCategoriesAsync(string authorizationToken)
        {
            try
            {
                ReturnResponse<HeaderReturnDto> headerCheckResponse = await _checkHeaderService.CheckPassedUserToken(authorizationToken);
                if (headerCheckResponse.Data.Status)
                {
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
                        return await _responseGeneratorService.GenerateResponseAsync(
                            false, StatusCodes.Status404NotFound, "No data.");
                    }

                    // Get all user IDs needed for the queries
                    var userIds = taskCategories
                        .SelectMany(tc => new List<string?> { tc.CreatedById, tc.UpdatedById, tc.DeletedBy })
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
                            AssignedBy = t.AssignedBy,
                            DueDate = t.DueDate
                        }).ToList()
                    }).ToList();

                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Success", taskCategoriesDto);
                }
                else
                {
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error getting all non-deleted task categories: {ex.Message}");
            }
        }





        public async Task<ReturnResponse> DeleteTaskCategoryAsync(Guid categoryId, string userId)
        {
            try
            {
                // Find the task category by ID
                var taskCategory = await _dbContext.TaskCategories.FindAsync(categoryId);

                if (taskCategory == null)
                {
                    return await _responseGeneratorService.GenerateResponseAsync(
                        false, StatusCodes.Status404NotFound, "Task category not found.");
                }

                // Soft delete by updating IsDeleted and related properties
                taskCategory.IsDeleted = true;
                taskCategory.UpdatedAt = DateTime.UtcNow;
                taskCategory.UpdatedById = userId;
                taskCategory.DeletedBy = userId;

                // Save changes to the database
                await _dbContext.SaveChangesAsync();

                return await _responseGeneratorService.GenerateResponseAsync(
                    true, StatusCodes.Status200OK, "Task category deleted successfully.");
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error deleting task category :{ex.Message}");
            }
        }
    }
}
