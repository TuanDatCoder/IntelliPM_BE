using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.MeetingParticipantRepos
{
    public class MeetingParticipantRepository : IMeetingParticipantRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public MeetingParticipantRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<MeetingParticipant> AddAsync(MeetingParticipant participant)
        {
            await _context.MeetingParticipant.AddAsync(participant);
            await _context.SaveChangesAsync();
            return participant;
        }

        public async Task<MeetingParticipant?> GetByIdAsync(int id) =>
            await _context.MeetingParticipant.FindAsync(id);

        public async Task<List<MeetingParticipant>> GetByMeetingIdAsync(int meetingId) =>
            await _context.MeetingParticipant
                .Where(mp => mp.MeetingId == meetingId)
                .ToListAsync();

        public async Task UpdateAsync(MeetingParticipant participant)
        {
            _context.MeetingParticipant.Update(participant);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(MeetingParticipant participant)
        {
            _context.MeetingParticipant.Remove(participant);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasTimeConflictAsync(int accountId, DateTime startTime, DateTime endTime, int? excludeMeetingId = null)
        {
            return await _context.MeetingParticipant
                .Where(mp => mp.AccountId == accountId
                    && mp.Meeting.StartTime < endTime
                    && mp.Meeting.EndTime > startTime
                    && (excludeMeetingId == null || mp.MeetingId != excludeMeetingId))
                .AnyAsync();
        }
    }
}