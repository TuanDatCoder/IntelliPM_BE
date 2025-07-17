using IntelliPM.Data.DTOs.WorkLog.Response;
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
        Task<List<WorkLogResponseDTO>> GetWorkLogsByTaskOrSubtaskAsync(string? taskId, string? subtaskId);
        Task<WorkLogResponseDTO> ChangeWorkLogHoursAsync(int id, decimal hours);
    }
}
