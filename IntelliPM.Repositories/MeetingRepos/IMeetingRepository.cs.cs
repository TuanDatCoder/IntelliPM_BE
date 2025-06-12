

using IntelliPM.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.MeetingRepos
{
    public interface IMeetingRepository
    {
        Task<Meeting> AddAsync(Meeting meeting);
        Task<Meeting?> GetByIdAsync(int id);
        Task<List<Meeting>> GetByAccountIdAsync(int accountId);
        Task UpdateAsync(Meeting meeting);
        Task DeleteAsync(Meeting meeting);
    }
}


