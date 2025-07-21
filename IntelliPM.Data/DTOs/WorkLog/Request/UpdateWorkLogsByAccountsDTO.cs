using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.WorkLog.Request
{
    public class UpdateWorkLogsByAccountsDTO
    {
        public string TaskId { get; set; }
        public List<UpdateWorkLogDTO> WorkLogs { get; set; } = new();
    }

}
