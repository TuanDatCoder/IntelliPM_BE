using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskComment.Response
{
    public class TaskCommentResponseDTO
    {
        public int Id { get; set; }

        public string TaskId { get; set; } = null!;

        public int AccountId { get; set; }

        public string AccountName { get; set; }

        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
