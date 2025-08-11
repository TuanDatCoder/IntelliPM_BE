using System.ComponentModel.DataAnnotations;

namespace IntelliPM.Data.DTOs.DocumentComment
{
    public class UpdateDocumentCommentRequestDTO
    {
        public int? FromPos { get; set; }
        public int? ToPos { get; set; }
        [MaxLength(2000)] public string? Content { get; set; }
        [MaxLength(4000)] public string? Comment { get; set; }
    }
}
