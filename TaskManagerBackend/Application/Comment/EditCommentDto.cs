using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Comment
{
    public class EditCommentDto
    {
        public string CommentDescription { get; set; }
        public string CommentedById { get; set; }
        public Guid TaskId { get; set; }
        public string UserId { get; set; }
    }
}
