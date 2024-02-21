using Application.AppUserDto;
using Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface IRegisterLoginService
    {
        Task<ReturnResponse> RegisterUserAsync (RegisterDto registerDto);
        Task<ReturnResponse> LoginUserAsync (LoginDto loginDto);
        Task<ReturnResponse> DeleteUserAsync(string userId);
    }
}
