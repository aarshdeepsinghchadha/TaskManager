using Application.Common;
using Application.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface ITaskService
    {
        Task<ReturnResponse> AddTaskAsync(AddTaskDto taskDto, string authorizationToken);
        Task<ReturnResponse> EditTaskAsync(Guid taskId, EditTaskDto taskDto , string authorizationToken);
        Task<ReturnResponse> GetTaskByIdAsync(Guid taskId , string authorizationToken);
        Task<ReturnResponse> GetAllTasksAsync(string authorizationToken);
        Task<ReturnResponse> DeleteTaskAsync(Guid taskId, string authorizationToken);
        Task<PagedResponse<List<GetTaskListingDto>>> GetTaskListing(string sortColumn, int sortDirection , int pageNumber , int pageSize , string authorizationToken);
    }
}
