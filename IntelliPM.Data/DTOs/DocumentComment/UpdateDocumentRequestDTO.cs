using IntelliPM.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace IntelliPM.Data.DTOs.DocumentComment
{
    public class UpdateDocumentCommentRequestDTO
    {
        public int? FromPos { get; set; }
        public int? ToPos { get; set; } public string? Content { get; set; }

        [DynamicMinLength("comment_length")]
        [DynamicMaxLength("comment_length")]
        public string? Comment { get; set; }
    }
}
