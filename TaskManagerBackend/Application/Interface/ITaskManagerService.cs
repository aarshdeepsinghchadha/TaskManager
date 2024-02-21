using Application.Common;
using Application.TaskManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface ITaskManagerService
    {
        Task<ReturnResponse> RegisterUserAsync (RegisterDto registerDto);
        Task<ReturnResponse> LoginUserAsync (LoginDto loginDto);
        Task<ReturnResponse> DeleteUserAsync(string userId , string authorizationToken);
        Task<ReturnResponse> VerifyEmailAsync (string email , string token);
        Task<ReturnResponse> ForgotPasswordGenerateOTPAsync(ForgotPasswordGenerateOTPDto forgotPasswordGenerateOTPDto);
        Task<ReturnResponse> ResetPasswordWithOTPAsync(ResetPasswordDto resetPasswordDto);
        Task<ReturnResponse<TokenResponseDto>> GenerateNewRefreshToken(TokenResponseDto tokenResponseDto);
        Task<ReturnResponse> ResendEmailVerificationLinkAsync(ResendEmailVerificationLinkDto resendEmailVerificationLinkDto);

    }
}
