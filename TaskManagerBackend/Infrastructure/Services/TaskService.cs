using Application.Common;
using Application.Interface;
using Application.Task;
using Application.TaskManager;
using Domain;
using Domain.Enum;
using log4net;
using log4net.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistance;

namespace Infrastructure.Services
{
    public class TaskService : ITaskService
    {
        private readonly DataContext _dbContext;
        private readonly IResponseGeneratorService _responseGeneratorService;
        private readonly ITokenService _tokenService;
        private readonly ILog _log;
        
        public TaskService(DataContext dbContext, IResponseGeneratorService responseGeneratorService, ITokenService tokenService)
        {
            _dbContext = dbContext;
            _responseGeneratorService = responseGeneratorService;
            _tokenService = tokenService;
            _log = LogManager.GetLogger(typeof(TaskService));
            
        }

        public async Task<PagedResponse<List<GetTaskListingDto>>> GetTaskListing(string sortColumn, int sortDirection , int pageNumber , int pageSize, string authorizationToken)
        {
           
            try
            {
                ReturnResponse<HeaderReturnDto> headerCheckResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
                if(headerCheckResponse.Data.Status)
                {
                    _log.Info($"Getting task list based on {sortColumn}");
                    pageNumber = Math.Max(1, pageNumber);
                    pageSize = Math.Min(5, pageSize);

                    int totalRecords = pageSize / pageNumber;

                    var tasksEntities = _dbContext.TaskEntities
                        .Include(t => t.TaskCategory)
                        .Include(t => t.Comments)
                        .Include(t => t.AssignedByAppUser)
                        .Where(t => !t.IsDeleted)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize);



                    IOrderedQueryable<TaskEntity> sortedTasks;

                    sortColumn.ToLower();
                    sortColumn.Trim();

                    switch (sortColumn)
                    {
                        case "taskpriority":

                            if (sortDirection == -1)
                            {
                                sortedTasks = tasksEntities.OrderBy(t => (int)t.TaskPriority);
                            }
                            else if (sortDirection == 1)
                            {
                                sortedTasks = tasksEntities.OrderByDescending(t => (int)t.TaskPriority);
                            }
                            else
                            {
                                // Handle invalid sortDirection value
                                return await _responseGeneratorService.GenerateResponseAsync<List<GetTaskListingDto>>(false, StatusCodes.Status204NoContent, "Invalid SortDirection", null, pageNumber, pageSize, 0);
                            }

                            break;

                        case "commentsontask":
                            if (sortDirection == -1)
                            {
                                sortedTasks = tasksEntities.OrderBy(t => t.Comments.Count).ThenBy(t => t.TaskName);
                            }
                            else if (sortDirection == 1)
                            {
                                sortedTasks = tasksEntities.OrderByDescending(t => t.Comments.Count).ThenByDescending(t => t.TaskName);
                            }
                            else
                            {
                                return await _responseGeneratorService.GenerateResponseAsync<List<GetTaskListingDto>>(false, StatusCodes.Status204NoContent, "Invalid SortDirection", null, pageNumber, pageSize, 0);
                            }

                            break;

                        case "status":
                            if (sortDirection == -1)
                            {
                                sortedTasks = tasksEntities.OrderBy(t => (int)t.Status);
                            }
                            else if (sortDirection == 1)
                            {
                                sortedTasks = tasksEntities.OrderByDescending(t => (int)t.Status);
                            }
                            else
                            {
                                // Handle other cases if needed
                                return await _responseGeneratorService.GenerateResponseAsync<List<GetTaskListingDto>>(false, StatusCodes.Status204NoContent, "Invalid SortDirection", null, pageNumber, pageSize, 0);
                            }
                            break;

                        case "taskname":
                            if (sortDirection == -1)
                            {
                                sortedTasks = tasksEntities.OrderBy(t => t.TaskName);
                            }
                            else if (sortDirection == 1)
                            {
                                sortedTasks = tasksEntities.OrderByDescending(t => t.TaskName);
                            }
                            else
                            {
                                return await _responseGeneratorService.GenerateResponseAsync<List<GetTaskListingDto>>(false, StatusCodes.Status204NoContent, "Invalid SortDirection", null, pageNumber, pageSize, 0);
                            }
                            break;

                        case "duedate":
                            if (sortDirection == -1)
                            {
                                sortedTasks = tasksEntities.OrderBy(t => t.DueDate).ThenBy(t => t.TaskName);
                            }
                            else if (sortDirection == 1)
                            {
                                sortedTasks = tasksEntities.OrderByDescending(t => t.DueDate).ThenByDescending(t => t.TaskName);
                            }
                            else
                            {
                                return await _responseGeneratorService.GenerateResponseAsync<List<GetTaskListingDto>>(false, StatusCodes.Status204NoContent, "Invalid SortDirection", null, pageNumber, pageSize, 0); ;
                            }
                            break;

                        default:
                            // Handle invalid sortColumn value
                            _log.Error($"{sortColumn} not found while filtring task list");
                            return await _responseGeneratorService.GenerateResponseAsync<List<GetTaskListingDto>>(false, StatusCodes.Status204NoContent, "SortColumn Not Found", null, pageNumber, pageSize, 0);
                    }

                    var tasksDto = await sortedTasks.Select(t => new GetTaskListingDto
                    {
                        TaskName = t.TaskName,
                        TaskPriority = t.TaskPriority.ToString(),
                        TaskCategoryName = t.TaskCategory != null ? t.TaskCategory.CategoryName : "No Category ",
                        CommentsOnTask = t.Comments.Count(),
                        UserAssignedTo = t.AssignedById != null ? 1 : 0,
                        Status = t.Status.ToString(),
                        DueDate = t.DueDate,
                    }).ToListAsync();

                    _log.Info($"Task List sorted successfully based on {sortColumn}");
                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Task List sorted successfully", tasksDto, pageNumber, pageSize, totalRecords);
                }
                else
                {
                    _log.Error("User not authorized to filter task list");
                    return await _responseGeneratorService.GenerateResponseAsync<List<GetTaskListingDto>>(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!",null,0,0,0);
                }


            }
            catch (Exception ex)
            {
                _log.Error($"Error occured while filtering a task list :{ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync<List<GetTaskListingDto>>(false,StatusCodes.Status404NotFound,ex.Message,null, 0,0,0);
            }
        }

        public async Task<ReturnResponse> AddTaskAsync(AddTaskDto taskDto, string authorizationToken)
        {
            
            try
            {
                ReturnResponse<HeaderReturnDto> headerCheckResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
                if (headerCheckResponse.Data.Status)
                {
                    _log.Info("User adding task");
                    var taskEntity = new TaskEntity
                    {
                        TaskName = taskDto.TaskName,
                        TaskPriority = (TaskPriorityStatus)taskDto.TaskPriority,
                        Status = (TaskStatusEnum)taskDto.Status,
                        TaskDescription = taskDto.TaskDescription,
                        CategoryId = taskDto.CategoryId,
                        AssignedById = taskDto.AssignedBy,
                        DueDate = taskDto.DueDate,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedById = headerCheckResponse.Data.UserId,
                        IsDeleted = false // Assuming a new task is not deleted by default
                    };

                    _dbContext.TaskEntities.Add(taskEntity);
                    await _dbContext.SaveChangesAsync();

                    _log.Info("Task added successfully");
                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status201Created, "Task added successfully");
                }
                else
                {
                    _log.Error("User not authorized to add task");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                _log.Error($"Error occured while adding task:{ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error adding task: {ex.Message}");
            }
        }

        public async Task<ReturnResponse> EditTaskAsync(Guid taskId, EditTaskDto taskDto, string authorizationToken)
        {
           
            try
            {
                ReturnResponse<HeaderReturnDto> headerCheckResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
                if(headerCheckResponse.Data.Status)
                {
                    _log.Info("User updating task");
                    var existingTask = await _dbContext.TaskEntities.FindAsync(taskId);

                    if (existingTask == null)
                    {
                        _log.Warn("Task not found");
                        return await _responseGeneratorService.GenerateResponseAsync(
                            false, StatusCodes.Status404NotFound, "Task not found");
                    }

                    existingTask.TaskName = taskDto.TaskName;
                    existingTask.TaskPriority = (TaskPriorityStatus)taskDto.TaskPriority;
                    existingTask.Status = (TaskStatusEnum)taskDto.Status;
                    existingTask.TaskDescription = taskDto.TaskDescription;
                    existingTask.CategoryId = taskDto.CategoryId;
                    existingTask.AssignedById = taskDto.AssignedBy;
                    existingTask.DueDate = taskDto.DueDate;
                    existingTask.UpdatedAt = DateTime.UtcNow;
                    existingTask.UpdatedById = headerCheckResponse.Data.UserId;

                    await _dbContext.SaveChangesAsync();

                    _log.Info("Task updated successfully");
                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Task updated successfully");
                }
                else
                {
                    _log.Error("User not authorized to update task");
                     return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }
                
               
            }
            catch (Exception ex)
            {
                // Handle exceptions
                _log.Error($"Error occured while updating task:{ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error updating task: {ex.Message}");
            }
        }

        public async Task<ReturnResponse> GetTaskByIdAsync(Guid taskId , string authorizationToken)
        {
            try
            {
                ReturnResponse<HeaderReturnDto> checkHeaderResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
                if (checkHeaderResponse.Data.Status)
                {
                    _log.Info("User getting a task by id");
                    var taskEntity = await _dbContext.
                    TaskEntities.Include(t => t.CreatedByAppUser)
                    .Include(t => t.UpdatedByAppUser)
                    .Include(t => t.TaskCategory)
                    .Include(t => t.Comments) // Include Comments directly here
                    .ThenInclude(c => c.CommentedAppUser)
                    .FirstOrDefaultAsync(t => t.TaskId == taskId && !t.IsDeleted);


                    if (taskEntity == null)
                    {
                        _log.Warn("Task not found");
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
                        TaskPriority = taskEntity.TaskPriority.ToString(),
                        Status = (int)taskEntity.Status,
                        TaskDescription = taskEntity.TaskDescription,
                        CategoryId = taskEntity.CategoryId,
                        AssignedBy = taskEntity.AssignedById,
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
                    _log.Info($"Task found successfully");
                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Task found successfully", taskDetailsDto);
                }
                else
                {
                    _log.Error("User not authorized to Get Task");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                _log.Error($"Error occured while getting a task:{ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error getting task by ID: {ex.Message}");
            }
        }

        public async Task<ReturnResponse> GetAllTasksAsync(string authorizationToken)
        {
            try
            {
                ReturnResponse<HeaderReturnDto> checkHeaderResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
                if (checkHeaderResponse.Data.Status)
                {
                    _log.Info("User getting task list");

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
                        _log.Warn("No task Found");
                        return await _responseGeneratorService.GenerateResponseAsync(
                            false, StatusCodes.Status404NotFound, "No task found");
                    }

                    var userIds = tasksEntities
                        .SelectMany(t => new List<string> { t.CreatedById, t.UpdatedById })
                        .Distinct()
                        .ToList();

                    var userDictionary = await GetUserDictionaryAsync(userIds);

                    var tasksDto = tasksEntities.Select(t => new GetAllTaskDto
                    {
                        TaskName = t.TaskName,
                        TaskPriority = t.TaskPriority.ToString(),
                        Status = (int)t.Status,
                        TaskDescription = t.TaskDescription,
                        CategoryId = t.CategoryId,
                        AssignedBy = t.AssignedById,
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
                    _log.Info($"Task List found successfully");
                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Task List found successfully", tasksDto);
                }
                else
                {
                    _log.Error("User not authorized to get task list");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                _log.Error($"Error occured while getting all task:{ex.Message}");
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

        public async Task<ReturnResponse> DeleteTaskAsync(Guid taskId, string authorizationToken)
        {
            try
            {
                ReturnResponse<HeaderReturnDto> checkHeaderResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
                if (checkHeaderResponse.Data.Status)
                {
                    _log.Info("Deleting Task");
                    // Find the task by ID
                    var taskEntity = await _dbContext.TaskEntities.FindAsync(taskId);

                    if (taskEntity == null)
                    {
                        _log.Warn("Task not found");
                        return await _responseGeneratorService.GenerateResponseAsync(
                            false, StatusCodes.Status404NotFound, "Task not found");
                    }

                    // Soft delete the task (mark as deleted)
                    taskEntity.IsDeleted = true;
                    taskEntity.UpdatedAt = DateTime.UtcNow;
                    taskEntity.UpdatedById = checkHeaderResponse.Data.UserId;
                    taskEntity.DeletedById = checkHeaderResponse.Data.UserId;

                    // Update the database
                    _dbContext.Update(taskEntity);
                    await _dbContext.SaveChangesAsync();
                    _log.Info("Task deleted successfully");
                    // Return success response
                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Task deleted successfully");
                }
                else
                {
                    _log.Error("User not authorized to delete task");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                _log.Error($"Error occured while deleting task:{ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error deleting task: {ex.Message}");
            }
        }

       
    }

}
