using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Task
{
    public class GetTaskListingDto
    {
        public string TaskName { get; set; }
        public string TaskCategoryName { get; set; }
        public string TaskPriority { get; set; }
        public int UserAssignedTo { get; set; }
        public int CommentsOnTask { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }

    }
}


