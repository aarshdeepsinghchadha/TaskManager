using Application.Comment;
using Application.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagerBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost("Comment")]
        public async Task<IActionResult> AddComment([FromBody] AddCommentDto commentDto , [FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _commentService.AddCommentAsync(commentDto, authorizationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("Comment/{commentId}")]
        public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] EditCommentDto commentDto , [FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _commentService.UpdateCommentAsync(commentId, commentDto , authorizationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("Comment/{commentId}")]
        public async Task<IActionResult> GetCommentById(Guid commentId, [FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _commentService.GetCommentByIdAsync(commentId, authorizationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("Comment")]
        public async Task<IActionResult> GetAllComments([FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _commentService.GetAllCommentsAsync(authorizationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("Comment/{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId, [FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _commentService.DeleteCommentAsync(commentId, authorizationToken);
            return StatusCode(result.StatusCode, result);
        }
    }

}
