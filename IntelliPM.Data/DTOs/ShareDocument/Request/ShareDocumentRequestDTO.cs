using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ShareDocument.Request
{
    public class ShareDocumentRequestDTO
    {
        public List<string> Emails { get; set; } = new();
        public string? Message { get; set; }
        public string ProjectKey { get; set; } = string.Empty;

        public string PermissionType { get; set; }
    }

}
