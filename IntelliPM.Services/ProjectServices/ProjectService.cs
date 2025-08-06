using AutoMapper;
using Google.Cloud.Storage.V1;
using IntelliPM.Data.DTOs.Epic.Response;
using IntelliPM.Data.DTOs.Milestone.Response;
using IntelliPM.Data.DTOs.Project.Request;
using IntelliPM.Data.DTOs.Project.Response;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.DTOs.ProjectPosition.Response;
using IntelliPM.Data.DTOs.Requirement.Response;
using IntelliPM.Data.DTOs.Sprint.Response;
using IntelliPM.Data.DTOs.Subtask.Response;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.DTOs.TaskDependency.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.MilestoneRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SprintRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskDependencyRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.EmailServices;
using IntelliPM.Services.EpicServices;
using IntelliPM.Services.External.TaskApi;
using IntelliPM.Services.Helper.CustomExceptions;
using IntelliPM.Services.Helper.DecodeTokenHandler;
using IntelliPM.Services.ProjectMemberServices;
using IntelliPM.Services.SubtaskServices;
using IntelliPM.Services.TaskServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

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
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly ILogger<ProjectService> _logger;
        private readonly IEpicService _epicService;
        private readonly ITaskService _taskService;
        private readonly ISubtaskService _subtaskService;
        private readonly ISprintRepository _sprintRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly IMilestoneRepository _milestoneRepo;
        private readonly ITaskDependencyRepository _taskDependencyRepo;
        private readonly ISubtaskRepository _subtaskRepo;
        private readonly IConfiguration _config;
       // private readonly string _backendUrl;
        private readonly string _frontendUrl;
        private readonly IServiceProvider _serviceProvider;

        public ProjectService(IMapper mapper, IProjectRepository projectRepo, IDecodeTokenHandler decodeToken, IAccountRepository accountRepo, IEmailService emailService, IProjectMemberService projectMemberService, IProjectMemberRepository projectMemberRepo, ILogger<ProjectService> logger, IEpicService epicService, ITaskService taskService, ISubtaskService subtaskService, ISprintRepository sprintRepo, ITaskRepository taskRepo, IMilestoneRepository milestoneRepo, ITaskDependencyRepository taskDependencyRepo, ISubtaskRepository subtaskRepo, IConfiguration config, IServiceProvider serviceProvider)
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
            _sprintRepo = sprintRepo;
            _taskRepo = taskRepo;
            _milestoneRepo = milestoneRepo;
            _taskDependencyRepo = taskDependencyRepo;
            _subtaskRepo = subtaskRepo;
            _projectMemberRepo = projectMemberRepo;
            _config = config;
                #pragma warning disable CS8601
           // _backendUrl = config["Environment:BE_URL"];
            _frontendUrl = config["Environment:FE_URL"];
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
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
                    Email = pm.Account?.Email,
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
            if (pm.Status.Equals("ACTIVE"))
                throw new InvalidOperationException("The Project Manager has already reviewed this project. Email will not be sent again.");
   

            var projectInfo = await GetProjectById(projectId);
            if (projectInfo == null)
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");
            if (!projectInfo.Status.Equals("PLANNING"))
                throw new InvalidOperationException("This project is no longer in the planning phase. Notification is unnecessary.");

            await _projectMemberService.ChangeProjectMemberStatus(pm.Id, "INVITED");

            var projectDetailsUrl = $"{_frontendUrl}/project/{projectInfo.ProjectKey}/overviewpm";

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


        public async Task<string> SendEmailToLeaderReject(int projectId, string token, string reason)
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

            var pm = membersWithPositions.FirstOrDefault(m => m.ProjectPositions != null && m.ProjectPositions.Any(p => p.Position == "TEAM_LEADER") && (m.Status == "ACTIVE"));

            if (pm == null || string.IsNullOrEmpty(pm.FullName) || string.IsNullOrEmpty(pm.Email))
                throw new ArgumentException("No Project Manager found or email is missing.");


            var projectInfo = await GetProjectById(projectId);
            if (projectInfo == null)
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");
            if (!projectInfo.Status.Equals("PLANNING"))
                throw new InvalidOperationException("This project is no longer in the planning phase. Notification is unnecessary.");

            await ChangeProjectStatus(projectId, "CANCELLED");

            var projectDetailsUrl = $"{_frontendUrl}/project/{projectInfo.ProjectKey}/overviewpm";

            await _emailService.SendProjectReject(
                  pm.FullName,
                  pm.Email,
                  currentAccount.FullName,
                  currentAccount.Username,
                  projectInfo.Name,
                  projectInfo.ProjectKey,
                  projectId,
                  projectDetailsUrl,
                  reason
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

            var projectInfo = await _projectRepo.GetByIdAsync(projectId);
            if (projectInfo == null)
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");

            await ChangeProjectStatus(projectId, "IN_PROGRESS");

            var pm = membersWithPositions.FirstOrDefault(m => m.ProjectPositions != null && m.ProjectPositions.Any(p => p.Position == "PROJECT_MANAGER"));
            if (pm != null)
                await _projectMemberService.ChangeProjectMemberStatus(pm.Id, "ACTIVE");

            var eligibleMembers = membersWithPositions
                .Where(m => m.ProjectPositions != null && !m.ProjectPositions.Any(p => p.Position == "PROJECT_MANAGER"))
                .Where(m => m.Status == "CREATED")
                .ToList();

            if (!eligibleMembers.Any())
                return "No eligible team members to send invitations.";

            foreach (var member in eligibleMembers)
            {
                await _projectMemberService.ChangeProjectMemberStatus(member.Id, "INVITED");
            }

            // Giới hạn gửi 5 email một lúc
            var semaphore = new SemaphoreSlim(5);
            var emailTasks = eligibleMembers.Select(async member =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var projectInvitationUrl = $"{_frontendUrl}/project/invitation?projectKey={projectInfo.ProjectKey}&memberId={member.Id}";

                    await _emailService.SendTeamInvitation(
                        member.FullName,
                        member.Email,
                        currentAccount.FullName,
                        currentAccount.Username,
                        projectInfo.Name,
                        projectInfo.ProjectKey,
                        projectId,
                        projectInvitationUrl
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send email to {member.Email}");
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(emailTasks);

            return $"Invitations sent successfully to {eligibleMembers.Count} team members.";
        }




        public async Task<ProjectResponseDTO> ChangeProjectStatus(int id, string status)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("Status cannot be null or empty.");

            var entity = await _projectRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project with ID {id} not found.");

            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _projectRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change project status: {ex.Message}", ex);
            }

            return _mapper.Map<ProjectResponseDTO>(entity);
        }



        public async Task<List<WorkItemResponseDTO>> GetAllWorkItemsByProjectId(int projectId)
        {
            var workItems = new List<WorkItemResponseDTO>();

            using var epicScope = _serviceProvider.CreateScope();
            using var taskScope = _serviceProvider.CreateScope();
            using var subtaskScope = _serviceProvider.CreateScope();

            var epicService = epicScope.ServiceProvider.GetRequiredService<IEpicService>();
            var taskService = taskScope.ServiceProvider.GetRequiredService<ITaskService>();
            var subtaskService = subtaskScope.ServiceProvider.GetRequiredService<ISubtaskService>();

            var epicTask = epicService.GetEpicsByProjectId(projectId);
            var taskTask = taskService.GetTasksByProjectIdDetailed(projectId);
            var subtaskTask = subtaskService.GetSubtasksByProjectIdDetailed(projectId);

            await Task.WhenAll(epicTask, taskTask, subtaskTask);

            foreach (var epic in await epicTask)
            {
                workItems.Add(new WorkItemResponseDTO
                {
                    ProjectId = projectId,
                    Type = epic.Type,
                    Key = epic.Id,
                    Summary = epic.Name,
                    Status = epic.Status,
                    CommentCount = epic.CommentCount,
                    SprintId = epic.SprintId,
                    SprintName = epic.SprintName,
                    Assignees = new List<AssigneeDTO>
            {
                new AssigneeDTO
                {
                    AccountId = epic.AssignedBy ?? 0,
                    Fullname = epic.AssignedByFullname ?? "Unknown",
                    Picture = epic.AssignedByPicture
                }
            },
                    DueDate = epic.EndDate,
                    Labels = epic.Labels.Select(l => l.Name).ToList(),
                    CreatedAt = epic.CreatedAt,
                    UpdatedAt = epic.UpdatedAt,
                    ReporterId = epic.ReporterId,
                    ReporterFullname = epic.ReporterFullname ?? "Unknown",
                    ReporterPicture = epic.ReporterPicture
                });
            }

            foreach (var task in await taskTask)
            {
                workItems.Add(new WorkItemResponseDTO
                {
                    ProjectId = projectId,
                    Type = task.Type,
                    Key = task.Id,
                    Summary = task.Title,
                    Status = task.Status,
                    CommentCount = task.CommentCount,
                    SprintId = task.SprintId,
                    SprintName = task.SprintName,
                    Assignees = task.TaskAssignments.Select(a => new AssigneeDTO
                    {
                        AccountId = a.AccountId,
                        Fullname = a.AccountFullname ?? "Unknown",
                        Picture = a.AccountPicture
                    }).ToList(),
                    DueDate = task.PlannedEndDate,
                    Labels = task.Labels.Select(l => l.Name).ToList(),
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt,
                    ReporterId = task.ReporterId,
                    ReporterFullname = task.ReporterFullname ?? "Unknown",
                    ReporterPicture = task.ReporterPicture
                });
            }

            foreach (var subtask in await subtaskTask)
            {
                workItems.Add(new WorkItemResponseDTO
                {
                    ProjectId = projectId,
                    Type = subtask.Type,
                    Key = subtask.Id,
                    TaskId = subtask.TaskId,
                    Summary = subtask.Title,
                    Status = subtask.Status,
                    CommentCount = subtask.CommentCount,
                    SprintId = subtask.SprintId,
                    SprintName = subtask.SprintName,
                    Assignees = new List<AssigneeDTO>
            {
                new AssigneeDTO
                {
                    AccountId = subtask.AssignedBy ?? 0,
                    Fullname = subtask.AssignedByFullname ?? "Unknown",
                    Picture = subtask.AssignedByPicture
                }
            },
                    DueDate = subtask.EndDate,
                    Labels = subtask.Labels.Select(l => l.Name).ToList(),
                    CreatedAt = subtask.CreatedAt,
                    UpdatedAt = subtask.UpdatedAt,
                    ReporterId = subtask.ReporterId,
                    ReporterFullname = subtask.ReporterFullname ?? "Unknown",
                    ReporterPicture = subtask.ReporterPicture
                });
            }

            return workItems.OrderBy(w => w.CreatedAt).ToList();
        }




        //public async Task<List<WorkItemResponseDTO>> GetAllWorkItemsByProjectId(int projectId)
        //    {
        //        const int estimatedCapacity = 1000; // Điều chỉnh dựa trên quy mô dự án
        //        var workItems = new List<WorkItemResponseDTO>(estimatedCapacity);

        //        // Tạo scope riêng cho từng dịch vụ
        //        using var epicScope = _serviceProvider.CreateScope();
        //        using var taskScope = _serviceProvider.CreateScope();
        //        using var subtaskScope = _serviceProvider.CreateScope();

        //        var epicService = epicScope.ServiceProvider.GetRequiredService<IEpicService>();
        //        var taskService = taskScope.ServiceProvider.GetRequiredService<ITaskService>();
        //        var subtaskService = subtaskScope.ServiceProvider.GetRequiredService<ISubtaskService>();

        //        // Chạy song song các tác vụ lấy dữ liệu
        //        var epicTask = epicService.GetEpicsByProjectId(projectId);
        //        var taskTask = taskService.GetTasksByProjectIdDetailed(projectId);
        //        var subtaskTask = subtaskService.GetSubtasksByProjectIdDetailed(projectId);

        //        await Task.WhenAll(epicTask, taskTask, subtaskTask);

        //        // Lấy kết quả và sắp xếp ngay tại DB (nếu có thể)
        //        var epics = (await epicTask).ToList(); // OrderBy tại DB nếu có thể
        //        var tasks = (await taskTask).ToList();
        //        var subtasks = (await subtaskTask).ToList();

        //        // Song song hóa ánh xạ nếu dữ liệu lớn, tuần tự nếu nhỏ
        //        bool useParallel = epics.Count + tasks.Count + subtasks.Count > 100; // Ngưỡng tùy chỉnh
        //        if (useParallel)
        //        {
        //            Parallel.ForEach(
        //                new[]
        //                {
        //                (type: "epic", items: epics as IEnumerable<object>),
        //                (type: "task", items: tasks as IEnumerable<object>),
        //                (type: "subtask", items: subtasks as IEnumerable<object>)
        //                },
        //                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
        //                tuple =>
        //                {
        //                    var localItems = new List<WorkItemResponseDTO>();
        //                    switch (tuple.type)
        //                    {
        //                        case "epic":
        //                            foreach (var epic in tuple.items.Cast<EpicDetailedResponseDTO>())
        //                            {
        //                                localItems.Add(new WorkItemResponseDTO
        //                                {
        //                                    ProjectId = projectId,
        //                                    Type = epic.Type,
        //                                    Key = epic.Id,
        //                                    Summary = epic.Name,
        //                                    Status = epic.Status,
        //                                    CommentCount = epic.CommentCount,
        //                                    SprintId = epic.SprintId,
        //                                    SprintName = epic.SprintName,
        //                                    Assignees = new List<AssigneeDTO>
        //                                    {
        //                                    new AssigneeDTO
        //                                    {
        //                                        AccountId = epic.AssignedBy ?? 0,
        //                                        Fullname = epic.AssignedByFullname ?? "Unknown",
        //                                        Picture = epic.AssignedByPicture
        //                                    }
        //                                    },
        //                                    DueDate = epic.EndDate,
        //                                    Labels = epic.Labels?.Select(l => l.Name).ToList() ?? new List<string>(),
        //                                    CreatedAt = epic.CreatedAt,
        //                                    UpdatedAt = epic.UpdatedAt,
        //                                    ReporterId = epic.ReporterId,
        //                                    ReporterFullname = epic.ReporterFullname ?? "Unknown",
        //                                    ReporterPicture = epic.ReporterPicture
        //                                });
        //                            }
        //                            break;

        //                        case "task":
        //                            foreach (var task in tuple.items.Cast<TaskDetailedResponseDTO>())
        //                            {
        //                                localItems.Add(new WorkItemResponseDTO
        //                                {
        //                                    ProjectId = projectId,
        //                                    Type = task.Type,
        //                                    Key = task.Id,
        //                                    Summary = task.Title,
        //                                    Status = task.Status,
        //                                    CommentCount = task.CommentCount,
        //                                    SprintId = task.SprintId,
        //                                    SprintName = task.SprintName,
        //                                    Assignees = task.TaskAssignments?.Select(a => new AssigneeDTO
        //                                    {
        //                                        AccountId = a.AccountId,
        //                                        Fullname = a.AccountFullname ?? "Unknown",
        //                                        Picture = a.AccountPicture
        //                                    }).ToList() ?? new List<AssigneeDTO>(),
        //                                    DueDate = task.PlannedEndDate,
        //                                    Labels = task.Labels?.Select(l => l.Name).ToList() ?? new List<string>(),
        //                                    CreatedAt = task.CreatedAt,
        //                                    UpdatedAt = task.UpdatedAt,
        //                                    ReporterId = task.ReporterId,
        //                                    ReporterFullname = task.ReporterFullname ?? "Unknown",
        //                                    ReporterPicture = task.ReporterPicture
        //                                });
        //                            }
        //                            break;

        //                        case "subtask":
        //                            foreach (var subtask in tuple.items.Cast<SubtaskDetailedResponseDTO>()) // Sửa thành SubtaskDetailedResponseDTO
        //                            {
        //                                localItems.Add(new WorkItemResponseDTO
        //                                {
        //                                    ProjectId = projectId,
        //                                    Type = subtask.Type,
        //                                    Key = subtask.Id,
        //                                    TaskId = subtask.TaskId,
        //                                    Summary = subtask.Title,
        //                                    Status = subtask.Status,
        //                                    CommentCount = subtask.CommentCount,
        //                                    SprintId = subtask.SprintId,
        //                                    SprintName = subtask.SprintName,
        //                                    Assignees = new List<AssigneeDTO>
        //                                    {
        //                                    new AssigneeDTO
        //                                    {
        //                                        AccountId = subtask.AssignedBy ?? 0,
        //                                        Fullname = subtask.AssignedByFullname ?? "Unknown",
        //                                        Picture = subtask.AssignedByPicture
        //                                    }
        //                                    },
        //                                    DueDate = subtask.EndDate,
        //                                    Labels = subtask.Labels?.Select(l => l.Name).ToList() ?? new List<string>(),
        //                                    CreatedAt = subtask.CreatedAt,
        //                                    UpdatedAt = subtask.UpdatedAt,
        //                                    ReporterId = subtask.ReporterId,
        //                                    ReporterFullname = subtask.ReporterFullname ?? "Unknown",
        //                                    ReporterPicture = subtask.ReporterPicture
        //                                });
        //                            }
        //                            break;
        //                    }
        //                    lock (workItems)
        //                    {
        //                        workItems.AddRange(localItems);
        //                    }
        //                });
        //        }
        //        else
        //        {
        //            // Tuần tự cho dữ liệu nhỏ
        //            foreach (var epic in epics.Cast<EpicDetailedResponseDTO>())
        //            {
        //                workItems.Add(new WorkItemResponseDTO
        //                {
        //                    ProjectId = projectId,
        //                    Type = epic.Type,
        //                    Key = epic.Id,
        //                    Summary = epic.Name,
        //                    Status = epic.Status,
        //                    CommentCount = epic.CommentCount,
        //                    SprintId = epic.SprintId,
        //                    SprintName = epic.SprintName,
        //                    Assignees = new List<AssigneeDTO>
        //                {
        //                    new AssigneeDTO
        //                    {
        //                        AccountId = epic.AssignedBy ?? 0,
        //                        Fullname = epic.AssignedByFullname ?? "Unknown",
        //                        Picture = epic.AssignedByPicture
        //                    }
        //                },
        //                    DueDate = epic.EndDate,
        //                    Labels = epic.Labels?.Select(l => l.Name).ToList() ?? new List<string>(),
        //                    CreatedAt = epic.CreatedAt,
        //                    UpdatedAt = epic.UpdatedAt,
        //                    ReporterId = epic.ReporterId,
        //                    ReporterFullname = epic.ReporterFullname ?? "Unknown",
        //                    ReporterPicture = epic.ReporterPicture
        //                });
        //            }
        //            foreach (var task in tasks.Cast<TaskDetailedResponseDTO>())
        //            {
        //                workItems.Add(new WorkItemResponseDTO
        //                {
        //                    ProjectId = projectId,
        //                    Type = task.Type,
        //                    Key = task.Id,
        //                    Summary = task.Title,
        //                    Status = task.Status,
        //                    CommentCount = task.CommentCount,
        //                    SprintId = task.SprintId,
        //                    SprintName = task.SprintName,
        //                    Assignees = task.TaskAssignments?.Select(a => new AssigneeDTO
        //                    {
        //                        AccountId = a.AccountId,
        //                        Fullname = a.AccountFullname ?? "Unknown",
        //                        Picture = a.AccountPicture
        //                    }).ToList() ?? new List<AssigneeDTO>(),
        //                    DueDate = task.PlannedEndDate,
        //                    Labels = task.Labels?.Select(l => l.Name).ToList() ?? new List<string>(),
        //                    CreatedAt = task.CreatedAt,
        //                    UpdatedAt = task.UpdatedAt,
        //                    ReporterId = task.ReporterId,
        //                    ReporterFullname = task.ReporterFullname ?? "Unknown",
        //                    ReporterPicture = task.ReporterPicture
        //                });
        //            }
        //            foreach (var subtask in subtasks.Cast<SubtaskDetailedResponseDTO>())
        //            {
        //                workItems.Add(new WorkItemResponseDTO
        //                {
        //                    ProjectId = projectId,
        //                    Type = subtask.Type,
        //                    Key = subtask.Id,
        //                    TaskId = subtask.TaskId,
        //                    Summary = subtask.Title,
        //                    Status = subtask.Status,
        //                    CommentCount = subtask.CommentCount,
        //                    SprintId = subtask.SprintId,
        //                    SprintName = subtask.SprintName,
        //                    Assignees = new List<AssigneeDTO>
        //                {
        //                    new AssigneeDTO
        //                    {
        //                        AccountId = subtask.AssignedBy ?? 0,
        //                        Fullname = subtask.AssignedByFullname ?? "Unknown",
        //                        Picture = subtask.AssignedByPicture
        //                    }
        //                },
        //                    DueDate = subtask.EndDate,
        //                    Labels = subtask.Labels?.Select(l => l.Name).ToList() ?? new List<string>(),
        //                    CreatedAt = subtask.CreatedAt,
        //                    UpdatedAt = subtask.UpdatedAt,
        //                    ReporterId = subtask.ReporterId,
        //                    ReporterFullname = subtask.ReporterFullname ?? "Unknown",
        //                    ReporterPicture = subtask.ReporterPicture
        //                });
        //            }
        //        }

        //        return workItems.OrderBy(w => w.CreatedAt).ToList();
        //    }


        public async Task<ProjectResponseDTO> GetProjectByKey(string projectKey)
        {
            if (string.IsNullOrEmpty(projectKey))
                throw new ArgumentException("Project key cannot be null or empty.");

            var project = await _projectRepo.GetProjectByKeyAsync(projectKey);
            if (project == null)
                throw new KeyNotFoundException($"Project with key {projectKey} not found.");

            return _mapper.Map<ProjectResponseDTO>(project);
        }

        public async Task<bool> CheckProjectKeyExists(string projectKey)
        {
            if (string.IsNullOrEmpty(projectKey))
                throw new ArgumentException("Project key cannot be null or empty.");

            var project = await _projectRepo.GetProjectByKeyAsync(projectKey);
            return project != null;
        }

        public async Task<bool> CheckProjectNameExists(string projectName)
        {
            if (string.IsNullOrEmpty(projectName))
                throw new ArgumentException("Project name cannot be null or empty.");

            var project = await _projectRepo.GetProjectByNameAsync(projectName);
            return project != null;
        }

        //public async Task<ProjectViewDTO?> GetProjectViewByKeyAsync(string projectKey)
        //{
        //    var project = await _projectRepo.GetProjectByKeyAsync(projectKey);
        //    if (project == null) return null;

        //    var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
        //    var subtasks = await _subtaskRepo.GetByProjectIdAsync(project.Id);
        //    var milestones = await _milestoneRepo.GetMilestonesByProjectIdAsync(project.Id);
        //    var sprints = await _sprintRepo.GetByProjectIdAsync(project.Id);
        //    var dependencies = await _taskDependencyRepo.GetByProjectIdAsync(project.Id);

        //    var dependenciesGrouped = dependencies
        //        .GroupBy(d => d.TaskId)
        //        .ToDictionary(
        //            g => g.Key,
        //            g => g.Select(dep => new TaskDependencyResponseDTO
        //            {
        //                Id = dep.Id,
        //                TaskId = dep.TaskId,
        //                LinkedFrom = dep.LinkedFrom,
        //                LinkedTo = dep.LinkedTo,
        //                Type = dep.Type
        //            }).ToList()
        //        );

        //    var subtasksGrouped = subtasks
        //        .GroupBy(s => s.TaskId)
        //        .ToDictionary(g => g.Key, g => g.Select(s => new SubtaskFullResponseDTO
        //        {
        //            Id = s.Id,
        //            TaskId = s.TaskId,
        //            AssignedBy = (int)s.AssignedBy,
        //            Title = s.Title,
        //            Description = s.Description,
        //            ReporterId = s.ReporterId,
        //            Status = s.Status,
        //            Priority = s.Priority,
        //            ManualInput = s.ManualInput,
        //            GenerationAiInput = s.GenerationAiInput,
        //            SprintId = s.SprintId,
        //            PlannedStartDate = s.PlannedStartDate,
        //            PlannedEndDate = s.PlannedEndDate,
        //            ActualStartDate = s.ActualStartDate,
        //            Duration = s.Duration,
        //            ActualEndDate = s.ActualEndDate,
        //            PercentComplete = s.PercentComplete,
        //            PlannedHours = s.PlannedHours,
        //            ActualHours = s.ActualHours,
        //            RemainingHours = s.RemainingHours,
        //            PlannedCost = s.PlannedCost,
        //            PlannedResourceCost = s.PlannedResourceCost,
        //            ActualCost = s.ActualCost,
        //            ActualResourceCost = s.ActualResourceCost,
        //            Evaluate = s.Evaluate,
        //            CreatedAt = s.CreatedAt,
        //            UpdatedAt = s.UpdatedAt,
        //            StartDate = s.StartDate,
        //            EndDate = s.EndDate
        //        }).ToList());

        //    return new ProjectViewDTO
        //    {
        //        Id = project.Id,
        //        ProjectKey = project.ProjectKey,
        //        Name = project.Name,
        //        Description = project.Description,
        //        Budget = project.Budget,
        //        ProjectType = project.ProjectType,
        //        CreatedBy = project.CreatedBy,
        //        StartDate = project.StartDate,
        //        EndDate = project.EndDate,
        //        CreatedAt = project.CreatedAt,
        //        UpdatedAt = project.UpdatedAt,
        //        IconUrl = project.IconUrl,
        //        Status = project.Status,

        //        Sprints = sprints.Select(s => new SprintResponseDTO
        //        {
        //            Id = s.Id,
        //            Name = s.Name,
        //            Goal = s.Goal,
        //            StartDate = s.StartDate,
        //            EndDate = s.EndDate,
        //            Status = s.Status,
        //            CreatedAt = s.CreatedAt,
        //            UpdatedAt = s.UpdatedAt,
        //        }).ToList(),
        //        Tasks = tasks.Select(t => new TaskSubtaskDependencyResponseDTO
        //        {
        //            Id = t.Id,
        //            ReporterId = t.ReporterId,
        //            ProjectId = t.ProjectId,
        //            EpicId = t.EpicId,
        //            SprintId = t.SprintId,
        //            Type = t.Type,
        //            ManualInput = t.ManualInput,
        //            GenerationAiInput = t.GenerationAiInput,
        //            Title = t.Title,
        //            Description = t.Description,
        //            PlannedStartDate = t.PlannedStartDate,
        //            PlannedEndDate = t.PlannedEndDate,
        //            ActualStartDate = t.ActualStartDate,
        //            ActualEndDate = t.ActualEndDate,
        //            Duration = t.Duration,
        //            PercentComplete = t.PercentComplete,
        //            PlannedHours = t.PlannedHours,
        //            ActualHours = t.ActualHours,
        //            RemainingHours = t.RemainingHours,
        //            PlannedCost = t.PlannedCost,
        //            PlannedResourceCost = t.PlannedResourceCost,
        //            ActualCost = t.ActualCost,
        //            ActualResourceCost = t.ActualResourceCost,
        //            Evaluate = t.Evaluate,
        //            Status = t.Status,
        //            Priority = t.Priority,
        //            CreatedAt = t.CreatedAt,
        //            UpdatedAt = t.UpdatedAt,
        //            Dependencies = dependenciesGrouped.ContainsKey(t.Id) ? dependenciesGrouped[t.Id] : new List<TaskDependencyResponseDTO>(),
        //            Subtasks = subtasksGrouped.ContainsKey(t.Id) ? subtasksGrouped[t.Id] : new List<SubtaskFullResponseDTO>()
        //        }).ToList(),
        //        Milestones = milestones.Select(m => new MilestoneResponseDTO
        //        {
        //            Id = m.Id,
        //            ProjectId = m.ProjectId,
        //            SprintId = m.SprintId,
        //            Name = m.Name,
        //            Description = m.Description,
        //            StartDate = m.StartDate,
        //            EndDate = m.EndDate,
        //            CreatedAt = m.CreatedAt,
        //            UpdatedAt = m.UpdatedAt,
        //            Status = m.Status,
        //        }).ToList()
        //    };
        //}

        public async Task<ProjectViewDTO?> GetProjectViewByKeyAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey);
            if (project == null) return null;

            var tasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var subtasks = await _subtaskRepo.GetByProjectIdAsync(project.Id);
            var milestones = await _milestoneRepo.GetMilestonesByProjectIdAsync(project.Id);
            var sprints = await _sprintRepo.GetByProjectIdAsync(project.Id);
            var dependencies = await _taskDependencyRepo.GetByProjectIdAsync(project.Id);

            var dependencyDTOs = _mapper.Map<List<TaskDependencyResponseDTO>>(dependencies);

            // Group theo from_id để dễ inject
            var groupedDependencies = dependencyDTOs
                .GroupBy(d => (d.LinkedFrom, d.FromType))
                .ToDictionary(g => g.Key, g => g.ToList());


            var subtasksGrouped = _mapper.Map<List<SubtaskDependencyResponseDTO>>(subtasks)
                .GroupBy(s => s.TaskId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var taskDTOs = _mapper.Map<List<TaskSubtaskDependencyResponseDTO>>(tasks);
            foreach (var t in taskDTOs)
            {
                t.Subtasks = subtasksGrouped.ContainsKey(t.Id) ? subtasksGrouped[t.Id] : new();
                t.Dependencies = groupedDependencies.TryGetValue((t.Id, "task"), out var deps) ? deps : new();
            }

            foreach (var t in taskDTOs)
            {
                foreach (var sub in t.Subtasks)
                {
                    sub.Dependencies = groupedDependencies.TryGetValue((sub.Id, "subtask"), out var sdeps) ? sdeps : new();
                }
            }

            var milestoneDTOs = _mapper.Map<List<MilestoneDependencyResponseDTO>>(milestones);
            foreach (var m in milestoneDTOs)
            {
                m.Dependencies = groupedDependencies.TryGetValue((m.Key, "milestone"), out var mdeps) ? mdeps : new();
            }


            // Final result
            var projectDTO = _mapper.Map<ProjectViewDTO>(project);
            projectDTO.Sprints = _mapper.Map<List<SprintResponseDTO>>(sprints);
            projectDTO.Milestones = milestoneDTOs;
            projectDTO.Tasks = taskDTOs;

            return projectDTO;
        }

        public async Task<List<ProjectItemDTO>> GetProjectItemsAsync(string projectKey)
        {
            var project = await _projectRepo.GetProjectByKeyAsync(projectKey);
            return await _projectRepo.GetProjectItemsAsync(project.Id);
        }
    }
}
