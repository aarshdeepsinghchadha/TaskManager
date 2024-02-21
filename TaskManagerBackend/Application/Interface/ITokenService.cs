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
        Task<string> GenerateLoginToken(string username, string password);
        Task<RefreshToken> SetRefreshToken(AppUser user, string token);
    }
}
