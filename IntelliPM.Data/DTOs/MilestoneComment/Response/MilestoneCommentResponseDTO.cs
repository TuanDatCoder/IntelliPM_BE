using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.MilestoneComment.Response
{
    public class MilestoneCommentResponseDTO
    {
        public int Id { get; set; }
        public int MilestoneId { get; set; }
        public int AccountId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string? AccountName { get; set; }
        public string? AccountPicture { get; set; } 
    }
}
