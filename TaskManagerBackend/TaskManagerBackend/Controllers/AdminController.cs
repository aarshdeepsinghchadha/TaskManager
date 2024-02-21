using Application.AdminDto;
using Application.Interface;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagerBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

     [Authorize(Roles ="Admin")]
        [HttpGet("UserList")]
        public async Task<IActionResult> GetAllUsers([FromHeader(Name = "Authorization")] string authorizationToken)
         {
            var result =  await _adminService.GetAllUser(authorizationToken);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("User")]
        public async Task<IActionResult> GetUser([FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _adminService.GetUserDetails(authorizationToken);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize]
        [HttpPut("EditUser/{userId}")]
        public async Task<IActionResult> UpdateUser(Guid userId, EditUserDetailsDTO editUserDetailsDTO ,[FromHeader(Name = "Authorization")] string authorizationToken)
        {
            var result = await _adminService.UpdateUser(userId ,editUserDetailsDTO ,authorizationToken);
            return StatusCode(result.StatusCode, result);
        }


    }
}
