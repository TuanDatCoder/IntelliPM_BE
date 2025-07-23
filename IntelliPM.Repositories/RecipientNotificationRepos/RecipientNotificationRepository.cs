using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.RecipientNotificationRepos
{
    public class RecipientNotificationRepository : IRecipientNotificationRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public RecipientNotificationRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }
        public async Task<List<RecipientNotification>> GetAllRecipientNotification()
        {
            return await _context.RecipientNotification
                .Include(v => v.Account)
                .Include(v => v.Notification)
                .OrderBy(t => t.Id)
                .ToListAsync();
        }

        public async Task<RecipientNotification?> GetByIdAsync(int id)
        {
            return await _context.RecipientNotification
                .Include(s => s.Account)
                .Include(v => v.Notification)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<RecipientNotification?> GetByAccountAndNotificationIdAsync(int accountId, int notificationId)
        {
            return await _context.RecipientNotification
                .Include(v => v.Notification)
                .FirstOrDefaultAsync(r => r.AccountId == accountId && r.NotificationId == notificationId);
        }

        public async Task MarkAsReadAsync(int accountId, int notificationId)
        {
            var entity = await GetByAccountAndNotificationIdAsync(accountId, notificationId);
            if (entity != null)
            {
                entity.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<RecipientNotification>> GetRecipientNotificationByAccountIdAsync(int accountId)
        {
            return await _context.RecipientNotification
                .Where(tf => tf.AccountId == accountId)
                .Include(r => r.Notification)
                .Include(t => t.Account)
                .OrderByDescending(tf => tf.CreatedAt)
                .ToListAsync();
        }

    }
}
