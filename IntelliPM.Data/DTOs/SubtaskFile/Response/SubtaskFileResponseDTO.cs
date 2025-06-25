using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.SubtaskFile.Response
{
    public class SubtaskFileResponseDTO
    {
        public int Id { get; set; }

        public string SubtaskId { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string UrlFile { get; set; } = null!;

        public string? Status { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
