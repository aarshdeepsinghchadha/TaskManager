using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.TaskCategory
{
    public class GetAllTaskCategoryDto
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public string CreatedByName { get; set; }
        public string UpdatedByName { get; set; }
        public bool isDeleted { get; set; }
        public List<GetAllTaskForTaskCategoryDto> Tasks { get; set; }
    }
}
