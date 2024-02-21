using Application.Comment;
using Application.Common;
using Application.Interface;
using Application.Task;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistance;

namespace Infrastructure.Services
{
    public class CommentService : ICommentService
    {
        private readonly DataContext _dbContext;
        private readonly IResponseGeneratorService _responseGeneratorService;

        public CommentService(DataContext dbContext, IResponseGeneratorService responseGeneratorService)
        {
            _dbContext = dbContext;
            _responseGeneratorService = responseGeneratorService;
        }

        public async Task<ReturnResponse> AddCommentAsync(AddCommentDto commentDto)
        {
            try
            {
                var commentEntity = new Comment
                {
                    CommentDescription = commentDto.CommentDescription,
                    CommentedById = commentDto.CommentedById,
                    TaskId = commentDto.TaskId,
                    CreatedAt = DateTime.Now,
                    CreatedById = commentDto.UserId,
                    IsDeleted = false
                };

                _dbContext.Comments.Add(commentEntity);
                await _dbContext.SaveChangesAsync();

                // Return success response
                return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status201Created, "Comment added successfully");
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error adding comment: {ex.Message}");
            }
        }

        public async Task<ReturnResponse> UpdateCommentAsync(Guid commentId, EditCommentDto commentDto)
        {
            try
            {
                var commentEntity = await _dbContext.Comments
                    .FirstOrDefaultAsync(c => c.CommentId == commentId && !c.IsDeleted);

                if (commentEntity == null)
                {
                    return await _responseGeneratorService.GenerateResponseAsync(
                        false, StatusCodes.Status404NotFound, "Comment not found.");
                }

                commentEntity.CommentDescription = commentDto.CommentDescription;
                commentEntity.CommentedById = commentDto.CommentedById;
                commentEntity.TaskId = commentDto.TaskId;
                commentEntity.UpdatedAt = DateTime.Now;
                commentEntity.UpdatedById = commentDto.UserId;

                await _dbContext.SaveChangesAsync();

                // Return success response
                return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Comment updated successfully");
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error updating comment: {ex.Message}");
            }
        }

        public async Task<ReturnResponse> GetCommentByIdAsync(Guid commentId)
        {
            try
            {
                var commentEntity = await _dbContext.Comments
                    .Include(c => c.CommentedTask)
                        .ThenInclude(t => t.TaskCategory)
                        .Include(c => c.CreatedByAppUser)
                        .Include(c => c.UpdatedByAppUser)
                        .Include(c => c.CommentedAppUser)
                    .FirstOrDefaultAsync(c => c.CommentId == commentId && !c.IsDeleted && c.CommentedTask != null && !c.CommentedTask.IsDeleted);

                if (commentEntity == null)
                {
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
                        AssignedBy = commentEntity.CommentedTask.AssignedBy,
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

                return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Success", commentDetailsDto);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error getting comment by ID: {ex.Message}");
            }
        }




        public async Task<ReturnResponse> GetAllCommentsAsync()
        {
            try
            {
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
                        AssignedBy = c.CommentedTask.AssignedBy,
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

                return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Success", commentsDto);
            }
            catch (Exception ex)
            {
                // Handle exceptions
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


        public async Task<ReturnResponse> DeleteCommentAsync(Guid commentId, string userId)
        {
            try
            {
                var commentEntity = await _dbContext.Comments
                    .FirstOrDefaultAsync(c => c.CommentId == commentId && !c.IsDeleted);

                if (commentEntity == null)
                {
                    return await _responseGeneratorService.GenerateResponseAsync(
                        false, StatusCodes.Status404NotFound, "Comment not found.");
                }

                commentEntity.IsDeleted = true;
                commentEntity.UpdatedAt = DateTime.UtcNow;
                commentEntity.UpdatedById = userId;
                commentEntity.DeletedBy = userId;

                await _dbContext.SaveChangesAsync();

                // Return success response
                return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Comment deleted successfully");
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"Error deleting comment: {ex.Message}");
            }
        }
    }

}
