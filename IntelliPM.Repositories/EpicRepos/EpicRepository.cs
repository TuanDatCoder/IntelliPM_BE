using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace IntelliPM.Repositories.EpicRepos
{
    public class EpicRepository : IEpicRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;
        private readonly IServiceProvider _serviceProvider;

        public EpicRepository(Su25Sep490IntelliPmContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }
        public Su25Sep490IntelliPmContext GetContext()
        {
            return _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<Su25Sep490IntelliPmContext>();
        }

        public async Task<List<Epic>> GetAllEpics()
        {
            return await _context.Epic
                .Include(e => e.Project)
                .Include(e => e.Reporter)
                .Include(e => e.Sprint)
                .Include(e => e.AssignedByNavigation)
                .Include(e => e.Sprint)
                .OrderBy(e => e.Id)
                .ToListAsync();
        }

        public async Task<Epic?> GetByIdAsync(string id)
        {
            return await _context.Epic
                .Include(e => e.Project)
                .Include(e => e.Reporter)
                .Include(e => e.Sprint)
                .Include(e => e.AssignedByNavigation)
                .Include(e => e.Sprint)
                .OrderBy(e => e.CreatedAt)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Epic>> GetByAccountIdAsync(int accountId)
        {
            return await _context.Epic
                .Include(e => e.Project)
                .Include(e => e.Reporter)
                .Include(e => e.Sprint)
                .Include(e => e.AssignedByNavigation)
                .Include(e => e.Sprint)
                .Where(e => e.AssignedBy == accountId)
                .OrderBy(e => e.Id)
                .ToListAsync();
        }

        public async Task<List<Epic>> GetByNameAsync(string name)
        {
            return await _context.Epic
                .Include(e => e.Project)
                .Include(e => e.Reporter)
                .Include(e => e.Sprint)
                .Include(e => e.AssignedByNavigation)
                .Include(e => e.Sprint)
                .Where(e => e.Name.Contains(name))
                .OrderBy(e => e.Id)
                .ToListAsync();
        }

        public async Task<List<Epic>> GetByProjectKeyAsync(string projectKey)
        {
            return await _context.Epic
                .Include(e => e.Project)
                .Include(e => e.Reporter)
                .Include(e => e.Sprint)
                .Include(e => e.AssignedByNavigation)
                .Where(e => e.Project != null && e.Project.ProjectKey == projectKey)
                .OrderBy(e => e.Id)
                .ToListAsync();
        }

        public async Task Add(Epic epic)
        {
            await _context.Epic.AddAsync(epic);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Epic epic)
        {
            _context.Epic.Update(epic);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Epic epic)
        {
            _context.Epic.Remove(epic);
            await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
        public async Task AddRangeAsync(List<Epic> epics)
        {
            await _context.Epic.AddRangeAsync(epics);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
