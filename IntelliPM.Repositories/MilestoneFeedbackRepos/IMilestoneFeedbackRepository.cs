using IntelliPM.Data.Entities;

namespace IntelliPM.Repositories.MilestoneFeedbackRepos
{
    public interface IMilestoneFeedbackRepository
    {
        Task AddAsync(MilestoneFeedback feedback);
    }
}