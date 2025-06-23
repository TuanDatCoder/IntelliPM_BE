using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.MeetingLogRepos;

public class MeetingLogRepository : IMeetingLogRepository
{
    private readonly Su25Sep490IntelliPmContext _context;

    public MeetingLogRepository(Su25Sep490IntelliPmContext context)
    {
        _context = context;
    }

    public async Task<MeetingLog> AddAsync(MeetingLog log)
    {
        _context.MeetingLog.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<List<MeetingLog>> GetByMeetingIdAsync(int meetingId)
    {
        return await _context.MeetingLog
            .Include(log => log.Account)
            .Where(log => log.MeetingId == meetingId)
            .ToListAsync();
    }

    public async Task<List<MeetingLog>> GetByAccountIdAsync(int accountId)
    {
        return await _context.MeetingLog
            .Include(log => log.Account)
            .Where(log => log.AccountId == accountId)
            .ToListAsync();
    }

    public async Task<List<MeetingLog>> GetAllAsync()
    {
        return await _context.MeetingLog
            .Include(log => log.Account)
            .ToListAsync();
    }
}