using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.MeetingRescheduleRequestRepos
{
    public class MeetingRescheduleRequestRepository : IMeetingRescheduleRequestRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public MeetingRescheduleRequestRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<MeetingRescheduleRequest>> GetAllAsync()
        {
            return await _context.MeetingRescheduleRequest.ToListAsync();
        }

        public async Task<MeetingRescheduleRequest?> GetByIdAsync(int id)
        {
            return await _context.MeetingRescheduleRequest.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<MeetingRescheduleRequest> AddAsync(MeetingRescheduleRequest request)
        {
            _context.MeetingRescheduleRequest.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task UpdateAsync(MeetingRescheduleRequest request)
        {
            _context.MeetingRescheduleRequest.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(MeetingRescheduleRequest request)
        {
            _context.MeetingRescheduleRequest.Remove(request);
            await _context.SaveChangesAsync();
        }
    }
}