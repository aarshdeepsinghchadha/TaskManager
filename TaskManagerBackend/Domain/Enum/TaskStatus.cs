using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enum
{
    public enum TaskStatusEnum
    {
        NotStarted,
        InProgress,
        Completed,
        Deferred, // Indicates that the task has been postponed or deferred for some reason.
        Cancelled
    }
}
