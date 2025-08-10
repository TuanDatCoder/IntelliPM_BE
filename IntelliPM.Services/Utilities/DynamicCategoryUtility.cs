using IntelliPM.Data.Entities;
using IntelliPM.Repositories.DynamicCategoryRepos;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliPM.Services.Utilities
{
    public static class DynamicCategoryUtility
    {
        private static readonly ConcurrentDictionary<string, DynamicCategory[]> _categoryCache = new();
        private static IMemoryCache _memoryCache;
        private static bool _isInitialized;

        public static async Task InitializeAsync(IDynamicCategoryRepository repo)
        {
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            var allCategories = await repo.GetDynamicCategories();
            var groupedCategories = allCategories
                .GroupBy(c => c.CategoryGroup, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.OrdinalIgnoreCase);

            foreach (var group in groupedCategories)
            {
                _categoryCache[group.Key] = group.Value;
            }

            _isInitialized = true;
        }

        public static void SetMemoryCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        

        public static async Task RefreshCacheAsync(IDynamicCategoryRepository repo, string categoryGroup)
        {
            if (repo == null)
                throw new ArgumentNullException(nameof(repo));

            var categories = await repo.GetByCategoryGroupAsync(categoryGroup);
            _categoryCache[categoryGroup] = categories.ToArray();
            if (_memoryCache != null)
            {
                _memoryCache.Set(categoryGroup, categories, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                });
            }
        }


        public static async Task<string> ValidateAndMapAsync(
     IDynamicCategoryRepository categoryRepo,
     string categoryGroup,
     string? value,
     bool isRequired = true)
        {
            if (categoryRepo == null)
                throw new ArgumentNullException(nameof(categoryRepo), "Dynamic category repository cannot be null.");

            if (string.IsNullOrWhiteSpace(categoryGroup))
                throw new ArgumentException("CategoryGroup cannot be null or empty.", nameof(categoryGroup));

            if (isRequired && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Value for CategoryGroup '{categoryGroup}' is required but was null or empty.");

            if (string.IsNullOrWhiteSpace(value))
                return string.Empty; // Return empty string if not required and value is null/empty

            var categories = await categoryRepo.GetByCategoryGroupAsync(categoryGroup);
            var matchedCategory = categories.FirstOrDefault(c =>
                c.IsActive &&
                (string.Equals(c.Name, value, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(c.Label, value, StringComparison.OrdinalIgnoreCase)));

            if (matchedCategory == null)
                throw new ArgumentException($"Invalid {categoryGroup}: {value} is not a valid value.");

            return matchedCategory.Name; // Return the Name (e.g., "HIGH" for "High")
        }

    }
}