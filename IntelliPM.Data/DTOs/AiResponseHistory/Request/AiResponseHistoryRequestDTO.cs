using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.AiResponseHistory.Request
{
    public class AiResponseHistoryRequestDTO
    {
        public string AiFeature { get; set; }
        public int? ProjectId { get; set; }
        public string ResponseJson { get; set; }
        public string Status { get; set; }
    }
}
