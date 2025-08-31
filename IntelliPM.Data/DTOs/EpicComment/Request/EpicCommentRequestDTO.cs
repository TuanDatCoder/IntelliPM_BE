using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.EpicComment.Request
{
    public class EpicCommentRequestDTO
    {
        [Required(ErrorMessage = "EpicId is required")]
        public string EpicId { get; set; } = null!;

        [Required(ErrorMessage = "AccountId is required")]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [DynamicMaxLength("content_comment")]
        [DynamicMinLength("content_comment")]
        public string Content { get; set; } = null!;

        [Required(ErrorMessage = "CreatedBy is required")]
        public int CreatedBy { get; set; }
    }
}
