using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.DocumentRequestMeetingRepos
{
    public interface IDocumentRequestMeetingRepository
    {
        Task AddAsync(DocumentRequestMeeting entity);
        Task<DocumentRequestMeeting?> GetByIdAsync(int id);
        Task<List<DocumentRequestMeeting>> GetAllAsync();
        Task UpdateAsync(DocumentRequestMeeting entity);
        Task DeleteAsync(DocumentRequestMeeting entity);
        Task SaveChangesAsync();
    }

}
