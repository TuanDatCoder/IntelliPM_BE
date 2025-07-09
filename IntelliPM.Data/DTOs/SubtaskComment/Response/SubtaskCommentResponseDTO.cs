using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.SubtaskComment.Response
{
    public class SubtaskCommentResponseDTO
    {
        public int Id { get; set; }

        public string SubtaskId { get; set; } = null!;

        public int AccountId { get; set; }

        public string AccountName { get; set; }

        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

    }
}
