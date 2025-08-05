using IntelliPM.Data.Entities;

namespace IntelliPM.Repositories.MeetingDocumentRepos
{
    public interface IMeetingDocumentRepository
    {
        Task<List<MeetingDocument>> GetAllAsync();
        Task<MeetingDocument?> GetByMeetingIdAsync(int meetingId);
        Task AddAsync(MeetingDocument doc);
        Task UpdateAsync(MeetingDocument doc);
        Task DeleteAsync(MeetingDocument doc);
        Task SaveChangesAsync();
    }
}