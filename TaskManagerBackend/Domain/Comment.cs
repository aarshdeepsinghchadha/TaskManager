using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    /// <summary>
    /// Comment represents a comment associated with a task.
    /// </summary>
    public class Comment : BaseEntity
    {
        /// <summary>
        /// Unique identifier for the comment.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid CommentId { get; set; }
        /// <summary>
        /// Description of the comment.
        /// </summary>
        public string CommentDescription { get; set; }
        /// <summary>
        /// ID of the user who made the comment.
        /// </summary>
        public string CommentedById { get; set; }
        /// <summary>
        /// ID of the task to which the comment belongs.
        /// </summary>
        public Guid TaskId { get; set; }

        /// <summary>
        /// Navigation property for the associated task.
        /// </summary>
        public TaskEntity CommentedTask { get; set; }
        /// <summary>
        /// Navigation property for the user who made the comment.
        /// </summary>
        public AppUser CommentedAppUser { get; set; }
    }
}
