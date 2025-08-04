using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ShareDocumentViaEmail
{
    public class ShareDocumentViaEmailRequest
    {
        //public int DocumentId { get; set; }

        public List<int> UserIds { get; set; } = new();

        public string? CustomMessage { get; set; }

        public IFormFile File { get; set; } = null!;
    }
}
