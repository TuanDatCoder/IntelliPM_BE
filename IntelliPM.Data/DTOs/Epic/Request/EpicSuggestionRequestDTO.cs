using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Epic.Request
{
    public class EpicSuggestionRequestDTO
    {
        public string? Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "TO_DO";
    }
}
