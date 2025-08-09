using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.DocumentRequestMeetingRepos
{
    public class DocumentRequestMeetingRepository : IDocumentRequestMeetingRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public DocumentRequestMeetingRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DocumentRequestMeeting entity) =>
            await _context.DocumentRequestMeeting.AddAsync(entity);

        public async Task<DocumentRequestMeeting?> GetByIdAsync(int id) =>
            await _context.DocumentRequestMeeting.FindAsync(id);

        public async Task<List<DocumentRequestMeeting>> GetAllAsync() =>
            await _context.DocumentRequestMeeting.ToListAsync();

        public async Task UpdateAsync(DocumentRequestMeeting entity) =>
            _context.DocumentRequestMeeting.Update(entity);

        public async Task DeleteAsync(DocumentRequestMeeting entity) =>
            _context.DocumentRequestMeeting.Remove(entity);

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();

        public async Task<List<DocumentRequestMeeting>> GetByProjectManagerAsync(
    int pmId, string? status = null, bool? sentToClient = null, bool? clientViewed = null,
    int? skip = null, int? take = null)
        {
            var q = _context.DocumentRequestMeeting.AsQueryable()
                .Where(x => x.ProjectManagerId == pmId);

            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(x => x.Status == status);
            if (sentToClient.HasValue) q = q.Where(x => x.SentToClient == sentToClient.Value);
            if (clientViewed.HasValue) q = q.Where(x => x.ClientViewed == clientViewed.Value);

            q = q.OrderByDescending(x => x.UpdatedAt);

            if (skip.HasValue) q = q.Skip(skip.Value);
            if (take.HasValue) q = q.Take(take.Value);

            return await q.ToListAsync();
        }
    }


}
