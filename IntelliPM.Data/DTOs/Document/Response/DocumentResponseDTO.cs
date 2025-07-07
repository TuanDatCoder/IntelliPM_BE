using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Document.Response
{
    public class DocumentResponseDTO
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string? TaskId { get; set; }
        public string Title { get; set; } = "";
        public string? Type { get; set; }
        public string? Template { get; set; }
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


    }

}
