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
        Task<ReturnResponse> AddTaskAsync(AddTaskDto taskDto);
        Task<ReturnResponse> UpdateTaskAsync(Guid taskId, EditTaskDto taskDto);
        Task<ReturnResponse> GetTaskByIdAsync(Guid taskId);
        Task<ReturnResponse> GetAllTasksAsync();
        Task<ReturnResponse> DeleteTaskAsync(Guid taskId, string userId);
    }
}
