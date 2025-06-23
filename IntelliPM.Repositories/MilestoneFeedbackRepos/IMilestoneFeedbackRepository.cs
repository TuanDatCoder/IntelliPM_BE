using IntelliPM.Data.Entities;

namespace IntelliPM.Repositories.MilestoneFeedbackRepos
{
    public interface IMilestoneFeedbackRepository
    {
        Task AddAsync(MilestoneFeedback feedback);
        Task<MilestoneFeedback?> GetByMeetingIdAsync(int meetingId);

        Task<MilestoneFeedback?> GetByIdAsync(int id);
        Task UpdateAsync(MilestoneFeedback feedback);
        Task DeleteAsync(MilestoneFeedback feedback);
    }
}