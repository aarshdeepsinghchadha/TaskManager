using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Comment
{
    public class AddCommentDto
    {
        public string CommentDescription { get; set; }

       // public string CommentedById { get; set; }
        public Guid TaskId { get; set; }
       
    }
}
