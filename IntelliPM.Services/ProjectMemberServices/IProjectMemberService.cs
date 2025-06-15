using IntelliPM.Data.DTOs.ProjectMember.Request;
using IntelliPM.Data.DTOs.ProjectMember.Response;
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
        Task DeleteProjectMember(int id);

        Task<List<ProjectByAccountResponseDTO>> GetProjectsByAccountId(int accountId);
        Task<List<AccountByProjectResponseDTO>> GetAccountsByProjectId(int projectId);
    }
}
