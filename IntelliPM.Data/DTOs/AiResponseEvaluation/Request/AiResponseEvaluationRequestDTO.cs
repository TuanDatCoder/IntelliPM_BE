using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.AiResponseEvaluation.Request
{
    public class AiResponseEvaluationRequestDTO
    {
        public int AiResponseId { get; set; }
        public int Rating { get; set; }
        public string? Feedback { get; set; }
    }
}
