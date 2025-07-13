using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.EpicFile.Response
{
    public class EpicFileResponseDTO
    {
        public int Id { get; set; }

        public string EpicId { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string UrlFile { get; set; } = null!;

        public string? Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
