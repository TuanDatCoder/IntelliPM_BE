using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.DynamicCategoryRepos
{
    public interface IDynamicCategoryRepository
    {
        Task<List<DynamicCategory>> GetDynamicCategories();
        Task<DynamicCategory> GetByIdAsync(int id);
        Task<List<DynamicCategory>> GetByNameOrCategoryGroupAsync(string name, string categoryGroup);
        Task<List<DynamicCategory>> GetByCategoryGroupAsync(string categoryGroup);
        Task Add(DynamicCategory dynamicCategory);
        Task Update(DynamicCategory dynamicCategory);
        Task Delete(DynamicCategory dynamicCategory);
        Task<List<string>> GetDistinctCategoryGroupsAsync();

    }
}