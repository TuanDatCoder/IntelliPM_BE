using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ShareDocument.Response
{
    public class ShareDocumentResponseDTO
    {
        public bool Success { get; set; }
        public List<string> FailedEmails { get; set; } = new();
    }

}
