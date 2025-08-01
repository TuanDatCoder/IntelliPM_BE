using IntelliPM.Data.DTOs.Label.Request;
using IntelliPM.Data.DTOs.Label.Response;
using IntelliPM.Data.DTOs.WorkItemLabel.Response;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.LabelServices
{
    public interface ILabelService
    {
        Task<LabelResponseDTO> CreateLabel(LabelRequestDTO request);
        Task DeleteLabel(int id);
        Task<List<LabelResponseDTO>> GetAllLabelAsync(int page = 1, int pageSize = 10);
        Task<LabelResponseDTO> GetLabelById(int id);
        Task<LabelResponseDTO> UpdateLabel(int id, LabelRequestDTO request);
        Task<WorkItemLabelResponseDTO> CreateLabelAndAssignAsync(CreateLabelAndAssignDTO dto);
        Task<List<LabelResponseDTO>> GetLabelByProject(int projectId);
    }
}
