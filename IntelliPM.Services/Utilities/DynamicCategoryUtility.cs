using IntelliPM.Data.Entities;
using IntelliPM.Repositories.DynamicCategoryRepos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliPM.Services.Utilities
{
    public static class DynamicCategoryUtility
    {

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