using IntelliPM.Data.DTOs.Risk.Request;
using IntelliPM.Data.DTOs.Risk.Response;
using IntelliPM.Data.DTOs.RiskSolution.Request;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.RiskServices
{
    public interface IRiskService
    {
        Task<List<RiskResponseDTO>> GetAllRisksAsync();
        Task<List<RiskResponseDTO>> GetByProjectIdAsync(int projectId);
        Task<List<RiskResponseDTO>> GetByProjectKeyAsync(string projectKey);
        Task<RiskResponseDTO> GetByIdAsync(int id);
        Task<RiskResponseDTO> GetByKeyAsync(string key);
        Task<RiskResponseDTO> CreateRiskAsync(RiskCreateRequestDTO request);
        Task DeleteAsync(int id);
        Task<List<RiskResponseDTO>> GetUnapprovedAIRisksAsync(int projectId);
        Task ApproveRiskAsync(RiskRequestDTO dto, RiskSolutionRequestDTO solutionDto);
        Task<List<RiskRequestDTO>> DetectAndSaveProjectRisksAsync(int projectId);
        Task<List<RiskRequestDTO>> DetectProjectRisksAsync(int projectId);
        Task<List<RiskRequestDTO>> SaveProjectRisksAsync(List<RiskRequestDTO> risks);
        Task<RiskResponseDTO?> UpdateStatusAsync(int id, string status, int createdBy);
        Task<RiskResponseDTO?> UpdateTypeAsync(int id, string type, int createdBy);
        Task<RiskResponseDTO?> UpdateResponsibleIdAsync(int id, int? responsibleId, int createdBy);
        Task<RiskResponseDTO?> UpdateDueDateAsync(int id, DateTime dueDate, int createdBy);
        Task<RiskResponseDTO?> UpdateTitleAsync(int id, string title, int createdBy);
        Task<RiskResponseDTO?> UpdateDescriptionAsync(int id, string description, int createdBy);
        Task<RiskResponseDTO?> UpdateImpactLevelAsync(int id, string impactLevel, int createdBy);
        Task<RiskResponseDTO?> UpdateProbabilityAsync(int id, string probability, int createdBy);
        Task<List<AIRiskResponseDTO>> ViewAIProjectRisksAsync(string projectKey);
        Task<List<AIRiskResponseDTO>> ViewAIDetectTaskRisksAsyncAsync(string projectKey);
        Task CheckAndCreateOverdueTaskRisksAsync(string projectKey);
    }
}
