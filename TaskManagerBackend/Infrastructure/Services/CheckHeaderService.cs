using Application.Common;
using Application;
using Application.Interface;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Persistance;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services
{
    public class CheckHeaderService : ICheckHeaderService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly DataContext _dbContext;
        private readonly IResponseGeneratorService _responseGeneratorService;
        private readonly IConfiguration _configuration;

        public CheckHeaderService(DataContext dbContext, UserManager<AppUser> userManager, IResponseGeneratorService responseGeneratorService, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _responseGeneratorService = responseGeneratorService;
            _configuration = configuration;
        }

        public async Task<ReturnResponse<HeaderReturnDto>> CheckPassedUserToken(string token)
        {
            try
            {
                if(string.IsNullOrEmpty(token) || string.IsNullOrWhiteSpace(token))
                {
                    return  await _responseGeneratorService.GenerateResponseAsync<HeaderReturnDto>(false, StatusCodes.Status401Unauthorized, "Please Login and pass the token", null);
                }
                // Remove "Bearer " prefix from the token, if present
                token = token?.Replace("Bearer ", string.Empty);
                // Decode and validate the JWT token
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSecret = _configuration["Jwt:Secret"];
                var key = Encoding.UTF8.GetBytes(jwtSecret);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                // Extract user information from the decoded token
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = principal.FindFirst(ClaimTypes.Name)?.Value;
                var email = principal.FindFirst(ClaimTypes.Email)?.Value;

                // Check if the user exists
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return await _responseGeneratorService.GenerateResponseAsync<HeaderReturnDto>(false, StatusCodes.Status404NotFound, "User does not exist", null);
                }

                // Check if the token exists in the refresh token table 
                var refreshTokenExists = await _dbContext.RefreshTokens.AnyAsync(rt => rt.Token == token && rt.AppUserId == userId);

                // Construct the response
                var response = new HeaderReturnDto
                {
                    UserId = user.Id,
                    Status = refreshTokenExists // Set the status based on whether the refresh token exists
                };

                return await _responseGeneratorService.GenerateResponseAsync<HeaderReturnDto>(true, StatusCodes.Status200OK, "User Authorized", response);
            }
            catch (Exception ex)
            {
                return await _responseGeneratorService.GenerateResponseAsync<HeaderReturnDto>(false, StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }
    }

}
