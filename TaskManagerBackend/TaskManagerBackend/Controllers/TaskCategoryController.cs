using Application.Interface;
using Application.TaskCategory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagerBackend.Controllers
{
    [ApiController]

    [Authorize]
    [Route("api/[controller]")]

    public class TaskCategoryController : ControllerBase
    {
        private readonly ITaskCategoryService _taskCategoryService;
       
        public TaskCategoryController(ITaskCategoryService taskCategoryService)
        {
            _taskCategoryService = taskCategoryService;
        }

        [HttpGet("GetCategoryListing")]
        public async Task<IActionResult> GetCategoryListing([FromQuery(Name = "SortColumn")] string SortColumn, [FromQuery(Name = "SortDirection")] int SortDirection, [FromQuery] int pageNumber , [FromQuery] int pageSize , [FromHeader(Name = "Authorization")] string authorizationToken)
        {
                var result = await _taskCategoryService.GetTaskCategoryListing(SortColumn, SortDirection,pageNumber,pageSize,authorizationToken);
            return StatusCode(200, result);
        }

        [HttpPost("Category")]
        public async Task<IActionResult> AddTaskCategory([FromBody] AddTaskCategoryDto addTaskCategoryDto , [FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _taskCategoryService.AddTaskCategoryAsync(addTaskCategoryDto , authorizationToken);
            return StatusCode(result.StatusCode, result);
        }

        
        [HttpPut("Category/{categoryId}")]
        public async Task<IActionResult> EditTaskCategory(Guid categoryId, [FromBody] EditTaskCategoryDto editTaskCategoryDto, [FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _taskCategoryService.EditTaskCategoryAsync(categoryId, editTaskCategoryDto , authorizationToken);
            return StatusCode(result.StatusCode, result);
        }

        
        [HttpGet("Category/{categoryId}")]
        public async Task<IActionResult> GetTaskCategoryById(Guid categoryId , [FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _taskCategoryService.GetTaskCategoryByIdAsync(categoryId , authorizationToken);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Category")]
        public async Task<IActionResult> GetAllTaskCategories([FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _taskCategoryService.GetAllTaskCategoriesAsync( authorizationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("Category/{categoryId}")]
        public async Task<IActionResult> DeleteTaskCategory(Guid categoryId, [FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _taskCategoryService.DeleteTaskCategoryAsync(categoryId, authorizationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
