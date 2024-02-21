using Application.AdminDto;
using Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface IAdminService
    {
        Task<ReturnResponse<List<UserDetailsDTO>>> GetAllUser(string authorizationToken);
        Task<ReturnResponse<UserDetailsDTO>> GetUserDetails(string authorizationToken);
        Task<ReturnResponse> UpdateUser(Guid userId, EditUserDetailsDTO editUserDetailsDTO, string authorizationToken);
    }
}
    