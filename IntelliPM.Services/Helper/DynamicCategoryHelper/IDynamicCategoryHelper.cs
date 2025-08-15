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
        Task<List<string>> GetEpicStatusesAsync();
        Task<List<string>> GetSubtaskStatusesAsync();
        Task<List<string>> GetTaskStatusesAsync();
        Task<List<string>> GetTaskTypesAsync();
        Task<List<string>> GetSubtaskTypesAsync();
        Task<List<string>> GetEpicTypesAsync();
        Task<List<string>> GetTaskPrioritiesAsync();
        Task<List<string>> GetSubtaskPrioritiesAsync();
    }
}
