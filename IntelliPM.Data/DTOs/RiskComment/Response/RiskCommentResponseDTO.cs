using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.RiskComment.Response
{
    public class RiskCommentResponseDTO
    {
        public int Id { get; set; }

        public int RiskId { get; set; }

        public int AccountId { get; set; }

        public string AccountUsername { get; set; }

        public string AccountFullname { get; set; }

        public string AccountPicture { get; set; }

        public string Comment { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
