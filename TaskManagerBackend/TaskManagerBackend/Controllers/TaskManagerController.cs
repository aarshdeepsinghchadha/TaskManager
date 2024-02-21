using Application.AppUserDto;
using Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagerBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskManagerController : ControllerBase
    {
        private readonly IRegisterLoginService _registerLoginService;

        public TaskManagerController(IRegisterLoginService registerLoginService)
        {
            _registerLoginService = registerLoginService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _registerLoginService.RegisterUserAsync(registerDto);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _registerLoginService.LoginUserAsync(loginDto);

            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _registerLoginService.DeleteUserAsync(id);
            if (result.Status)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }
}
