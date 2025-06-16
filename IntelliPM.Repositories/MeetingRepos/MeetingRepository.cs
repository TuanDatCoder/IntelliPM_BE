using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.MeetingRepos
{
    public class MeetingRepository : IMeetingRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public MeetingRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<Meeting> AddAsync(Meeting meeting)
        {
            _context.Meeting.Add(meeting);  // Thêm cuộc họp vào DbSet
            return meeting;
        }

        public async Task<Meeting?> GetByIdAsync(int id) =>
            await _context.Meeting.FindAsync(id);  // Tìm cuộc họp theo ID

        public async Task<List<Meeting>> GetByAccountIdAsync(int accountId)
        {
            return await _context.Meeting.ToListAsync();  // Lấy tất cả cuộc họp (có thể thay đổi tùy theo logic)
        }
        public async Task<List<Meeting>> GetMeetingsByAccountIdDetailedAsync(int accountId)
        {
            return await _context.MeetingParticipant
                .Where(mp => mp.AccountId == accountId)
                .Include(mp => mp.Meeting)
                .ThenInclude(m => m.Project)
                .Select(mp => mp.Meeting)
                .ToListAsync();
        }

        public async Task UpdateAsync(Meeting meeting)
        {
            _context.Meeting.Update(meeting);  // Cập nhật cuộc họp vào DbSet
        }

        public async Task DeleteAsync(Meeting meeting)
        {
            _context.Meeting.Remove(meeting);  // Xóa cuộc họp khỏi DbSet
        }
    }
}
