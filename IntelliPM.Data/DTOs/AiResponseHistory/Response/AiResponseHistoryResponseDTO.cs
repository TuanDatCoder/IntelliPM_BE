using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.AiResponseHistory.Response
{
    public class AiResponseHistoryResponseDTO
    {
        public int Id { get; set; }
        public string AiFeature { get; set; }
        public int? ProjectId { get; set; }
        public string ResponseJson { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByFullname { get; set; }
        public string CreatedByPicture { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Status { get; set; }
    }
}
