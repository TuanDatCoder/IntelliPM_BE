using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.NotificationRepos
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public NotificationRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }
        public async Task Add(Notification notification)
        {
            await _context.Notification.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetNotificationByAccountIdAsync(int accountId)
        {
            return await _context.Notification
                .Where(tf => tf.CreatedBy == accountId)
                .Include(t => t.CreatedByNavigation)
                .OrderByDescending(tf => tf.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetAllNotification()
        {
            return await _context.Notification
                .Include(t => t.CreatedByNavigation)
                .ToListAsync();
        }
    }
}
