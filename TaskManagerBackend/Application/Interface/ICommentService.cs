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
        Task<ReturnResponse> AddCommentAsync(AddCommentDto commentDto);
        Task<ReturnResponse> UpdateCommentAsync(Guid commentId, EditCommentDto commentDto);
        Task<ReturnResponse> GetCommentByIdAsync(Guid commentId);
        Task<ReturnResponse> GetAllCommentsAsync();
        Task<ReturnResponse> DeleteCommentAsync(Guid commentId, string userId);
    }
}
