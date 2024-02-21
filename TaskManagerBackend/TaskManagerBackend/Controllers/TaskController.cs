using Application.Common;
using Application.Interface;
using Application.Task;
using Application.TaskCategory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace TaskManagerBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
       

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
            
        }

        [HttpGet("GetTaskList")]
        public async Task<IActionResult> GetTaskListing([FromQuery(Name = "SortColumn")] string SortColumn, [FromQuery(Name = "SortDirection")] int SortDirection ,[FromQuery] int pageNumber , [FromQuery] int pageSize , [FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _taskService.GetTaskListing(SortColumn, SortDirection ,pageNumber,pageSize,authorizationToken);
            return StatusCode(200, result);
        }

        
        [HttpPost("Task")]
        public async Task<IActionResult> AddTask([FromBody] AddTaskDto addTaskDto, [FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _taskService.AddTaskAsync(addTaskDto, authorizationToken);
            return StatusCode(result.StatusCode, result);
        }

        
        [HttpPut("Task/{taskId}")]
        public async Task<IActionResult> EditTask(Guid taskId, [FromBody] EditTaskDto editTaskDto, [FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _taskService.EditTaskAsync(taskId, editTaskDto, authorizationToken);
            return StatusCode(result.StatusCode, result);
        }

       
        [HttpGet("Task/{taskId}")]
        public async Task<IActionResult> GetTaskById(Guid taskId, [FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _taskService.GetTaskByIdAsync(taskId, authorizationToken);
            return StatusCode(result.StatusCode, result);
        }

        
        [HttpGet("Task")]
        public async Task<IActionResult> GetAllTask([FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _taskService.GetAllTasksAsync(authorizationToken);
            return StatusCode(result.StatusCode, result);
        }
  
        [HttpDelete("Task/{taskId}")]
        public async Task<IActionResult> DeleteTask(Guid taskId, [FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _taskService.DeleteTaskAsync(taskId, authorizationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
