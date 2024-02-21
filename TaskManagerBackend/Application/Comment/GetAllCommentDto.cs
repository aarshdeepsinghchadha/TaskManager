using Application.Task;

namespace Application.Comment
{
    public class GetAllCommentDto
    {
        public string CommentDescription { get; set; }
        public string CommentedById { get; set; }
        public string CommentedByName { get; set; }
        public List<TaskDetails> Tasks { get; set; }
    }
    public class TaskDetails
    {
        public string TaskName { get; set; }
        public int TaskPriority { get; set; }
        public int Status { get; set; }
        public string TaskDescription { get; set; }
        public Guid CategoryId { get; set; }
        public string AssignedBy { get; set; }
        public DateTime DueDate { get; set; }
        public string CreatedbyName { get; set; }
        public string UpdatedbyName { get; set; }
        public TaskCategoryDetailsDto TaskCategory { get; set; }
    }
}
