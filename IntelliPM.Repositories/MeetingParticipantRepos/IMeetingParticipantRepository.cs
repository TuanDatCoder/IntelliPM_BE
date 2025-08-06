using IntelliPM.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.MeetingParticipantRepos
{
    public interface IMeetingParticipantRepository
    {
        Task<MeetingParticipant> AddAsync(MeetingParticipant participant);
        Task<MeetingParticipant?> GetByIdAsync(int id);
        Task<List<MeetingParticipant>> GetByMeetingIdAsync(int meetingId);
        Task UpdateAsync(MeetingParticipant participant);
        Task DeleteAsync(MeetingParticipant participant);
        Task<bool> HasTimeConflictAsync(int accountId, DateTime startTime, DateTime endTime, int? excludeMeetingId = null);
    }
}
