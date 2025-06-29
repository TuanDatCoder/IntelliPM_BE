using IntelliPM.Data.DTOs.Project.Request;
using IntelliPM.Data.DTOs.Project.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ProjectServices
{
    public interface IProjectService
    {
        Task<List<ProjectResponseDTO>> GetAllProjects();
        Task<ProjectResponseDTO> GetProjectById(int id);
        Task<List<ProjectResponseDTO>> SearchProjects(string searchTerm, string? projectType, string? status);
        Task<ProjectResponseDTO> CreateProject(ProjectRequestDTO request);
        Task<ProjectResponseDTO> UpdateProject(int id, ProjectRequestDTO request);
        Task DeleteProject(int id);
        Task<ProjectDetailsDTO> GetProjectDetails(int id);
        Task<string> SendEmailToProjectManager(int projectId, string token);
        Task<string> SendInvitationsToTeamMembers(int projectId, string token);
        Task<List<WorkItemResponseDTO>> GetAllWorkItemsByProjectId(int projectId);
        Task<bool> CheckProjectKeyExists(string projectKey);
        Task<ProjectResponseDTO> GetProjectByKey(string projectKey);
    }
}
