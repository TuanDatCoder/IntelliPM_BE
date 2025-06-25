using IntelliPM.Data.Entities;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.MeetingSummaryRepos
{
    public interface IMeetingSummaryRepository
    {
        Task<MeetingSummary> AddAsync(MeetingSummary summary);
        Task<MeetingSummary?> GetByTranscriptIdAsync(int meetingTranscriptId);
    }
}