using IntelliPM.Data.DTOs.ProjectPosition.Request;
using IntelliPM.Data.DTOs.ProjectPosition.Response;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ProjectPositionServices
{
    public interface IProjectPositionService
    {
        Task<List<ProjectPositionResponseDTO>> GetAllProjectPositions(int projectMemberId);
        Task<ProjectPositionResponseDTO> GetProjectPositionById(int id);
        Task<ProjectPositionResponseDTO> CreateProjectPosition(int projectMemberId, ProjectPositionNoMemberIdRequestDTO request);
        Task<ProjectPositionResponseDTO> UpdateProjectPosition(int id, int projectMemberId, ProjectPositionNoMemberIdRequestDTO request);
        Task<ProjectPositionResponseDTO> AddProjectPosition(ProjectPositionRequestDTO request);
        Task DeleteProjectPosition(int id);
        Task<List<ProjectPosition>> GetAllByProjectId(int projectId);
        
    }
}
