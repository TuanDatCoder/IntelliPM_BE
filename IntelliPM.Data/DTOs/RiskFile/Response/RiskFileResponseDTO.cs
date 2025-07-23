using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.RiskFile.Response
{
    public class RiskFileResponseDTO
    {
        public int Id { get; set; }

        public int RiskId { get; set; }

        public string FileName { get; set; } = null!;

        public string FileUrl { get; set; } = null!;

        public int UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}
