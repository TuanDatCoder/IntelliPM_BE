using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Ai.GenerateStoryTask.Request
{
    public class GenerateStoryTaskRequestDTO
    {
        [DynamicCategoryValidation("task_type", Required = false)]
        public string Type { get; set; }
        public string EpicTitle { get; set; }
        public DateTime EpicStartDate { get; set; }
        public DateTime EpicEndDate { get; set; }
        public string? StoryTitle { get; set; }
        public DateTime? StoryStartDate { get; set; }
        public DateTime? StoryEndDate { get; set; }
        public List<string>? ExistingTitles { get; set; }
    }
}
