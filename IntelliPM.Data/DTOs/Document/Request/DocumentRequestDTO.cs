using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Document.Request
{
    public class DocumentRequestDTO
    {
        public int ProjectId { get; set; }
        public int? TaskId { get; set; }
        public string Title { get; set; } = "";
        public string? Type { get; set; }
        public string? Template { get; set; }
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public int CreatedBy { get; set; }
    }
}
