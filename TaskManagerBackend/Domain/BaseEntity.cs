using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    /// <summary>
    /// BaseEntity represents common properties for all entities
    /// </summary>
    public class BaseEntity
    {
        /// <summary>
        /// Date and time when the entity was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Date and time when the entity was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        /// <summary>
        /// Flag indicating whether the entity is deleted
        /// </summary>
        public bool IsDeleted { get; set; }
        /// <summary>
        /// ID of the user who created the entity
        /// </summary>
        public string CreatedById { get; set; }
        /// <summary>
        /// ID of the user who last updated the entity
        /// </summary>
        public string? UpdatedById { get; set; }
        /// <summary>
        /// ID of the user who deleted the entity
        /// </summary>
        public string? DeletedBy { get; set; }

        /// Navigation properties to link to AppUser

        /// <summary>
        /// User who created the entity
        /// </summary>
        public AppUser CreatedByAppUser { get; set; }
        /// <summary>
        /// User who last updated the entity
        /// </summary>
        public AppUser? UpdatedByAppUser { get; set; }
        /// <summary>
        /// User who deleted the entity
        /// </summary>
        public AppUser? DeletedByAppUser { get; set; }
    }
}
