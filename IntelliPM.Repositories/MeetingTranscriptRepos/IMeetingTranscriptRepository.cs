using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.MeetingTranscriptRepos
{

    public interface IMeetingTranscriptRepository
    {
        Task<MeetingTranscript> AddAsync(MeetingTranscript transcript);
        Task<MeetingTranscript> GetByMeetingIdAsync(int meetingId);

        Task UpdateAsync(MeetingTranscript entity);
    }
}
