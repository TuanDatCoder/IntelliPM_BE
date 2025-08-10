using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.DocumentComment
{
    public class DocumentCommentRequestDTO
    {
        public int DocumentId { get; set; }
        public string Content { get; set; } = null!;
    }
}
