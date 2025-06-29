using IntelliPM.Data.DTOs.Meeting.Request;
using IntelliPM.Data.DTOs.Meeting.Response;
using IntelliPM.Data.DTOs.MeetingLog.Request;
using IntelliPM.Data.DTOs.MeetingLog.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Services.MeetingLogServices;

public interface IMeetingLogService
{
    Task<MeetingLogResponseDTO> AddLogAsync(MeetingLogRequestDTO dto);
    Task<List<MeetingLogResponseDTO>> GetLogsByMeetingIdAsync(int meetingId);
    Task<List<MeetingLogResponseDTO>> GetLogsByAccountIdAsync(int accountId);
    Task<List<MeetingLogResponseDTO>> GetAllLogsAsync();

    
}