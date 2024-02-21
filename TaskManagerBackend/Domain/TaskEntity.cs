using Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    /// <summary>
    /// TaskEntity represents a task with additional properties and relationships.
    /// </summary>
    public class TaskEntity : BaseEntity
    {
        /// <summary>
        /// Unique identifier for the task.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TaskId { get; set; }
        /// <summary>
        /// Name of the task.
        /// </summary>
        public string TaskName { get; set; }
        /// <summary>
        /// Priority status of the task.
        /// </summary>
        public TaskPriorityStatus TaskPriority { get; set; }
        /// <summary>
        /// Status of the task.
        /// </summary>
        public TaskStatusEnum Status { get; set; }
        /// <summary>
        /// Description of the task.
        /// </summary>
        public string TaskDescription { get; set; }
        /// <summary>
        /// ID of the category to which the task belongs.
        /// </summary>
        public Guid CategoryId { get; set; }
        /// <summary>
        /// ID of the user who assigned the task.
        /// </summary>
        public string AssignedBy { get; set; }

        /// <summary>
        /// Task Due Date
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Navigation property for the associated task category.
        /// </summary>
        public TaskCategory TaskCategory { get; set; }
        /// <summary>
        /// Navigation property for the user who assigned the task.
        /// </summary>
        public AppUser AssignedByAppUser { get; set; }
        /// <summary>
        /// Collection of comments associated with the task.
        /// </summary>
        public ICollection<Comment> Comments { get; set; } = null!;
    }

}
