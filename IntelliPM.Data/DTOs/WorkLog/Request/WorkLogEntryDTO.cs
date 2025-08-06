using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.WorkLog.Request
{
    public class WorkLogEntryDTO
    {
        public int AccountId { get; set; }
        public decimal Hours { get; set; }
    }
}
