using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Repositories.MilestoneFeedbackRepos
{
    public class MilestoneFeedbackRepository : IMilestoneFeedbackRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public MilestoneFeedbackRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task AddAsync(MilestoneFeedback feedback)
        {
            await _context.MilestoneFeedback.AddAsync(feedback);
            await _context.SaveChangesAsync();
        }
    }
}