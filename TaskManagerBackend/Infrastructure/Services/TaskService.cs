using Application.Common;
using Application.Interface;
using Application.Task;
using Domain;
using Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistance;

namespace Infrastructure.Services
{
    public class TaskService : ITaskService
    {
        private readonly DataContext _dbContext;
        private readonly IResponseGeneratorService _responseGeneratorService;

        public TaskService(DataContext dbContext, IResponseGeneratorService responseGeneratorService)
        {
            _dbContext = dbContext;
            _responseGeneratorService = responseGeneratorService;
        }

        public async Task<ReturnResponse> AddTaskAsync(AddTaskDto taskDto)
        {
            try
            {
                var taskEntity = new TaskEntity
                {
                    TaskName = taskDto.TaskName,
                    TaskPriority = (TaskPriorityStatus)taskDto.TaskPriority,
                    Status = (TaskStatusEnum)taskDto.Status,
                    TaskDescription = taskDto.TaskDescription,
                    CategoryId = taskDto.CategoryId,
                    AssignedBy = taskDto.AssignedBy,
                    DueDate = taskDto.DueDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedById = taskDto.UserId,
                    IsDeleted = false // Assuming a new task is not deleted by default
                };

                _dbContext.TaskEntities.Add(taskEntity);
                await _dbContext.SaveChangesAsync();

                return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status201Created, "Task added successfully");
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error adding task: {ex.Message}");
            }
        }

        public async Task<ReturnResponse> UpdateTaskAsync(Guid taskId, EditTaskDto taskDto)
        {
            try
            {
                var existingTask = await _dbContext.TaskEntities.FindAsync(taskId);

                if (existingTask == null)
                {
                    return await _responseGeneratorService.GenerateResponseAsync(
                        false, StatusCodes.Status404NotFound, "Task not found");
                }

                existingTask.TaskName = taskDto.TaskName;
                existingTask.TaskPriority = (TaskPriorityStatus)taskDto.TaskPriority;
                existingTask.Status = (TaskStatusEnum)taskDto.Status;
                existingTask.TaskDescription = taskDto.TaskDescription;
                existingTask.CategoryId = taskDto.CategoryId;
                existingTask.AssignedBy = taskDto.AssignedBy;
                existingTask.DueDate = taskDto.DueDate;
                existingTask.UpdatedAt = DateTime.UtcNow;
                existingTask.UpdatedById = taskDto.UserId;

                await _dbContext.SaveChangesAsync();

                return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Task updated successfully");
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error updating task: {ex.Message}");
            }
        }

        public async Task<ReturnResponse> GetTaskByIdAsync(Guid taskId)
        {
            try
            {
                var taskEntity = await _dbContext.TaskEntities
           .Include(t => t.CreatedByAppUser)
           .Include(t => t.UpdatedByAppUser)
           .Include(t => t.TaskCategory)
           .Include(t => t.Comments) // Include Comments directly here
               .ThenInclude(c => c.CommentedAppUser)
           .FirstOrDefaultAsync(t => t.TaskId == taskId && !t.IsDeleted);

                if (taskEntity == null)
                {
                    return await _responseGeneratorService.GenerateResponseAsync(
                        false, StatusCodes.Status404NotFound, "Task not found");
                }

                var userIds = new List<string>
                {
                    taskEntity.CreatedById,
                    taskEntity.UpdatedById
                };

                var userDictionary = await GetUserDictionaryAsync(userIds);

                var taskDetailsDto = new GetAllTaskDto
                {
                    TaskName = taskEntity.TaskName,
                    TaskPriority = (int)taskEntity.TaskPriority,
                    Status = (int)taskEntity.Status,
                    TaskDescription = taskEntity.TaskDescription,
                    CategoryId = taskEntity.CategoryId,
                    AssignedBy = taskEntity.AssignedBy,
                    DueDate = taskEntity.DueDate,
                    CreatedbyName = userDictionary.GetValueOrDefault(taskEntity.CreatedById)?.UserName,
                    UpdatedbyName = userDictionary.GetValueOrDefault(taskEntity.UpdatedById ?? "")?.UserName, // Handle null UpdatedById
                    TaskCategory = new TaskCategoryDetailsDto
                    {
                        CategoryName = taskEntity.TaskCategory?.CategoryName,
                        CategoryDescription = taskEntity.TaskCategory?.CategoryDescription
                    },
                    Comments = taskEntity.Comments?.Select(c => new TaskCommentList
                    {
                        CommentDescription = c.CommentDescription,
                        CommentedById = c.CommentedById,
                        CommentedByName = userDictionary.GetValueOrDefault(c.CommentedById)?.UserName
                    }).ToList()
                };


                return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Success", taskDetailsDto);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error getting task by ID: {ex.Message}");
            }
        }

        public async Task<ReturnResponse> GetAllTasksAsync()
        {
            try
            {
                var tasksEntities = await _dbContext.TaskEntities
                    .Include(t => t.CreatedByAppUser)
                    .Include(t => t.UpdatedByAppUser)
                    .Include(t => t.TaskCategory)
                    .Include(t => t.Comments)
                    .Where(t => !t.IsDeleted)
                    .ToListAsync();


                if (!tasksEntities.Any())
                {
                    // No non-deleted task categories found
                    return await _responseGeneratorService.GenerateResponseAsync(
                        false, StatusCodes.Status404NotFound, "No data.");
                }

                var userIds = tasksEntities
                    .SelectMany(t => new List<string> { t.CreatedById, t.UpdatedById })
                    .Distinct()
                    .ToList();

                var userDictionary = await GetUserDictionaryAsync(userIds);

                var tasksDto = tasksEntities.Select(t => new GetAllTaskDto
                {
                    TaskName = t.TaskName,
                    TaskPriority = (int)t.TaskPriority,
                    Status = (int)t.Status,
                    TaskDescription = t.TaskDescription,
                    CategoryId = t.CategoryId,
                    AssignedBy = t.AssignedBy,
                    DueDate = t.DueDate,
                    CreatedbyName = userDictionary.GetValueOrDefault(t.CreatedById)?.UserName,
                    UpdatedbyName = userDictionary.GetValueOrDefault(t.UpdatedById ?? "")?.UserName, // Handle null UpdatedById
                    TaskCategory = new TaskCategoryDetailsDto
                    {
                        CategoryName = t.TaskCategory?.CategoryName,
                        CategoryDescription = t.TaskCategory?.CategoryDescription
                    },
                    Comments = t.Comments?.Select(c => new TaskCommentList
                    {
                        CommentDescription = c.CommentDescription,
                        CommentedById = c.CommentedById,
                        CommentedByName = userDictionary.GetValueOrDefault(c.CommentedById)?.UserName
                    }).ToList()
                }).ToList();

                return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Success", tasksDto);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error getting all tasks: {ex.Message}");
            }
        }

        private async Task<Dictionary<string, AppUser>> GetUserDictionaryAsync(List<string> userIds)
        {
            var users = await _dbContext.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            return users;
        }

        public async Task<ReturnResponse> DeleteTaskAsync(Guid taskId, string userId)
        {
            try
            {
                // Find the task by ID
                var taskEntity = await _dbContext.TaskEntities.FindAsync(taskId);

                if (taskEntity == null)
                {
                    return await _responseGeneratorService.GenerateResponseAsync(
                        false, StatusCodes.Status404NotFound, "Task not found");
                }

                // Soft delete the task (mark as deleted)
                taskEntity.IsDeleted = true;
                taskEntity.UpdatedAt = DateTime.UtcNow;
                taskEntity.UpdatedById = userId;
                taskEntity.DeletedBy = userId;

                // Update the database
                 _dbContext.Update(taskEntity);
                await _dbContext.SaveChangesAsync();

                // Return success response
                return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Task deleted successfully");
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error deleting task: {ex.Message}");
            }
        }

    }

}
