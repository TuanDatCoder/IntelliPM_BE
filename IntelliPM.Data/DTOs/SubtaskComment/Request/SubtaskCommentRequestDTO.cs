using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.SubtaskComment.Request
{
    public class SubtaskCommentRequestDTO
    {
        public string SubtaskId { get; set; } = null!;

        public int AccountId { get; set; }

        public string Content { get; set; } = null!;

        public int CreatedBy { get; set; }

    }
}
