using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.DocumentComment
{
    public class DocumentCommentRequestDTO
    {
        public int DocumentId { get; set; }              // ID của document
        public int FromPos { get; set; }                 // Vị trí bắt đầu đoạn comment
        public int ToPos { get; set; }                   // Vị trí kết thúc
        public string Content { get; set; } = null!;     // Đoạn text được chọn
        public string Comment { get; set; } = null!;     // Nội dung người dùng nhập
    }
}
