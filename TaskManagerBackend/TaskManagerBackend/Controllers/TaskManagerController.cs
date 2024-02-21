using Application.Interface;
using Application.TaskManager;
using Infrastructure.Services;
using log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagerBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskManagerController : ControllerBase
    { 
        private readonly ITokenService _tokenService;
        private readonly ITaskManagerService _taskManagerService;
        private readonly ILogger<TaskManagerController> _logger;
        private readonly ILog _log;

        public TaskManagerController(ITokenService tokenService, ITaskManagerService taskManagerService, ILogger<TaskManagerController> logger)
        {
            _tokenService = tokenService;
            _taskManagerService = taskManagerService;
            _logger = logger;
            _log = LogManager.GetLogger(typeof(TaskManagerController));
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> RefreshToken(TokenResponseDto tokenResponseDto)
        {
            var result = await _taskManagerService.GenerateNewRefreshToken(tokenResponseDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
           // var origin = Request.Headers["origin"];
            var result = await _taskManagerService.RegisterUserAsync(registerDto);
             
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _taskManagerService.LoginUserAsync(loginDto);

            return StatusCode(result.StatusCode, result);
        }

        [Authorize]
        [HttpDelete("User/{id}")]
        public async Task<IActionResult> DeleteUser(string id , [FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _taskManagerService.DeleteUserAsync(id , authorizationToken);
            if (result.Status)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        [AllowAnonymous]
        [HttpGet("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail([FromQuery(Name = "email")] string email,[FromQuery(Name = "token")] string token)
        {
            var result = await _taskManagerService.VerifyEmailAsync(email, token);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("ResendEmailVerificationLink")]
        public async Task<IActionResult> ResendEmailVerificationLink([FromBody] ResendEmailVerificationLinkDto resendEmailVerificationLinkDto)
        {
            //var origin = Request.Headers["origin"];
            var result = await _taskManagerService.ResendEmailVerificationLinkAsync(resendEmailVerificationLinkDto);
            return StatusCode(result.StatusCode, result);
        }



        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPasswordGenerateOTP([FromBody] ForgotPasswordGenerateOTPDto forgotPasswordGenerateOTPDto)
        {
            var result = await _taskManagerService.ForgotPasswordGenerateOTPAsync(forgotPasswordGenerateOTPDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPasswordWithOTP([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var result = await _taskManagerService.ResetPasswordWithOTPAsync(resetPasswordDto);
            return StatusCode(result.StatusCode, result);
        }


    }
}
