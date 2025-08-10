using IntelliPM.Repositories.DynamicCategoryRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.Helper.DynamicCategoryHelper
{
    public class DynamicCategoryHelper : IDynamicCategoryHelper
    {
        private readonly IDynamicCategoryRepository _dynamicCategoryRepo;

        public DynamicCategoryHelper(IDynamicCategoryRepository dynamicCategoryRepo)
        {
            _dynamicCategoryRepo = dynamicCategoryRepo;
        }

        public async Task<string> GetCategoryNameAsync(string categoryGroup, string name)
        {
            var list = await _dynamicCategoryRepo.GetByCategoryGroupAsync(categoryGroup);
            var item = list.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (item == null)
                throw new InvalidOperationException($"Not find '{name}' in group '{categoryGroup}'.");
            return item.Name;
        }

        // Lấy giá trị mặc định: order index == 1 nếu có, nếu không thì lấy item có OrderIndex nhỏ nhất
        public async Task<string> GetDefaultCategoryNameAsync(string categoryGroup)
        {
            var list = await _dynamicCategoryRepo.GetByCategoryGroupAsync(categoryGroup);
            if (list == null || !list.Any())
                throw new InvalidOperationException($"No categories found in group '{categoryGroup}'.");

            var byOne = list.FirstOrDefault(x =>
            {
                var prop = x.GetType().GetProperty("OrderIndex");
                if (prop == null) return false;
                var val = prop.GetValue(x);
                return val != null && Convert.ToInt32(val) == 1;
            });

            if (byOne != null)
                return byOne.Name;

            // fallback: lấy item có OrderIndex nhỏ nhất (nếu có), hoặc đầu list nếu không có OrderIndex
            var propOrder = list.First().GetType().GetProperty("OrderIndex");
            if (propOrder != null)
            {
                var ordered = list
                    .OrderBy(x => Convert.ToInt32(propOrder.GetValue(x) ?? int.MaxValue))
                    .First();
                return ordered.Name;
            }

            return list.First().Name;
        }
    }

}
