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
    }
}
