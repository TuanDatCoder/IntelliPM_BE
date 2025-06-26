using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskComment.Request
{
    public class TaskCommentRequestDTO
    {
        public string TaskId { get; set; } = null!;

        public int AccountId { get; set; }

        public string Content { get; set; } = null!;
    }
}
