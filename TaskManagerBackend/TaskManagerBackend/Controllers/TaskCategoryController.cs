using Application;
using Application.Common;
using Application.Interface;
using Application.TaskCategory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagerBackend.Controllers
{

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class TaskCategoryController : ControllerBase
    {
        private readonly ITaskCategoryService _taskCategoryService;

        public TaskCategoryController(ITaskCategoryService taskCategoryService)
        {
            _taskCategoryService = taskCategoryService;
        }

        [HttpPost]
        public async Task<IActionResult> AddTaskCategory([FromHeader(Name = "Authorization")] string authorizationToken, [FromBody] AddTaskCategoryDto addTaskCategoryDto)
        {
            var result = await _taskCategoryService.AddTaskCategoryAsync(addTaskCategoryDto, authorizationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{categoryId}")]
        public async Task<IActionResult> EditTaskCategory(Guid categoryId, [FromBody] EditTaskCategoryDto editTaskCategoryDto)
        {
            var result = await _taskCategoryService.EditTaskCategoryAsync(categoryId, editTaskCategoryDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetTaskCategoryById(Guid categoryId)
        {
            var result = await _taskCategoryService.GetTaskCategoryByIdAsync(categoryId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTaskCategories([FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _taskCategoryService.GetAllTaskCategoriesAsync(authorizationToken);
            return StatusCode(result.StatusCode, result);
        }


        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteTaskCategory(Guid categoryId, [FromQuery] string userId)
        {
            var result = await _taskCategoryService.DeleteTaskCategoryAsync(categoryId, userId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
