
using Application.Common;
using Application.Interface;
using Application.TaskManager;
using Domain;
using log4net;
using log4net.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Persistance;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services   
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        private readonly IResponseGeneratorService _responseGeneratorService;
        private readonly DataContext _dataContext;
        //  private readonly ILogger<TokenService> _logger;
        private readonly ILog _log;

        public TokenService(IConfiguration configuration, UserManager<AppUser> userManager, IResponseGeneratorService responseGeneratorService, DataContext dataContext)
        {
            _configuration = configuration;
            _userManager = userManager;
            _responseGeneratorService = responseGeneratorService;
            _dataContext = dataContext;
         //   _logger = logger;
            _log = LogManager.GetLogger(typeof(TokenService));
        }
        public async Task<string> GenerateTokenAsync(string username, string password)
        {
            try
            {

                var user = await _userManager.FindByNameAsync(username);
                if (user == null || !await _userManager.CheckPasswordAsync(user, password))
                {
                    _log.Warn("failed to generate token due to incorrect user ");
                   throw new Exception("Username is incorrect");
                }


                //Header for Jwt token
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var roles = await _userManager.GetRolesAsync(user);
                //claims , that is payload part 
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name , user.UserName ) ,
                    new Claim(ClaimTypes.Email , user.Email)
                      //new Claim(JwtRegisteredClaimNames.Iat , DateTime.Now.ToString())
                };

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var jwtExpirationTimeInMinutes = Convert.ToInt32(_configuration["Jwt:ExpirationTimeInMinutes"]);

                //creating Jwt Token 
                var token = new JwtSecurityToken
                    (
                   _configuration["Jwt:Issuer"],
                   _configuration["Jwt:Audience"],
                   claims,
                   expires: DateTime.UtcNow.AddMinutes(jwtExpirationTimeInMinutes),
                   signingCredentials: credentials
                    );
                _log.Info("Token generated");
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch ( Exception ex )
            {
                _log.Error($"Error occured while adding task:{ex.Message}");
                throw new Exception(ex.Message);
            }
        }
        public async Task<ReturnResponse<RefreshToken>> SetRefreshToken(AppUser user ,string token)
        {
            try
            {
                if (user == null)
                {
                    _log.Warn("User not found while saving token in table ");
                    await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status404NotFound, "User not found");
                }

               
                // Revoke tokens
                var refreshTokensToRevoke = _dataContext.RefreshTokens
                    .Where(rt => rt.AppUserId == user.Id && rt.Revoked == null)
                    .ToList();

                foreach (var item in refreshTokensToRevoke)
                {
                    item.Revoked = DateTime.UtcNow;
                }

                var expirationInMinutes = Convert.ToInt32(_configuration["Jwt:ExpirationTimeInMinutes"]);
                var expirationDateTime = DateTime.UtcNow.AddMinutes(expirationInMinutes);

                RefreshToken refreshToken = new RefreshToken()
                {
                    AppUserId = user.Id,
                    Expires = expirationDateTime,
                    Token = token

                };
                // Update User 
                user.RefreshTokens.Add(refreshToken);
                await _userManager.UpdateAsync(user);

                //var cookieOptions = new CookieOptions
                //{

                //    HttpOnly = true, // not accessible via JavaScript
                //    Expires = expirationDateTime
                //};
                _log.Info("Token Saved successfully");
               return  await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Token Saved Successfully", refreshToken);

            }
            catch (Exception ex)
            {
                _log.Error($"Error occured while setting token in refreshtoken table task:{ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync<RefreshToken>(false, StatusCodes.Status500InternalServerError, $"Failed to save token : {ex.Message}",null);
            }
        }
        public async Task<ReturnResponse<HeaderReturnDto>> DecodeGeneratedToken(string token)
        {
            try
            {
                _log.Info("Decoding generated token");
                if (string.IsNullOrEmpty(token) || string.IsNullOrWhiteSpace(token))
                {
                    return await _responseGeneratorService.GenerateResponseAsync<HeaderReturnDto>(false, StatusCodes.Status401Unauthorized, "Please Login and pass the token", null); ;
                }
                // Remove "Bearer " prefix from the token, if present

                token = token?.Replace("Bearer ", string.Empty);

                var handler = new JwtSecurityTokenHandler();
                // Validate and parse token 
                var decodedtoken = handler.ReadToken(token) as JwtSecurityToken;

                var validateparameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken securityToken;

                var principal = handler.ValidateToken(token, validateparameters, out securityToken);
                var username = principal.FindFirst(ClaimTypes.Name)?.Value;
                var email = principal.FindFirst(ClaimTypes.Email)?.Value;

                // Fetching Registered User Email and User Name 
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    _log.Error("Invalid user found after while decoding");
                    return await _responseGeneratorService.GenerateResponseAsync<HeaderReturnDto>(false, StatusCodes.Status404NotFound, "User does not exist", null);
                }

                //Check refresh token already exists or not 
                var refreshToken = await _dataContext.RefreshTokens.AnyAsync(rt => rt.Token == token && rt.AppUserId == user.Id && rt.Revoked == null);

                var response = new HeaderReturnDto
                {
                    Status = refreshToken,
                    UserId = user.Id
                };
                _log.Info("Token decoded successfully");
                return await _responseGeneratorService.GenerateResponseAsync<HeaderReturnDto>(true, StatusCodes.Status200OK, "User Authorized", response);

            }
            catch (Exception ex)
            {
                _log.Error($"Error occured while decoding generated token :{ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync<HeaderReturnDto>(false, StatusCodes.Status500InternalServerError, ex.Message, null);
            }

        }
        public async Task<ReturnResponse<HeaderReturnDto>> DecodeRefreshToken(string expiredToken)
        {

            try
            {
                _log.Info("Decoding refresh token");
                if (string.IsNullOrEmpty(expiredToken) || string.IsNullOrWhiteSpace(expiredToken))
                {
                    return await _responseGeneratorService.GenerateResponseAsync<HeaderReturnDto>(false, StatusCodes.Status401Unauthorized, "Token not found", null); 
                }
                // Remove "Bearer " prefix from the token, if present

                
                expiredToken = expiredToken?.Replace("Bearer ", string.Empty);

                var handler = new JwtSecurityTokenHandler();
                // Validate and parse token 
                var decodedToken = handler.ReadToken(expiredToken) as JwtSecurityToken;

                var validateParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                    ValidateLifetime =false,
                   
                };

                SecurityToken decodedSecurityToken;
                var principal = handler.ValidateToken(expiredToken, validateParameters, out decodedSecurityToken);

                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var user = await _userManager.FindByEmailAsync(email);
                if (user== null)
                {
                    _log.Error("Invalid user found after while decoding");
                    return await _responseGeneratorService.GenerateResponseAsync<HeaderReturnDto>(false, StatusCodes.Status404NotFound, "User not found", null);
                }

                var response = new HeaderReturnDto
                {
                    UserId = user.Id
                };
                _log.Info("Token decoded successfully");
                return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Token decoded successfully", response);


            }
            catch (Exception ex)
            {
                _log.Error($"Error occured while decoding refresh token :{ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync<HeaderReturnDto>(false, StatusCodes.Status500InternalServerError, $"Error occured while decoding token: {ex.Message}", null);

            }

        }
    }
}
