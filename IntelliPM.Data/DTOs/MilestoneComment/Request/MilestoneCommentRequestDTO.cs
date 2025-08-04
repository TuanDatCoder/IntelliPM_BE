using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.MilestoneComment.Request
{
    public class MilestoneCommentRequestDTO
    {
        public int MilestoneId { get; set; }
        public int AccountId { get; set; }
        public string Content { get; set; } = null!;
    }
}
