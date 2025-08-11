using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.AiResponseEvaluation.Response
{
    public class AiResponseEvaluationResponseDTO
    {
        public int Id { get; set; }
        public int AiResponseId { get; set; }
        public int AccountId { get; set; }
        public string AccountFullname { get; set; }
        public string AccountPicture { get; set; }
        public int Rating { get; set; }
        public string? Feedback { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
