using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.AdminDto
{
    public class UserDetailsDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<TaskDTO> Tasks { get; set; }
    }

    public class TaskDTO
    {
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public DateTime DueDate { get; set; }
        public string AssignedBy { get; set; }
        public TaskCategoryDTO TaskCategory { get; set; }
        public List<CommentDTO> Comments { get; set; }
    }

    public class TaskCategoryDTO
    {
        public string TaskCategoryName { get; set; }
    }

    public class CommentDTO
    {
        public string CommentedBy { get; set; }
        public string CommentDescription { get; set; }
    }



}
