using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Document.Request
{
    public class UpdateDocumentRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public string Visibility { get; set; }


    }
}
