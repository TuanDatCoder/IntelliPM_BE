using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.WorkLog.Request
{
    public class UpdateWorkLogDTO
    {
        public int WorkLogId { get; set; }
        public List<WorkLogEntryDTO> Entries { get; set; } = new();
    }
}
