using Application.Comment;
using Application.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagerBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] AddCommentDto commentDto)
        {
            var result = await _commentService.AddCommentAsync(commentDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{commentId}")]
        public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] EditCommentDto commentDto)
        {
            var result = await _commentService.UpdateCommentAsync(commentId, commentDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{commentId}")]
        public async Task<IActionResult> GetCommentById(Guid commentId)
        {
            var result = await _commentService.GetCommentByIdAsync(commentId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllComments()
        {
            var result = await _commentService.GetAllCommentsAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId, [FromQuery] string userId)
        {
            var result = await _commentService.DeleteCommentAsync(commentId, userId);
            return StatusCode(result.StatusCode, result);
        }
    }

}
