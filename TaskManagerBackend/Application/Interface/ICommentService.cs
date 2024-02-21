using Application.Comment;
using Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface ICommentService
    {
        Task<ReturnResponse> AddCommentAsync(AddCommentDto commentDto , string authorizationToken);
        Task<ReturnResponse> UpdateCommentAsync(Guid commentId, EditCommentDto commentDto , string authorizationToken);
        Task<ReturnResponse> GetCommentByIdAsync(Guid commentId , string authorizationToken);
        Task<ReturnResponse> GetAllCommentsAsync(string authorizationToken);
        Task<ReturnResponse> DeleteCommentAsync(Guid commentId, string authorizationToken);
    }
}
