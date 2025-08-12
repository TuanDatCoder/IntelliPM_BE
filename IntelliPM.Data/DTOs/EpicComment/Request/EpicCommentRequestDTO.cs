using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.EpicComment.Request
{
    public class EpicCommentRequestDTO
    {
        public string EpicId { get; set; } = null!;
        public int AccountId { get; set; }
        public string Content { get; set; } = null!;
        public int CreatedBy { get; set; }
    }
}
