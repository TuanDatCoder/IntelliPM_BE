using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Repositories.MilestoneFeedbackRepos
{
    public class MilestoneFeedbackRepository : IMilestoneFeedbackRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public MilestoneFeedbackRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task AddAsync(MilestoneFeedback feedback)
        {
            await _context.MilestoneFeedback.AddAsync(feedback);
            await _context.SaveChangesAsync();
        }

        public async Task<MilestoneFeedback?> GetByMeetingIdAsync(int meetingId)
        {
            return await _context.MilestoneFeedback
                .FirstOrDefaultAsync(f => f.MeetingId == meetingId);
        }

        public async Task<MilestoneFeedback?> GetByIdAsync(int id)
        {
            return await _context.MilestoneFeedback.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task UpdateAsync(MilestoneFeedback feedback)
        {
            _context.MilestoneFeedback.Update(feedback);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MilestoneFeedback>> GetByMeetingIdAndStatusAsync(int meetingId, string status)
        {
            return await _context.MilestoneFeedback
                .Where(fb => fb.MeetingId == meetingId && fb.Status == status)
                .Include(fb => fb.Account) // Để lấy tên account
                .ToListAsync();
        }
        public async Task DeleteAsync(MilestoneFeedback feedback)
        {
            _context.MilestoneFeedback.Remove(feedback);
            await _context.SaveChangesAsync();
        }
    }
}