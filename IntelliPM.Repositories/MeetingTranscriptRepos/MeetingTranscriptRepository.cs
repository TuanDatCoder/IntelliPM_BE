using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Repositories.MeetingTranscriptRepos
{
    public class MeetingTranscriptRepository : IMeetingTranscriptRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;


        public MeetingTranscriptRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<MeetingTranscript> AddAsync(MeetingTranscript transcript)
        {
            _context.MeetingTranscript.Add(transcript);
            await _context.SaveChangesAsync();
            return transcript;
        }

        public async Task<MeetingTranscript> GetByMeetingIdAsync(int meetingId)
        {
            return await _context.MeetingTranscript
                .FirstOrDefaultAsync(t => t.MeetingId == meetingId);
        }

        public async Task UpdateAsync(MeetingTranscript entity)
        {
            _context.MeetingTranscript.Update(entity);
            await _context.SaveChangesAsync();
        }

    }
}
