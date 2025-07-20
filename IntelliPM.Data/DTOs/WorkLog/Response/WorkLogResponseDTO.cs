using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.WorkLog.Response
{
    public class WorkLogResponseDTO
    {
        public int Id { get; set; }

        public string? TaskId { get; set; }

        public string? SubtaskId { get; set; }

        public DateTime LogDate { get; set; }

        public decimal? Hours { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public List<AccountBasicDTO>? Accounts { get; set; }

    }
}
