using IntelliPM.Data.DTOs.Label.Response;
using IntelliPM.Data.DTOs.WorkItemLabel.Request;
using IntelliPM.Data.DTOs.WorkItemLabel.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.WorkItemLabelServices
{
    public interface IWorkItemLabelService
    {
        Task<WorkItemLabelResponseDTO> CreateWorkItemLabel(WorkItemLabelRequestDTO request);
        Task DeleteWorkItemLabel(int id);
        Task<List<WorkItemLabelResponseDTO>> GetAllWorkItemLabelAsync(int page = 1, int pageSize = 10);
        Task<WorkItemLabelResponseDTO> GetWorkItemLabelById(int id);
        Task<WorkItemLabelResponseDTO> UpdateWorkItemLabel(int id, WorkItemLabelRequestDTO request);
        Task<List<WorkItemLabelResponseDTO>> GetByEpicIdAsync(string? epicId);
        Task<List<WorkItemLabelResponseDTO>> GetBySubtaskIdAsync(string? subtaskId);
        Task<List<WorkItemLabelResponseDTO>> GetByTaskIdAsync(string? taskId);
        Task<LabelResponseDTO> GetLabelById(int labelId);
    }
}
