using IntelliPM.Data.DTOs.Requirement.Request;
using IntelliPM.Data.DTOs.Requirement.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.RequirementServices
{
    public interface IRequirementService
    {
        Task<List<RequirementResponseDTO>> GetAllRequirements(int projectId);
        Task<RequirementResponseDTO> GetRequirementById(int id);
        Task<List<RequirementResponseDTO>> GetRequirementByTitle(string title);
        Task<RequirementResponseDTO> CreateRequirement(RequirementRequestDTO request);
        Task<RequirementResponseDTO> UpdateRequirement(int id, RequirementRequestDTO request);
        Task DeleteRequirement(int id);
        Task<RequirementResponseDTO> ChangeRequirementPriority(int id, string priority);

       Task<List<RequirementResponseDTO>> CreateListRequirement(int projectId, List<RequirementBulkRequestDTO> requests);

    }
}
