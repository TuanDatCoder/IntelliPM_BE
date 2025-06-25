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
    }
}