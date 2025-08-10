using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.DocumentComment
{
    public class DocumentCommentResponseDTO
    {
        public int Id { get; set; }                      
        public int DocumentId { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = null!;
        public string? AuthorAvatar { get; set; }
        public int FromPos { get; set; }
        public int ToPos { get; set; }
        public string Content { get; set; } = null!;
        public string Comment { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
