using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Repositories.MeetingDocumentRepos
{
    public class MeetingDocumentRepository : IMeetingDocumentRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public MeetingDocumentRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }


        public async Task<List<MeetingDocument>> GetAllAsync()
        {
            return await _context.Set<MeetingDocument>().ToListAsync();
        }

        public async Task<MeetingDocument?> GetByMeetingIdAsync(int meetingId)
        {
            return await _context.Set<MeetingDocument>().FirstOrDefaultAsync(md => md.MeetingId == meetingId);
        }

        public async Task AddAsync(MeetingDocument doc)
        {
            await _context.Set<MeetingDocument>().AddAsync(doc);
        }

        public async Task UpdateAsync(MeetingDocument doc)
        {
            _context.Set<MeetingDocument>().Update(doc);
        }

        public async Task DeleteAsync(MeetingDocument doc)
        {
            _context.Set<MeetingDocument>().Remove(doc);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}