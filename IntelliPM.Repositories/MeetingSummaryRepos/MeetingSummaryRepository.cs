using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.MeetingSummaryRepos
{
    public class MeetingSummaryRepository : IMeetingSummaryRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public MeetingSummaryRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<MeetingSummary> AddAsync(MeetingSummary summary)
        {
            _context.MeetingSummary.Add(summary);
            await _context.SaveChangesAsync();
            return summary;
        }

        public async Task<MeetingSummary?> GetByTranscriptIdAsync(int meetingTranscriptId)
        {
            return await _context.MeetingSummary
                .FirstOrDefaultAsync(x => x.MeetingTranscriptId == meetingTranscriptId);
        }

        public async Task<List<MeetingSummary>> GetByAccountIdAsync(int accountId)
        {
            // Lấy các meetingId mà account này tham gia
            var meetingIds = _context.MeetingParticipant
                .Where(mp => mp.AccountId == accountId)
                .Select(mp => mp.MeetingId);

            // Lấy transcriptId của các meeting đó
            var transcriptIds = _context.MeetingTranscript
    .Where(mt => meetingIds.Contains(mt.MeetingId))
    .Select(mt => mt.MeetingId);

            // Lấy summary theo transcriptId
            return await _context.MeetingSummary
                .Where(ms => transcriptIds.Contains(ms.MeetingTranscriptId))
                .ToListAsync();
        }
        public async Task UpdateAsync(MeetingSummary entity)
        {
            _context.MeetingSummary.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}