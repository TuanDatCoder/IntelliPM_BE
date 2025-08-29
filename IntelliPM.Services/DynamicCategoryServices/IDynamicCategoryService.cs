using IntelliPM.Data.DTOs.DynamicCategory.Request;
using IntelliPM.Data.DTOs.DynamicCategory.Response;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliPM.Services.DynamicCategoryServices
{
    public interface IDynamicCategoryService
    {
        Task<List<DynamicCategoryResponseDTO>> GetAllDynamicCategories();
        Task<DynamicCategoryResponseDTO> GetDynamicCategoryById(int id);
        Task<List<DynamicCategoryResponseDTO>> GetDynamicCategoryByNameOrCategoryGroup(string name, string categoryGroup);
        Task<List<DynamicCategoryResponseDTO>> GetDynamicCategoryByCategoryGroup(string categoryGroup);
        Task<DynamicCategoryResponseDTO> CreateDynamicCategory(DynamicCategoryRequestDTO request);
        Task<DynamicCategoryResponseDTO> UpdateDynamicCategory(int id, DynamicCategoryRequestDTO request);
        Task DeleteDynamicCategory(int id);
        Task<DynamicCategoryResponseDTO> ChangeDynamicCategoryStatus(int id, bool isActive);
        Task<List<string>> GetDistinctCategoryGroups();

    }
}