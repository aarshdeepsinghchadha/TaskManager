using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Task
{
    public class GetAllTaskDto
    {
        public string TaskName { get; set; }
        public int TaskPriority { get; set; }
        public int Status { get; set; }
        public string TaskDescription { get; set; }
        public Guid CategoryId { get; set; }
        public string AssignedBy { get; set; }
        public DateTime DueDate { get; set; }
        public string CreatedbyName {  get; set; }
        public string UpdatedbyName {  get; set; }
        public TaskCategoryDetailsDto TaskCategory { get; set; }
        public List<TaskCommentList>? Comments { get; set; }
    }
    public class TaskCategoryDetailsDto
    {
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
    }
    public class TaskCommentList
    {
        public string CommentDescription { get; set;}
        public string CommentedById { get; set;}
        public string CommentedByName { get; set;}
    }
}
