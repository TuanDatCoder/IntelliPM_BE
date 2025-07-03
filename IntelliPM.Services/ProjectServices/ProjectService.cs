using AutoMapper;
using IntelliPM.Data.DTOs.Project.Request;
using IntelliPM.Data.DTOs.Project.Response;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.DTOs.ProjectPosition.Response;
using IntelliPM.Data.DTOs.Requirement.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Services.EmailServices;
using IntelliPM.Services.EpicServices;
using IntelliPM.Services.Helper.CustomExceptions;
using IntelliPM.Services.Helper.DecodeTokenHandler;
using IntelliPM.Services.ProjectMemberServices;
using IntelliPM.Services.SubtaskServices;
using IntelliPM.Services.TaskServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ProjectServices
{
    public class ProjectService : IProjectService
    {
        private readonly IMapper _mapper;
        private readonly IProjectRepository _projectRepo;
        private readonly IDecodeTokenHandler _decodeToken;
        private readonly IAccountRepository _accountRepo;
        private readonly IEmailService _emailService; 
        private readonly IProjectMemberService _projectMemberService;
        private readonly ILogger<ProjectService> _logger;
        private readonly IEpicService _epicService;
        private readonly ITaskService _taskService;
        private readonly ISubtaskService _subtaskService;

        public ProjectService(IMapper mapper, IProjectRepository projectRepo, IDecodeTokenHandler decodeToken, IAccountRepository accountRepo, IEmailService emailService, IProjectMemberService projectMemberService, ILogger<ProjectService> logger, IEpicService epicService, ITaskService taskService, ISubtaskService subtaskService)
        {
            _mapper = mapper;
            _projectRepo = projectRepo;
            _decodeToken = decodeToken;
            _accountRepo = accountRepo;
            _emailService = emailService;
            _projectMemberService = projectMemberService;
            _logger = logger;
            _epicService = epicService;
            _taskService = taskService;
            _subtaskService = subtaskService;
        }

        public async Task<List<ProjectResponseDTO>> GetAllProjects()
        {
            var entities = await _projectRepo.GetAllProjects();
            return _mapper.Map<List<ProjectResponseDTO>>(entities);
        }

        public async Task<ProjectResponseDTO> GetProjectById(int id)
        {
            var entity = await _projectRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project with ID {id} not found.");

            return _mapper.Map<ProjectResponseDTO>(entity);
        }

        public async Task<List<ProjectResponseDTO>> SearchProjects(string searchTerm, string? projectType, string? status)
        {
            if (string.IsNullOrEmpty(searchTerm) && string.IsNullOrEmpty(projectType) && string.IsNullOrEmpty(status))
                throw new ArgumentException("At least one search criterion must be provided.");

            var entities = await _projectRepo.SearchProjects(searchTerm, projectType, status);
            return _mapper.Map<List<ProjectResponseDTO>>(entities);
        }

        public async Task<ProjectResponseDTO> CreateProject(string token, ProjectRequestDTO request)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token is required.", nameof(token));

            var decode = _decodeToken.Decode(token);
            if (decode == null || string.IsNullOrEmpty(decode.username))
                throw new ApiException(HttpStatusCode.Unauthorized, "Invalid token data.");

            var currentAccount = await _accountRepo.GetAccountByUsername(decode.username);
            if (currentAccount == null)
                throw new ApiException(HttpStatusCode.NotFound, "User not found.");


            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Name))
                throw new ArgumentException("Project name is required.", nameof(request.Name));

            var entity = _mapper.Map<Project>(request);
            entity.CreatedBy = currentAccount.Id;
            entity.IconUrl = "https://res.cloudinary.com/dcv4x7oen/image/upload/v1751353454/project2_abr0nj.png";
            entity.Status = "PLANNING";

            try
            {
                await _projectRepo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create project due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create project: {ex.Message}", ex);
            }

            return _mapper.Map<ProjectResponseDTO>(entity);
        }

        public async Task<ProjectResponseDTO> UpdateProject(int id, ProjectRequestDTO request)
        {
            var entity = await _projectRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project with ID {id} not found.");

            _mapper.Map(request, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _projectRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update project: {ex.Message}", ex);
            }

            return _mapper.Map<ProjectResponseDTO>(entity);
        }

        public async Task DeleteProject(int id)
        {
            var entity = await _projectRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project with ID {id} not found.");

            try
            {
                await _projectRepo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete project: {ex.Message}", ex);
            }
        }





        public async Task<ProjectDetailsDTO> GetProjectDetails(int id)
        {
            var project = await _projectRepo.GetProjectWithMembersAndRequirements(id);

            if (project == null)
                throw new KeyNotFoundException($"Project with ID {id} not found.");

            var details = new ProjectDetailsDTO
            {
                Id = project.Id,
                ProjectKey = project.ProjectKey,
                Name = project.Name,
                Description = project.Description,
                Budget = project.Budget,
                ProjectType = project.ProjectType,
                CreatedBy = project.CreatedBy,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                Status = project.Status,

                Requirements = project.Requirement?.Select(r => new RequirementResponseDTO
                {
                    Id = r.Id,
                    ProjectId = r.ProjectId,
                    Title = r.Title,
                    Type = r.Type,
                    Description = r.Description,
                    Priority = r.Priority,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                }).ToList(),

                ProjectMembers = project.ProjectMember?.Select(pm => new ProjectMemberWithPositionsResponseDTO
                {
                    Id = pm.Id,
                    AccountId = pm.AccountId,
                    ProjectId = pm.ProjectId,
                    JoinedAt = pm.JoinedAt,
                    InvitedAt = pm.InvitedAt,
                    Status = pm.Status,
                    FullName = pm.Account?.FullName, 
                    Username = pm.Account?.Username, 
                    Picture = pm.Account?.Picture,  
                    ProjectPositions = pm.ProjectPosition?.Select(pp => new ProjectPositionResponseDTO
                    {
                        Id = pp.Id,
                        ProjectMemberId = pp.ProjectMemberId,
                        Position = pp.Position
                    }).ToList()
                }).ToList()
            };

            return details;
        }

        public async Task<string> SendEmailToProjectManager(int projectId, string token)
        {
            var decode = _decodeToken.Decode(token);
            if (decode == null || string.IsNullOrEmpty(decode.username))
                throw new UnauthorizedAccessException("Invalid token data.");

            var currentAccount = await _accountRepo.GetAccountByUsername(decode.username);
            if (currentAccount == null)
                throw new KeyNotFoundException("User not found.");

            var membersWithPositions = await _projectMemberService.GetProjectMemberWithPositionsByProjectId(projectId);
            if (membersWithPositions == null || !membersWithPositions.Any())
                throw new KeyNotFoundException($"No project members found for Project ID {projectId}.");

            // Tìm Project Manager trong danh sách
            var pm = membersWithPositions.FirstOrDefault(m => m.ProjectPositions != null && m.ProjectPositions.Any(p => p.Position == "PROJECT_MANAGER"));
            if (pm == null || string.IsNullOrEmpty(pm.FullName) || string.IsNullOrEmpty(pm.Email))
                throw new ArgumentException("No Project Manager found or email is missing.");
            if(!pm.Status.Equals("CREATED"))
                throw new InvalidOperationException("The Project Manager has already reviewed this project. Email will not be sent again.");

            var projectInfo = await GetProjectById(projectId);
            if (projectInfo == null)
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");
            if (!projectInfo.Status.Equals("PLANNING"))
                throw new InvalidOperationException("This project is no longer in the planning phase. Notification is unnecessary.");

            var projectDetailsUrl = $"https://localhost:7128/api/project/{projectId}/details";

            await _emailService.SendProjectCreationNotification(
                  pm.FullName,
                  pm.Email,
                  currentAccount.FullName,
                  currentAccount.Username,
                  projectInfo.Name, 
                  projectInfo.ProjectKey,  
                  projectId,
                  projectDetailsUrl
              );

            return "Email sent successfully to Project Manager.";
        }


        public async Task<string> SendInvitationsToTeamMembers(int projectId, string token)
        {
            var decode = _decodeToken.Decode(token);
            if (decode == null || string.IsNullOrEmpty(decode.username))
                throw new UnauthorizedAccessException("Invalid token data.");

            var currentAccount = await _accountRepo.GetAccountByUsername(decode.username);
            if (currentAccount == null)
                throw new KeyNotFoundException("User not found.");


            var membersWithPositions = await _projectMemberService.GetProjectMemberWithPositionsByProjectId(projectId);
            if (membersWithPositions == null || !membersWithPositions.Any())
                throw new KeyNotFoundException($"No project members found for Project ID {projectId}.");


            var projectInfo = await GetProjectById(projectId);
            if (projectInfo == null)
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");

         
            var eligibleMembers = membersWithPositions
                .Where(m => m.ProjectPositions != null && !m.ProjectPositions.Any(p => p.Position == "PROJECT_MANAGER"))
                .Where(m => m.Status == "CREATED")
                .ToList();

            if (!eligibleMembers.Any())
                return "No eligible team members to send invitations.";

            
            var projectDetailsUrl = $"https://localhost:7128/api/project/{projectId}/details";

            foreach (var member in eligibleMembers)
            {
                await _emailService.SendTeamInvitation(
                    member.FullName,
                    member.Email,
                    currentAccount.FullName,
                    currentAccount.Username,
                    projectInfo.Name,
                    projectInfo.ProjectKey,
                    projectId,
                    projectDetailsUrl
                );
            }

            return $"Invitations sent successfully to {eligibleMembers.Count} team members.";
        }

        public async Task<List<WorkItemResponseDTO>> GetAllWorkItemsByProjectId(int projectId)
        {
            var workItems = new List<WorkItemResponseDTO>();

            var epics = await _epicService.GetEpicsByProjectId(projectId);
            foreach (var epic in epics)
            {
                var item = new WorkItemResponseDTO
                {
                    Type = epic.Type,
                    Key = epic.Id,
                    Summary = epic.Name,
                    Status = epic.Status,
                    CommentCount = epic.CommentCount,
                    SprintId = epic.SprintId,
                    Assignees = new List<AssigneeDTO>
            {
                new AssigneeDTO
                {
                    Fullname = epic.AssignedByFullname ?? "Unknown",
                    Picture = null 
                }
            },
                    DueDate = epic.EndDate,
                    Labels = epic.Labels.Select(l => l.Name).ToList(),
                    CreatedAt = epic.CreatedAt,
                    UpdatedAt = epic.UpdatedAt,
                    ReporterFullname = epic.ReporterFullname ?? "Unknown",
                    ReporterPicture = epic.ReporterPicture
                };
                workItems.Add(item);
            }

            var tasks = await _taskService.GetTasksByProjectIdDetailed(projectId);
            foreach (var task in tasks)
            {
                var item = new WorkItemResponseDTO
                {
                    Type = task.Type,
                    Key = task.Id,
                    Summary = task.Title,
                    Status = task.Status,
                    CommentCount = task.CommentCount,
                    SprintId = task.SprintId,
                    Assignees = task.TaskAssignments.Select(a => new AssigneeDTO
                    {
                        Fullname = a.AccountFullname ?? "Unknown",
                        Picture = a.AccountPicture 
                    }).ToList(),
                    DueDate = task.PlannedEndDate,
                    Labels = task.Labels.Select(l => l.Name).ToList(),
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt,
                    ReporterFullname = task.ReporterFullname ?? "Unknown",
                    ReporterPicture = task.ReporterPicture
                };
                workItems.Add(item);
            }

            var subtasks = await _subtaskService.GetSubtasksByProjectIdDetailed(projectId);
            foreach (var subtask in subtasks)
            {
                var item = new WorkItemResponseDTO
                {
                    Type = subtask.Type,
                    Key = subtask.Id,
                    TaskId = subtask.TaskId,
                    Summary = subtask.Title,
                    Status = subtask.Status,
                    CommentCount = subtask.CommentCount,
                    SprintId = subtask.SprintId,
                    Assignees = new List<AssigneeDTO>
            {
                new AssigneeDTO
                {
                    Fullname = subtask.AssignedByFullname ?? "Unknown",
                    Picture = subtask.AssignedByPicture 
                }
            },
                    DueDate = subtask.EndDate,
                    Labels = subtask.Labels.Select(l => l.Name).ToList(),
                    CreatedAt = subtask.CreatedAt,
                    UpdatedAt = subtask.UpdatedAt,
                    ReporterFullname = subtask.ReporterFullname ?? "Unknown",
                    ReporterPicture = subtask.ReporterPicture
                };
                workItems.Add(item);
            }

            return workItems.OrderBy(w => w.CreatedAt).ToList();
        }

        public async Task<bool> CheckProjectKeyExists(string projectKey)
        {
            if (string.IsNullOrEmpty(projectKey))
                throw new ArgumentException("Project key cannot be null or empty.");

            var project = await _projectRepo.GetProjectByKeyAsync(projectKey);
            return project != null;
        }
        public async Task<ProjectResponseDTO> GetProjectByKey(string projectKey)
        {
            if (string.IsNullOrEmpty(projectKey))
                throw new ArgumentException("Project key cannot be null or empty.");

            var project = await _projectRepo.GetProjectByKeyAsync(projectKey);
            if (project == null)
                throw new KeyNotFoundException($"Project with key {projectKey} not found.");

            return _mapper.Map<ProjectResponseDTO>(project);
        }
    }
}
