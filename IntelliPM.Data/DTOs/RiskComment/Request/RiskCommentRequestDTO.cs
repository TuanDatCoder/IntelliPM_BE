using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.RiskComment.Request
{
    public class RiskCommentRequestDTO
    {
        public int RiskId { get; set; }

        public int AccountId { get; set; }

        public string Comment { get; set; } = null!;
    }
}
