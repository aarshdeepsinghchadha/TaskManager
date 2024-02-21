using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    /// <summary>
    /// TaskCategory represents a category to which tasks can belong.
    /// </summary>
    public class TaskCategory : BaseEntity
    {
        /// <summary>
        /// Unique identifier for the category.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid CategoryId { get; set; }
        /// <summary>
        /// Name of the category.
        /// </summary>
        public string CategoryName { get; set; }
        /// <summary>
        /// Description of the category.
        /// </summary>
        public string CategoryDescription { get; set; }

        /// <summary>
        /// Collection of tasks associated with the category.
        /// </summary>
        public ICollection<TaskEntity> Tasks { get; set; }
    }
}
