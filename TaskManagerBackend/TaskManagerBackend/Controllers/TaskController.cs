using Application.Interface;
using Application.Task;
using Application.TaskCategory;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagerBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpPost]
        public async Task<IActionResult> AddTask([FromBody] AddTaskDto addTaskDto)
        {
            var result = await _taskService.AddTaskAsync(addTaskDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{taskId}")]
        public async Task<IActionResult> EditTask(Guid taskId, [FromBody] EditTaskDto editTaskDto)
        {
            var result = await _taskService.UpdateTaskAsync(taskId, editTaskDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTaskById(Guid taskId)
        {
            var result = await _taskService.GetTaskByIdAsync(taskId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTask()
        {
            var result = await _taskService.GetAllTasksAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(Guid taskId, [FromQuery] string userId)
        {
            var result = await _taskService.DeleteTaskAsync(taskId, userId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
