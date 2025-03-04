using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Domain.DTOs
{
    public class TaskDto
    {
        public int TaskId { get; set; }

        public int? UserId { get; set; }

        public string TaskName { get; set; }

        public string TaskDescription { get; set; }

        public int? Duration { get; set; }

        public string AiProcessedDescription { get; set; }

        public int? TimeOverdue { get; set; }
        public DateTime? CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public TasksStatus Status { get; set; }

        public virtual Users User { get; set; }
    }
}
