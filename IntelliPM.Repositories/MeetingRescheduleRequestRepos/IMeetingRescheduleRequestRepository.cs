using IntelliPM.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.MeetingRescheduleRequestRepos
{
    public interface IMeetingRescheduleRequestRepository
    {
        Task<List<MeetingRescheduleRequest>> GetAllAsync();
        Task<MeetingRescheduleRequest?> GetByIdAsync(int id);
        Task<MeetingRescheduleRequest> AddAsync(MeetingRescheduleRequest request);
        Task UpdateAsync(MeetingRescheduleRequest request);
        Task DeleteAsync(MeetingRescheduleRequest request);
    }
}