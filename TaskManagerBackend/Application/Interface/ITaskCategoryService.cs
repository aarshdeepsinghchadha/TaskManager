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
        Task<ReturnResponse> AddTaskCategoryAsync(AddTaskCategoryDto addTaskCategoryDto , string authorizationToken);
        Task<ReturnResponse> EditTaskCategoryAsync(Guid categoryId, EditTaskCategoryDto editTaskCategoryDto , string authorizationToken);
        Task<ReturnResponse> GetTaskCategoryByIdAsync(Guid categoryId , string authorizationToken);
        Task<ReturnResponse> GetAllTaskCategoriesAsync(string authorizationToken);
        Task<ReturnResponse> DeleteTaskCategoryAsync(Guid categoryId, string authorizationToken);
        Task<PagedResponse<List<GetTaskCategoryListingDto>>> GetTaskCategoryListing(string sortColumn, int sortDirection , int pageNumber , int pageSize , string authorizationToken);
    }

}
