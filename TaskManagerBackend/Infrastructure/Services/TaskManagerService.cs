using Application.Common;
using Application.Interface;
using Application.TaskManager;
using Azure.Core;
using Domain;
using log4net;
using log4net.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Persistance;
using System.ComponentModel.Design;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using static System.Net.WebRequestMethods;

namespace Infrastructure.Services
{
    public class TaskManagerService : ITaskManagerService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IResponseGeneratorService _responseGeneratorService;
        private readonly ITokenService _tokenService;
        private readonly IEmailSenderService _emailSenderService;
        private readonly DataContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILog _log;
      


        public TaskManagerService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService generateTokenService, IResponseGeneratorService responseGeneratorService, IEmailSenderService emailSenderService, DataContext dbContext, IConfiguration configuration)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _responseGeneratorService = responseGeneratorService ?? throw new ArgumentNullException(nameof(responseGeneratorService));
            _tokenService = generateTokenService ?? throw new ArgumentNullException(nameof(generateTokenService));
            _emailSenderService = emailSenderService;
            _dbContext = dbContext;
            _configuration = configuration;
        //    _logger = logger;
            _log = LogManager.GetLogger(typeof(TaskManagerService));
        }
        public async Task<ReturnResponse> LoginUserAsync(LoginDto loginDto)
        {
            try
            {
                //_logger.Log( LogLevel.Information, $"User {loginDto.Username} is logging");
                _log.Info($"User {loginDto.Username} is logging");
                // Find the user by email or username
                var user = await GetUserByEmailOrUsernameAsync(loginDto.Username);

                if (user == null)
                {
                   _log.Warn($"Login attempt failed for non-existent user: {loginDto.Username}");
                    return await _responseGeneratorService.GenerateResponseAsync(
                        false, StatusCodes.Status401Unauthorized, "Invalid username or email.");
                }

                // Check if the provided password is valid
                var result = await _signInManager.CheckPasswordSignInAsync(
                    user, loginDto.Password, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    
                    // Getting token here 
                    loginDto.Username = user.UserName;
                    _log.Info("Generating token");
                    var token = await _tokenService.GenerateTokenAsync(user.UserName,loginDto.Password);
                    _log.Info("Saving token in refresh token table");
                    await _tokenService.SetRefreshToken(user, token);

                    _log.Info($"User with usernamer {user.UserName} has successfully logged in ");
                    return await _responseGeneratorService.GenerateResponseAsync<object>(
                        true, StatusCodes.Status200OK, "Login successful.",new { token });
                }
                else
                {
                    _log.Warn("Password is invalid");
                    return await _responseGeneratorService.GenerateResponseAsync(
                        false, StatusCodes.Status401Unauthorized, "Invalid password.");
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                _log.Error($"An error occurred during user login: {ex.Message}", ex);
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"An error occurred during user login : {ex.Message}");
            }
        }
        private async Task<AppUser?> GetUserByEmailOrUsernameAsync(string? username)
        {
            //if the user has enter email in the username text box
            if (!string.IsNullOrEmpty(username) && username.Contains("@"))
            {
                return await _userManager.FindByEmailAsync(username);
            }
            else if (!string.IsNullOrEmpty(username))
            {
                return await _userManager.FindByNameAsync(username);
            }
            return null;
        }
        public async Task<ReturnResponse> RegisterUserAsync(RegisterDto registerDto)
        {
            try
            {
                _log.Info($"User {registerDto.Username} is registering");
                // Check if password and confirmPassword match
                if (registerDto.Password != registerDto.ConfirmPassword)
                {
                    _log.Warn($"Password and confirm password do not match.");
                    return await _responseGeneratorService.GenerateResponseAsync(
                        false, StatusCodes.Status400BadRequest, "Password and confirm password do not match.");
                }

                // Check if a user with the same email already exists
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    _log.Warn("User with the same email already exists.");
                    return await _responseGeneratorService.GenerateResponseAsync(
                        false, StatusCodes.Status400BadRequest, $"User {existingUser.UserName} with the same email {existingUser.Email} already exists.");
                }
                //Hashing Password 
                var passwordHasher = new PasswordHasher<AppUser>();
                var hashedPassword = passwordHasher.HashPassword(null, registerDto.Password);

                // Create a new AppUser
                var newUser = new AppUser
                {
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    UserName = registerDto.Username,
                    Email = registerDto.Email,
                 //   ConfirmPassword = hashedPassword,
                    PhoneNumber = registerDto.PhoneNumber
                };

                // Register the user in the database
                var result = await _userManager.CreateAsync(newUser, registerDto.Password);
                _log.Info($"Sending verification link to {newUser.UserName}");
                var sendEmailResponse = await SendVerificationEmailAsync(newUser);

                if (result.Succeeded)
                {
                    _log.Info("User registered , verification pending ");
                    if (sendEmailResponse.Status)
                    {
                        _log.Info("User Registered and verification link is sent to email");
                        return await _responseGeneratorService.GenerateResponseAsync(
                        true, StatusCodes.Status200OK, $"Email sent successfully , Please check your Email to verify ");
                    }
                    else
                    {
                        var errorMessage = sendEmailResponse.Message;
                        _log.Error($"Error occurred to get email :{errorMessage}");
                        return await _responseGeneratorService.GenerateResponseAsync(
                            false, StatusCodes.Status500InternalServerError, $"Failed to send email : {errorMessage}");
                    }
                }
                else
                {
                    _log.Error($"User registration failed for user {newUser.UserName}");
                    // Handle registration failure
                    return await _responseGeneratorService.GenerateResponseAsync(
                        false, StatusCodes.Status500InternalServerError, $"Failed to register user : {result.Errors}");
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                _log.Error($"Error occurred during user registration {ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"An error occurred during user registration: {ex.Message}");
            }
        }
        private async Task<ReturnResponse> SendVerificationEmailAsync(AppUser newUser)
        {
            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
               var baseUrl = "https://localhost:44342";
                var verificationLink = $"{baseUrl}/api/TaskManager/VerifyEmail?email={newUser.Email}&token={encodedToken}";

                var emailBody = $@"<html><body><p>Thank you for registering! Click here <a href='{verificationLink}'>Verification Link</a> to verify your email.</p></body></html>";

                // Assuming _emailSenderService has a method named SendEmailBySendGridAsync
                await _emailSenderService.SendEmailByMailgunAsync(newUser.Email, "Confirm Email", emailBody);
                _log.Info("Email Sent Successfully");
                return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Email Sent Successfully"); 
            }
            catch (Exception ex)
            {
                _log.Error($"Error occurred while sending email {ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status404NotFound, ex.Message);
            }
        }
        public async Task<ReturnResponse> ResendEmailVerificationLinkAsync(ResendEmailVerificationLinkDto resendEmailVerificationLinkDto)
        {
            try
            {
                _log.Info("Resending verification link");
                var existingUser = await _userManager.FindByEmailAsync(resendEmailVerificationLinkDto.Email);
                if (existingUser != null)
                {
                   
                    if(existingUser.EmailConfirmed)
                    {
                        _log.Warn($"User {existingUser.UserName} is already verified");
                        return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "User is already verified");
                    }

                    var resendEmailResponse = await SendVerificationEmailAsync(existingUser);

                    if (resendEmailResponse.Status)
                    {
                        _log.Info($"verification link has been resent successfully to {existingUser.UserName}");         
                        return await _responseGeneratorService.GenerateResponseAsync(
                         true, StatusCodes.Status200OK, $"Verification email has been resent successfully for user {existingUser.UserName}");
                    }
                    else
                    {
                        _log.Info("Failed to resend verification email");
                        return await _responseGeneratorService.GenerateResponseAsync(
                       false, StatusCodes.Status500InternalServerError, $"Failed to resend verification email");
                    }
                }
                else
                {
                    _log.Error("User does not exist");
                    return await _responseGeneratorService.GenerateResponseAsync(
                   false, StatusCodes.Status404NotFound, "User does not exist");
                }
            }
            catch(Exception ex) 
            {
                _log.Error($"Faied to resend verification email {ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                  false, StatusCodes.Status404NotFound, $"Faied to resend verification email {ex.Message}");
            }
        }
        public async Task<ReturnResponse> DeleteUserAsync(string userId , string authorizationToken)
        {
            try
            {
               
                ReturnResponse<HeaderReturnDto> checkHeaderResponse = await _tokenService.DecodeGeneratedToken(authorizationToken);
                if (checkHeaderResponse.Data.Status)
                {
                    string requestingUserId = checkHeaderResponse.Data.UserId;
                    var requestingUser = await _userManager.FindByIdAsync(requestingUserId);
                    if (requestingUser == null)
                    {
                        _log.Warn("User not found.");
                        return await _responseGeneratorService.GenerateResponseAsync(
                            false, StatusCodes.Status404NotFound, "Requesting User not found.");
                    }

                    _log.Info("Deleting User");
                    // Find the user by Id
                    var user = await _userManager.FindByIdAsync(userId);

                    if (user == null)
                    {
                        _log.Warn("User not found.");
                        return await _responseGeneratorService.GenerateResponseAsync(
                            false, StatusCodes.Status404NotFound, "User not found.");
                    }

                    var isRequestingUserAdmin = await _userManager.IsInRoleAsync(requestingUser, "Admin");

                    if (!(isRequestingUserAdmin || userId == requestingUserId))
                    {
                        return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status403Forbidden, "You are not authorized to delete this user.");
                    }

                    // Delete the user
                    var result = await _userManager.DeleteAsync(user);

                    if (result.Succeeded)
                    {
                        _log.Info($"User {user.UserName} is being deleted successfully");
                        return await _responseGeneratorService.GenerateResponseAsync(
                            true, StatusCodes.Status200OK, "User deleted successfully.");
                    }
                    else
                    {
                        // Handle deletion failure
                        _log.Info($"Failed to delete user {user.UserName}");
                        return await _responseGeneratorService.GenerateResponseAsync(
                            false, StatusCodes.Status500InternalServerError, "Failed to delete user.");
                    }
                }
                else
                {
                    _log.Info($"Invalid token , cannot delete user ");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status401Unauthorized, "Please Login again and pass token!");
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                _log.Error($"An error occurred during user deletion {ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, "An error occurred during user deletion.");
            }
        }
        public async Task<ReturnResponse> VerifyEmailAsync(string email, string token)
        {
            try
            {
                _log.Info("Verifying user");
                byte[] decodedToken = WebEncoders.Base64UrlDecode(token);
                string normalToken = Encoding.UTF8.GetString(decodedToken);

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _log.Warn($"User with email {email} is not found");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status404NotFound, "User not found");
                }

                var result = await _userManager.ConfirmEmailAsync(user, normalToken);
                if (result.Succeeded)
                {
                    _log.Info($"User {user.UserName} with {email} verified successfully ");
                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Email verified successfully");
                }
                else
                {
                    _log.Error("Invalid email or token , verification failed ");
                    return await _responseGeneratorService.GenerateResponseAsync(
                        false, StatusCodes.Status400BadRequest, "Invalid email or token   ");
                }
            }
            catch(Exception ex) 
            {
                _log.Error($"Failed to verify email:{email}");
                return await _responseGeneratorService.GenerateResponseAsync(
                    false, StatusCodes.Status500InternalServerError, $"An error occurred during email verification: {ex.Message}");
            }

        }
        public async Task<ReturnResponse> ForgotPasswordGenerateOTPAsync(ForgotPasswordGenerateOTPDto forgotPasswordGenerateOTPDto)
        {
            try
            {
                _log.Info("Generating forgot password otp");
                var user = await _userManager.FindByEmailAsync(forgotPasswordGenerateOTPDto.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    _log.Warn("Invalid email or token ");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status404NotFound, "Invalid Email");
                }
                var otp = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                var emailBody = $@"<html><body><p>Your OTP for password reset is: {otp}</p></body></html>";
                _log.Info($"Forgot password OTP sent to {user.UserName} ");
                var sendEmailResponse = await _emailSenderService.SendEmailByMailgunAsync(forgotPasswordGenerateOTPDto.Email, "Password Reset OTP", emailBody);

                // Check if the email was sent successfully
                if (sendEmailResponse.Status)
                {
                    _log.Info($"OTP sent to user {user.UserName}");
                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "OTP sent successfully to your registered email");
                }
                else
                {
                    _log.Warn($"Failed to send OTP emai :{sendEmailResponse.Message}");
                    var errorMessage = sendEmailResponse.Message;
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status500InternalServerError, $"Failed to send OTP email {errorMessage}");
                }

            }
            catch (Exception ex)
            {
                _log.Error($"Error occurred in sending forgot password otp {ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status500InternalServerError, $"An error occurred in sending Email: {ex.Message}");
            }

        }
        public async Task<ReturnResponse> ResetPasswordWithOTPAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                _log.Info("Reseting password");
                var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
                if (user == null)
                {
                    _log.Warn("Invalid email, user not found");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status404NotFound, "Invalid Email");
                }

                var isOtpValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", resetPasswordDto.Token);
                if (!isOtpValid)
                {
                    _log.Warn("Invalid OTP ");
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status404NotFound, "Invalid OTP");
                }

                var tokenForResetPassword = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, tokenForResetPassword, resetPasswordDto.NewPassword);

                if (result.Succeeded)
                {
                    //Hashing the password while saving 
                    var passwordHasher = new PasswordHasher<AppUser>();
                    var hashedPassword = passwordHasher.HashPassword(user, resetPasswordDto.NewPassword);
                   // user.ConfirmPassword = hashedPassword;

                    await _userManager.UpdateAsync(user);
                    _log.Info("Password has been reset successfully.");
                    return await _responseGeneratorService.GenerateResponseAsync(
                       true, StatusCodes.Status200OK, "Password has been reset successfully.");
                }
                else
                {
                    _log.Error($"Failed to reset password for user {user.UserName}");
                    return await _responseGeneratorService.GenerateResponseAsync(
                        false, StatusCodes.Status500InternalServerError, $"Failed to reset password");
                }

            }
            catch (Exception ex)
            {
                _log.Error($"Failed to reset password {ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(
                       false, StatusCodes.Status500InternalServerError, $"Failed to reset password {ex.Message}");
            }

        }

       // Refresh Token Service
        public async Task<ReturnResponse<TokenResponseDto>> GenerateNewRefreshToken(TokenResponseDto tokenResponseDto)
        {
            TokenResponseDto checkTokenResponse = new();

            try
            {
                _log.Info("Generating new refresh token");
                // Match token with token in Refresh Token Table 
                var existingToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == tokenResponseDto.Token && !rt.Revoked.HasValue);

                if (existingToken == null)
                {
                    _log.Warn("Token is revoked , failed to regenerate new refresh token");
                    return await _responseGeneratorService.GenerateResponseAsync<TokenResponseDto>(false, StatusCodes.Status401Unauthorized, "Token is revoked", null );
                }

                // Get UserId using IsUserTokenValid method in CheckHeaderService
                ReturnResponse<HeaderReturnDto> decodedTokenCheckResponse = await _tokenService.DecodeRefreshToken(tokenResponseDto.Token);

                if (decodedTokenCheckResponse.Data.UserId == null)
                {
                    _log.Warn("Failed to decode token ");
                    return await _responseGeneratorService.GenerateResponseAsync<TokenResponseDto>(false, StatusCodes.Status404NotFound, "Failed to decode token ", null); 
                }
                // Find User 
                AppUser user = await _userManager.FindByIdAsync(decodedTokenCheckResponse.Data.UserId);

                if (user == null)

                {
                    _log.Warn("User cannot be found after decoding token ");
                    return await _responseGeneratorService.GenerateResponseAsync<TokenResponseDto>(false, StatusCodes.Status404NotFound, "User Cannot be found ", null);
                }

                // Validate User
                if (user.Email != tokenResponseDto.Email)
                {
                    _log.Warn("Invalid user as email is not matching with email from decoded token");
                    return await _responseGeneratorService.GenerateResponseAsync<TokenResponseDto>(false, StatusCodes.Status404NotFound, "Invalid user, cannot find email ", null);
                }

                // Generate new Jwt Refreshed Token and add it to RefreshToken table
                string newRefreshedToken = GenerateJwtToken(user);
                await _tokenService.SetRefreshToken(user, newRefreshedToken);
                
                await _dbContext.SaveChangesAsync();

                checkTokenResponse = new TokenResponseDto
                {
                    Email = user.Email,
                    Token = newRefreshedToken
                };
                _log.Info("Token refreshed successfully");
                return await _responseGeneratorService.GenerateResponseAsync<TokenResponseDto>(true, StatusCodes.Status200OK, "Token is refreshed ", checkTokenResponse);
            }
            catch (Exception ex)
            {
                _log.Error($"Error occurred while generating new refresh token {ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync<TokenResponseDto>(false, StatusCodes.Status404NotFound, ex.Message, null);
            }
        }
        private string GenerateJwtToken(AppUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var jwtExpirationTimeInMinutes = Convert.ToInt32(_configuration["Jwt:ExpirationTimeInMinutes"]);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(jwtExpirationTimeInMinutes),
                signingCredentials: credentials
            );
            _log.Info("Token generated ");
            return new JwtSecurityTokenHandler().WriteToken(token);


        }
    }
}
