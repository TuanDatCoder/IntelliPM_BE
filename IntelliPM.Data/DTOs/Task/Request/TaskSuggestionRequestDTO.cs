using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Task.Request
{
    public class TaskSuggestionRequestDTO
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "TO_DO";
        public bool ManualInput { get; set; } = false;
        public bool GenerationAiInput { get; set; } = true;
        public string? Type { get; set; }
    }

}
