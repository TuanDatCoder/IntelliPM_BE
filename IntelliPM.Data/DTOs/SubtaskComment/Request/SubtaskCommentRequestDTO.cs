using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.SubtaskComment.Request
{
    public class SubtaskCommentRequestDTO
    {
        [Required]
        public string SubtaskId { get; set; } = null!;

        [Required]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [DynamicMaxLength("content_comment")]
        [DynamicMinLength("content_comment")]
        public string Content { get; set; } = null!;

        [Required(ErrorMessage = "CreatedBy is required")]
        public int CreatedBy { get; set; }

    }
}
