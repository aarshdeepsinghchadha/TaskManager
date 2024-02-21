
using Application.Common;
using Application.TaskManager;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(string username, string password);
        Task<ReturnResponse<RefreshToken>> SetRefreshToken(AppUser user, string token);
        Task<ReturnResponse<HeaderReturnDto>> DecodeGeneratedToken(string token);
        Task<ReturnResponse<HeaderReturnDto>> DecodeRefreshToken(string expiredToken);


    }

}
