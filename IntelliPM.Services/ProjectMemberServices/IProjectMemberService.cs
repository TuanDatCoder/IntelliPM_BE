using IntelliPM.Data.DTOs.ProjectMember.Request;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ProjectMemberServices
{
    public interface IProjectMemberService
    {
        Task<List<ProjectMemberResponseDTO>> GetAllProjectMembers(int projectId);
        Task<ProjectMemberResponseDTO> GetProjectMemberById(int id);
        Task<ProjectMemberResponseDTO> AddProjectMember(ProjectMemberRequestDTO request);
        Task DeleteProjectMember(int id);
    }
}
