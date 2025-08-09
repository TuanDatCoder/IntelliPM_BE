using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.Helper.DynamicCategoryHelper
{
    public interface IDynamicCategoryHelper
    {
        Task<string> GetCategoryNameAsync(string categoryGroup, string name);
        Task<string> GetDefaultCategoryNameAsync(string categoryGroup);
    }
}
