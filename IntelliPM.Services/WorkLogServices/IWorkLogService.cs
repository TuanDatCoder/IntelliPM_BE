using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.WorkLogServices
{
    public interface IWorkLogService
    {
        Task GenerateDailyWorkLogsAsync();
    }
}
