using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Document.Request
{
    public class UpdateDocumentStatusRequest
    {
        public string Status { get; set; } = "PendingApproval"; // hoặc Approved / Rejected
        public string? Comment { get; set; }
    }
}
