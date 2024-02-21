using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.TaskCategory
{
    public class GetAllTaskForTaskCategoryDto
    {
        public Guid TaskId { get; set; }
        public string TaskName { get; set; }
        public string TaskPriority { get; set; }
        public string Status { get; set; }
        public string TaskDescription { get; set; }
        public string AssignedBy { get; set; }
        public DateTime DueDate { get; set; }
    }
}
