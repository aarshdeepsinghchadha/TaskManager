using Application.Common;
using Application.TaskCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface ITaskCategoryService
    {
        Task<ReturnResponse> AddTaskCategoryAsync(AddTaskCategoryDto addTaskCategoryDto, string authorizationToken);
        Task<ReturnResponse> EditTaskCategoryAsync(Guid categoryId, EditTaskCategoryDto editTaskCategoryDto);
        Task<ReturnResponse> GetTaskCategoryByIdAsync(Guid categoryId);
        Task<ReturnResponse> GetAllTaskCategoriesAsync(string authorizationToken);
        Task<ReturnResponse> DeleteTaskCategoryAsync(Guid categoryId, string userId);
    }

}
