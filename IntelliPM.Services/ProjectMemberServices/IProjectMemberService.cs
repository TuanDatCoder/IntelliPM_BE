using IntelliPM.Data.DTOs.Project.Response;
using IntelliPM.Data.DTOs.ProjectMember.Request;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ProjectMemberServices
{
    public interface IProjectMemberService
    {

        Task<List<ProjectMemberResponseDTO>> GetAllAsync();
        Task<List<ProjectMemberResponseDTO>> GetAllProjectMembers(int projectId);
        Task<ProjectMemberResponseDTO> GetProjectMemberById(int id);
        Task<ProjectMemberResponseDTO> AddProjectMember(ProjectMemberRequestDTO request);
        Task<ProjectMemberResponseDTO> CreateProjectMember(int ProjectId,ProjectMemberNoProjectIdRequestDTO request);
        Task DeleteProjectMember(int id);
        Task<List<ProjectByAccountResponseDTO>> GetProjectsByAccountId(int accountId);
        Task<List<ProjectByAccountResponseDTO>> GetProjectsByAccount(string token);
        Task<List<AccountByProjectResponseDTO>> GetAccountsByProjectId(int projectId);
        Task<List<ProjectMember>> GetAllByProjectId(int projectId);
        Task<List<ProjectMemberResponseDTO>> GetProjectMemberByProjectId(int projectId);
        Task<List<ProjectMemberWithPositionsResponseDTO>> CreateBulkWithPositions(int projectId, string token, List<ProjectMemberWithPositionRequestDTO> requests);
        Task<List<ProjectMemberWithPositionsResponseDTO>> GetProjectMemberWithPositionsByProjectId(int projectId);

        Task<ProjectMemberResponseDTO> ChangeProjectMemberStatus(int id, string status);
    }
}
