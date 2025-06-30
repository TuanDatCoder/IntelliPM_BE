using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Label.Response
{
    public class LabelResponseDTO
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; } = null!;
        public string? ColorCode { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
    }
}
