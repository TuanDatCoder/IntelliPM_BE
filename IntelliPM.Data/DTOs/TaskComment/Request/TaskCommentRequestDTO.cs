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
        public int TaskId { get; set; }

        public int UserId { get; set; }

        public string Content { get; set; } = null!;
    }
}
