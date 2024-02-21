using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Task
{
    public class AddTaskDto
    {
        public string TaskName { get; set; }
        public int TaskPriority { get; set; }
        public int Status { get; set; }
        public string TaskDescription { get; set; }
        public Guid CategoryId { get; set; }
        public string AssignedBy { get; set; }
        public DateTime DueDate { get; set; }
        public string UserId { get; set; }
    }
}
