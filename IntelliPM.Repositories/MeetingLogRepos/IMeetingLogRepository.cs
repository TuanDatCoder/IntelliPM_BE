using IntelliPM.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.MeetingLogRepos;

public interface IMeetingLogRepository
{
    Task<MeetingLog> AddAsync(MeetingLog log);
    Task<List<MeetingLog>> GetByMeetingIdAsync(int meetingId);
    Task<List<MeetingLog>> GetByAccountIdAsync(int accountId);
    Task<List<MeetingLog>> GetAllAsync();
}