using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.DynamicCategoryRepos
{
    public class DynamicCategoryRepository : IDynamicCategoryRepository
    {
        private readonly Su25Sep490IntelliPmContext _context;

        public DynamicCategoryRepository(Su25Sep490IntelliPmContext context)
        {
            _context = context;
        }

        public async Task<List<DynamicCategory>> GetDynamicCategories()
        {
            return await _context.DynamicCategory
                .Where(dc => dc.IsActive)
                .OrderBy(dc => dc.OrderIndex)
                .ToListAsync();
        }

        public async Task<DynamicCategory?> GetByIdAsync(int id)
        {
            return await _context.DynamicCategory
                .FirstOrDefaultAsync(dc => dc.IsActive && dc.Id == id);
        }

        public async Task<List<DynamicCategory>> GetByNameOrCategoryGroupAsync(string name, string categoryGroup)
        {
            return await _context.DynamicCategory
                .Where(dc => dc.IsActive &&
                             (string.IsNullOrEmpty(name) || dc.Name == name) &&
                             (string.IsNullOrEmpty(categoryGroup) || dc.CategoryGroup == categoryGroup))
                .OrderBy(dc => dc.OrderIndex)
                .ToListAsync();
        }
        public async Task<List<DynamicCategory>> GetByCategoryGroupAsync(string categoryGroup)
        {
            return await _context.DynamicCategory
                .Where(dc => dc.IsActive &&
                             (string.IsNullOrEmpty(categoryGroup) || dc.CategoryGroup == categoryGroup))
                .OrderBy(dc => dc.OrderIndex)
                .ToListAsync();
        }

        public async Task Add(DynamicCategory dynamicCategory)
        {
            await _context.DynamicCategory.AddAsync(dynamicCategory);
            await _context.SaveChangesAsync();
        }

        public async Task Update(DynamicCategory dynamicCategory)
        {
            _context.DynamicCategory.Update(dynamicCategory);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(DynamicCategory dynamicCategory)
        {
            _context.DynamicCategory.Remove(dynamicCategory);
            await _context.SaveChangesAsync();
        }
    }
}