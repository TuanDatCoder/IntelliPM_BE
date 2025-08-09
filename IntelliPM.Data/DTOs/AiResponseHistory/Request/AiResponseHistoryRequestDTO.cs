using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.AiResponseHistory.Request
{
    public class AiResponseHistoryRequestDTO
    {
        [DynamicCategoryValidation("ai_feature", Required = true)]
        public string AiFeature { get; set; }
        public int? ProjectId { get; set; }
        public string ResponseJson { get; set; }
        [DynamicCategoryValidation("ai_history_status", Required = true)]
        public string Status { get; set; }
    }
}
