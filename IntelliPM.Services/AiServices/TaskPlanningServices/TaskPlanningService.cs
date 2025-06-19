using AutoMapper;
using IntelliPM.Data.DTOs.Project.Response;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.DTOs.ProjectPosition.Response;
using IntelliPM.Data.DTOs.Requirement.Request;
using IntelliPM.Data.DTOs.Requirement.Response;
using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.DTOs.TaskAssignment.Request;
using IntelliPM.Data.Entities;
using IntelliPM.Services.EpicServices;
using IntelliPM.Services.ProjectServices;
using IntelliPM.Services.SprintServices;
using IntelliPM.Services.TaskAssignmentServices;
using IntelliPM.Services.TaskServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IntelliPM.Services.AiServices.TaskPlanningServices
{
    public class TaskPlanningService : ITaskPlanningService
    {
        private readonly IProjectService _projectService;
        private readonly ITaskAssignmentService _taskAssignmentService;
        private readonly ITaskService _taskService;
        private readonly IEpicService _epicService;
        private readonly ISprintService _sprintService;
        private readonly IMapper _mapper;
        private readonly ILogger<TaskPlanningService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public TaskPlanningService(IProjectService projectService, ITaskAssignmentService taskAssignmentService,
            ITaskService taskService, IEpicService epicService, ISprintService sprintService,
            IMapper mapper, ILogger<TaskPlanningService> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _taskAssignmentService = taskAssignmentService ?? throw new ArgumentNullException(nameof(taskAssignmentService));
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _epicService = epicService ?? throw new ArgumentNullException(nameof(epicService));
            _sprintService = sprintService ?? throw new ArgumentNullException(nameof(sprintService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/");
            _apiKey = configuration["GEMINI_API_KEY"] ?? throw new InvalidOperationException("GEMINI_API_KEY is not set in configuration.");
        }

        public async Task<List<object>> GenerateTaskPlan(int projectId)
        {
            if (projectId <= 0)
                throw new ArgumentException("Invalid request. ProjectId must be greater than 0.");

            var result = new List<object>();

            var projectDetails = await _projectService.GetProjectDetails(projectId);
            if (projectDetails == null)
            {
                _logger.LogWarning("No project details found for ProjectId {ProjectId}. Using fallback data.", projectId);
                projectDetails = GenerateFallbackProjectDetailsResponse(projectId);
            }

            var projectKey = projectDetails.ProjectKey ?? $"PROJ-{projectId}";
            var startDate = projectDetails.StartDate ?? DateTime.UtcNow;
            var endDate = projectDetails.EndDate ?? startDate.AddMonths(6);

            var requirements = projectDetails.Requirements?.Select(r => _mapper.Map<RequirementRequestDTO>(r)).ToList() ?? new List<RequirementRequestDTO>();
            _logger.LogInformation("Mapped requirements count: {Count}", requirements?.Count ?? 0);

            if (!requirements.Any())
            {
                _logger.LogWarning("No requirements found for ProjectId {ProjectId}. Using fallback data.", projectId);
                requirements = GenerateFallbackRequirementsResponse(projectId);
            }

            var projectMembers = projectDetails.ProjectMembers?.Select(pm => new ProjectMember
            {
                Id = pm.Id,
                AccountId = pm.AccountId,
                ProjectId = pm.ProjectId,
                JoinedAt = pm.JoinedAt,
                InvitedAt = pm.InvitedAt,
                Status = pm.Status,
                FullName = pm.FullName,
                Picture = pm.Picture,
                ProjectPosition = pm.ProjectPositions?.Select(pp => new ProjectPosition
                {
                    Id = pp.Id,
                    ProjectMemberId = pp.ProjectMemberId,
                    Position = pp.Position
                }).ToList() ?? new List<ProjectPosition>()
            }).ToList() ?? new List<ProjectMember>();
            _logger.LogInformation("Mapped project members count: {Count}", projectMembers?.Count ?? 0);

            if (!projectMembers.Any())
            {
                _logger.LogWarning("No project members found for ProjectId {ProjectId}. Using fallback data.", projectId);
                projectMembers = GenerateFallbackMembersResponse(projectId);
            }

            var positions = projectMembers.SelectMany(pm => pm.ProjectPosition).ToList();
            if (!positions.Any())
            {
                _logger.LogWarning("No positions found for ProjectId {ProjectId}. Using fallback data.", projectId);
                positions = GenerateFallbackPositionsResponse(projectId);
            }

            var epics = CreateEpicsResponse(startDate, endDate, projectId, requirements, projectKey);

            var epicTasks = await GenerateEpicTasksFromAIResponse(startDate, endDate, projectId, requirements, projectMembers, positions, projectKey);

            foreach (var epic in epicTasks)
            {
                foreach (var task in epic.Tasks)
                {
                    if (!task.Members.Any())
                    {
                        task.Members = AssignMembersToTaskResponse(positions, task.SuggestedRole);
                    }
                    // Thêm fullName và picture cho từng member
                    foreach (var member in task.Members)
                    {
                        var projectMember = projectMembers.FirstOrDefault(pm => pm.AccountId == member.AccountId);
                        if (projectMember != null)
                        {
                            member.FullName = projectMember.FullName;
                            member.Picture = projectMember.Picture;
                        }
                    }
                }
            }

            var savedTasks = new List<Tasks>();
            foreach (var epic in epicTasks)
            {
                foreach (var task in epic.Tasks)
                {
                    var taskRequest = _mapper.Map<TaskRequestDTO>(task);
                    taskRequest.ProjectId = projectId;
                    taskRequest.ReporterId = 1;
                    var savedTaskResponse = await _taskService.CreateTask(taskRequest);
                    var savedTask = _mapper.Map<Tasks>(savedTaskResponse);
                    savedTasks.Add(savedTask);
                }
            }

            var assignments = AssignTasksToMembersResponse(savedTasks, projectMembers, positions);

            result.AddRange(epicTasks.Select((e, index) => new
            {
                Type = "Epic",
                Data = new
                {
                    EpicId = $"{projectKey}-{index + 1}",
                    ProjectId = projectId,
                    Title = e.Title,
                    Description = e.Description,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Tasks = e.Tasks.Select((t, taskIndex) => new
                    {
                        TaskId = $"{projectKey}-{index + 1}-{taskIndex + 1}",
                        Title = t.Title,
                        Description = t.Description,
                        Milestone = t.Milestone,
                        StartDate = t.StartDate,
                        EndDate = t.EndDate,
                        SuggestedRole = t.SuggestedRole,
                        Members = t.Members.Select(m => new
                        {
                            AccountId = m.AccountId,
                            Position = m.Position,
                            FullName = m.FullName,
                            Picture = m.Picture
                        }).ToList()
                    }).ToList()
                }
            }));

            return result;
        }

        private ProjectDetailsDTO GenerateFallbackProjectDetailsResponse(int projectId)
        {
            return new ProjectDetailsDTO
            {
                Id = projectId,
                ProjectKey = $"PROJ-{projectId}",
                Name = "Fallback Project",
                ProjectType = "WEB_APPLICATION",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                Requirements = new List<RequirementResponseDTO>
                {
                    new RequirementResponseDTO { Id = 1, ProjectId = projectId, Title = "Fallback Requirement 1", Priority = "MEDIUM" }
                },
                ProjectMembers = new List<ProjectMemberWithPositionsResponseDTO>
                {
                    new ProjectMemberWithPositionsResponseDTO { Id = 1, AccountId = 1, ProjectId = projectId, FullName = "Fallback Admin", Picture = "https://fallback.com/admin.png", ProjectPositions = new List<ProjectPositionResponseDTO> { new ProjectPositionResponseDTO { Position = "PROJECT_MANAGER" } } },
                    new ProjectMemberWithPositionsResponseDTO { Id = 2, AccountId = 2, ProjectId = projectId, FullName = "Fallback Dev", Picture = "https://fallback.com/dev.png", ProjectPositions = new List<ProjectPositionResponseDTO> { new ProjectPositionResponseDTO { Position = "BACKEND_DEVELOPER" } } }
                }
            };
        }

        private List<RequirementRequestDTO> GenerateFallbackRequirementsResponse(int projectId)
        {
            return new List<RequirementRequestDTO>
            {
                new RequirementRequestDTO { ProjectId = projectId, Title = "Fallback Requirement 1", Description = "Initial setup" }
            };
        }

        private List<object> CreateEpicsResponse(DateTime startDate, DateTime endDate, int projectId, List<RequirementRequestDTO> requirements, string projectKey)
        {
            var epics = new List<object>();
            var epicCount = 5;

            for (int i = 0; i < epicCount; i++)
            {
                var epicStart = startDate.AddDays(i * 28);
                var epicEnd = i == epicCount - 1 ? endDate : epicStart.AddDays(27);
                epics.Add(new
                {
                    EpicId = $"{projectKey}-{i + 1}",
                    ProjectId = projectId,
                    Title = $"Epic {i + 1}: {requirements[i % requirements.Count].Title}",
                    Description = $"Epic for {requirements[i % requirements.Count].Description}",
                    StartDate = epicStart,
                    EndDate = epicEnd
                });
            }
            return epics;
        }

        private async Task<List<EpicWithTasksDTO>> GenerateEpicTasksFromAIResponse(DateTime startDate, DateTime endDate, int projectId, List<RequirementRequestDTO> requirements, List<ProjectMember> projectMembers, List<ProjectPosition> positions, string projectKey)
        {
            var projectDetails = await _projectService.GetProjectDetails(projectId);
            if (projectDetails == null)
            {
                _logger.LogWarning("No project details found for ProjectId {ProjectId} in GenerateEpicTasksFromAIResponse.", projectId);
                projectDetails = GenerateFallbackProjectDetailsResponse(projectId);
            }

            var projectName = projectDetails.Name;
            var projectType = projectDetails.ProjectType;
            var startDateStr = startDate.ToString("yyyy-MM-dd");
            var endDateStr = endDate.ToString("yyyy-MM-dd");

            var requirementsText = string.Join(", ", projectDetails.Requirements?.Select(r => r.Title) ?? new List<string>());
            var memberNames = string.Join(", ", projectDetails.ProjectMembers?.Where(pm => pm.AccountId != 1 && pm.AccountId != 4).Select(pm => pm.AccountId.ToString() ?? $"Account {pm.Id}") ?? new List<string>());
            var roles = string.Join(", ", projectDetails.ProjectMembers?.Where(pm => pm.AccountId != 1 && pm.AccountId != 4).SelectMany(pm => pm.ProjectPositions?.Select(pp => $"{pp.Position} (Account ID: {pm.AccountId})") ?? new List<string>()) ?? new List<string>());

            var prompt = $"Generate a task plan for a software project with ID {projectId}. " +
                         $"Project Name: '{projectName}'. Project Type: '{projectType}'. " +
                         $"Requirements: {requirementsText}. Duration: {startDateStr} to {endDateStr}. " +
                         $"Team Members (excluding Admin and PM who do not code): {memberNames}. Roles and Positions: {roles}. " +
                         $"Use Agile/Scrum methodology with 2-week sprints. " +
                         $"Create exactly 5 Epics based on requirements. For each Epic, generate 5-10 tasks with functions: 'Design', 'Development', 'Testing', 'Documentation', 'Review'. " +
                         $"Each task must have 'title', 'description', 'milestone' (e.g., 'Sprint 1'), 'startDate' (YYYY-MM-DD), 'endDate' (YYYY-MM-DD), 'suggestedRole', and 'assignedMembers' (1-3 members per task from available roles, ensuring even distribution among Frontend Developers, Backend Developers, Testers, Designers, and Team Leader; exclude Admin and PM from coding tasks). " +
                         $"Titles should be concise and action-oriented (e.g., 'Develop Login Page'). Distribute tasks evenly: 5+ tasks for Frontend Developers, 5+ for Backend Developers, 3+ for Testers, 3+ for Designers, 2+ for Team Leader. " +
                         $"Dates within {startDateStr} to {endDateStr}. Return ONLY a valid JSON array with structure: " +
                         $"[{{\"epicId\": \"<id>\", \"title\": \"<title>\", \"description\": \"<desc>\", \"startDate\": \"<date>\", \"endDate\": \"<date>\", \"tasks\": [{{ \"title\": \"<title>\", \"description\": \"<desc>\", \"milestone\": \"<milestone>\", \"startDate\": \"<date>\", \"endDate\": \"<date>\", \"suggestedRole\": \"<role>\", \"assignedMembers\": [{{ \"accountId\": <id>, \"position\": \"<pos>\" }}] }}]}}]. " +
                         "No markdown or text outside JSON.";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { maxOutputTokens = 3000, temperature = 0.7 }
            };

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"models/gemini-1.5-flash:generateContent?key={_apiKey}", content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Gemini API raw response: {Response}", jsonResponse);

                var aiResponse = JsonSerializer.Deserialize<GeminiResponseDTO>(jsonResponse);
                if (aiResponse?.candidates == null || !aiResponse.candidates.Any() ||
                    aiResponse.candidates[0]?.content?.parts == null || !aiResponse.candidates[0].content.parts.Any())
                {
                    _logger.LogWarning("No valid content from Gemini API.");
                    return GenerateFallbackEpicTasksResponse(startDate, endDate, requirements, projectMembers, positions, projectKey);
                }

                var epicTasksJson = aiResponse.candidates[0].content.parts[0].text;
                var cleanedJson = CleanJsonResponse(epicTasksJson);

                if (string.IsNullOrWhiteSpace(cleanedJson) || cleanedJson == "[]")
                {
                    _logger.LogWarning("No valid tasks from Gemini. Using fallback.");
                    return GenerateFallbackEpicTasksResponse(startDate, endDate, requirements, projectMembers, positions, projectKey);
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var aiEpicTasks = JsonSerializer.Deserialize<List<EpicWithTasksDTO>>(cleanedJson, options) ?? new List<EpicWithTasksDTO>();

                // Thay đổi epicId và taskId
                for (int i = 0; i < aiEpicTasks.Count; i++)
                {
                    aiEpicTasks[i].EpicId = $"{projectKey}-{i + 1}";
                    for (int j = 0; j < aiEpicTasks[i].Tasks.Count; j++)
                    {
                        aiEpicTasks[i].Tasks[j].TaskId = $"{projectKey}-{i + 1}-{j + 1}";
                    }
                }

                // Điền fullName và picture cho từng member
                foreach (var epic in aiEpicTasks)
                {
                    foreach (var task in epic.Tasks)
                    {
                        foreach (var member in task.Members)
                        {
                            var projectMember = projectMembers.FirstOrDefault(pm => pm.AccountId == member.AccountId);
                            if (projectMember != null)
                            {
                                member.FullName = projectMember.FullName;
                                member.Picture = projectMember.Picture;
                            }
                        }
                    }
                }

                _logger.LogInformation("Generated epic tasks count: {Count}", aiEpicTasks.Count);
                return aiEpicTasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenerateEpicTasksFromAIResponse");
                return GenerateFallbackEpicTasksResponse(startDate, endDate, requirements, projectMembers, positions, projectKey);
            }
        }

        private List<ProjectMember> GenerateFallbackMembersResponse(int projectId)
        {
            return new List<ProjectMember>
            {
                new ProjectMember { AccountId = 1, ProjectId = projectId, FullName = "Fallback Admin", Picture = "https://fallback.com/admin.png" },
                new ProjectMember { AccountId = 2, ProjectId = projectId, FullName = "Fallback Dev", Picture = "https://fallback.com/dev.png" }
            };
        }

        private List<ProjectPosition> GenerateFallbackPositionsResponse(int projectId)
        {
            return new List<ProjectPosition>
            {
                new ProjectPosition { ProjectMemberId = 1, Id = projectId, Position = "PROJECT_MANAGER" },
                new ProjectPosition { ProjectMemberId = 2, Id = projectId, Position = "BACKEND_DEVELOPER" }
            };
        }

        private List<EpicWithTasksDTO> GenerateFallbackEpicTasksResponse(DateTime startDate, DateTime endDate, List<RequirementRequestDTO> requirements, List<ProjectMember> projectMembers, List<ProjectPosition> positions, string projectKey)
        {
            var epicTasks = new List<EpicWithTasksDTO>();
            var epicCount = 5;

            for (int i = 0; i < epicCount; i++)
            {
                var epicStart = startDate.AddDays(i * 28);
                var epicEnd = i == epicCount - 1 ? endDate : epicStart.AddDays(27);
                var epic = new EpicWithTasksDTO
                {
                    EpicId = $"{projectKey}-{i + 1}",
                    Title = $"Epic {i + 1}: {requirements[i % requirements.Count].Title}",
                    Description = $"Epic for {requirements[i % requirements.Count].Description}",
                    StartDate = epicStart,
                    EndDate = epicEnd,
                    Tasks = new List<TaskWithMembersDTO>()
                };

                var taskCount = new Random().Next(5, 11);
                for (int j = 0; j < taskCount; j++)
                {
                    var taskStart = epicStart.AddDays(j * (28 / taskCount));
                    var taskEnd = j == taskCount - 1 ? epicEnd : taskStart.AddDays((28 / taskCount) - 1);
                    var suggestedRole = GetSuggestedRoleResponse(j, positions);
                    var task = new TaskWithMembersDTO
                    {
                        TaskId = $"{projectKey}-{i + 1}-{j + 1}",
                        Title = $"Task {j + 1} for {requirements[i % requirements.Count].Title}",
                        Description = $"Task {j + 1} description for {requirements[i % requirements.Count].Title}",
                        Milestone = $"Sprint {i + 1}",
                        StartDate = taskStart,
                        EndDate = taskEnd,
                        SuggestedRole = suggestedRole,
                        Members = AssignMembersToTaskResponse(positions, suggestedRole).Take(new Random().Next(1, 4)).ToList()
                    };
                    // Điền fullName và picture cho fallback
                    foreach (var member in task.Members)
                    {
                        var projectMember = projectMembers.FirstOrDefault(pm => pm.AccountId == member.AccountId);
                        if (projectMember != null)
                        {
                            member.FullName = projectMember.FullName;
                            member.Picture = projectMember.Picture;
                        }
                    }
                    epic.Tasks.Add(task);
                }
                epicTasks.Add(epic);
            }
            _logger.LogInformation("Generated fallback epic tasks: {Count}", epicTasks.Count);
            return epicTasks;
        }

        private string GetSuggestedRoleResponse(int index, List<ProjectPosition> positions)
        {
            var roles = positions.Select(p => p.Position).Distinct().Where(r => r != "ADMIN" && r != "PROJECT_MANAGER").ToArray();
            if (!roles.Any()) roles = new[] { "FRONTEND_DEVELOPER", "BACKEND_DEVELOPER", "TESTER", "DESIGNER", "TEAM_LEADER" };
            return roles[index % roles.Length];
        }

        private List<MemberAssignmentDTO> AssignMembersToTaskResponse(List<ProjectPosition> positions, string suggestedRole)
        {
            return positions
                .Where(p => p.Position.ToLower().Contains(suggestedRole.ToLower().Replace(" ", "")) && p.Position != "ADMIN" && p.Position != "PROJECT_MANAGER")
                .Select(p => new MemberAssignmentDTO { AccountId = p.ProjectMemberId, Position = p.Position })
                .ToList();
        }

        private string CleanJsonResponse(string rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson)) return "[]";
            var cleaned = rawJson.Trim();
            if (cleaned.StartsWith("```json")) cleaned = cleaned.Substring(7).Trim();
            if (cleaned.StartsWith("```")) cleaned = cleaned.Substring(3).Trim();
            if (cleaned.EndsWith("```")) cleaned = cleaned.Substring(0, cleaned.Length - 3).Trim();
            if (!cleaned.EndsWith("]")) cleaned += "]";
            if (!cleaned.StartsWith("[")) cleaned = "[" + cleaned;
            try { JsonSerializer.Deserialize<object>(cleaned); return cleaned; }
            catch { _logger.LogWarning("Invalid JSON: {RawJson}", rawJson); return "[]"; }
        }

        private List<TaskAssignment> AssignTasksToMembersResponse(List<Tasks> tasks, List<ProjectMember> projectMembers, List<ProjectPosition> positions)
        {
            var assignments = new List<TaskAssignment>();
            var roleMap = positions.ToDictionary(p => p.ProjectMemberId, p => p.Position.ToLower());

            foreach (var task in tasks)
            {
                var suitableMember = projectMembers
                    .Where(m => roleMap.ContainsKey(m.AccountId) && roleMap[m.AccountId] != "admin" && roleMap[m.AccountId] != "project_manager")
                    .OrderBy(m => Guid.NewGuid())
                    .FirstOrDefault(m => IsSuitableForRoleResponse(roleMap[m.AccountId], task.Title));

                if (suitableMember != null)
                {
                    var assignment = new TaskAssignment
                    {
                        TaskId = task.Id.ToString(),
                        AccountId = suitableMember.AccountId,
                        Status = "Assigned",
                        AssignedAt = DateTime.UtcNow,
                        HourlyRate = 50.00m
                    };
                    assignments.Add(assignment);
                    _taskAssignmentService.CreateTaskAssignment(_mapper.Map<TaskAssignmentRequestDTO>(assignment)).GetAwaiter().GetResult();
                }
            }
            return assignments;
        }

        private bool IsSuitableForRoleResponse(string position, string taskTitle)
        {
            position = position.ToLower();
            taskTitle = taskTitle.ToLower();
            return (position.Contains("be") && taskTitle.Contains("develop"))
                || (position.Contains("fe") && taskTitle.Contains("design"))
                || (position.Contains("tester") && taskTitle.Contains("test"))
                || (position.Contains("designer") && taskTitle.Contains("design"))
                || (position.Contains("tl") && taskTitle.Contains("plan"))
                || (position.Contains("developer") && taskTitle.Contains("develop"));
        }
    }

    public class EpicWithTasksDTO
    {
        public string EpicId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<TaskWithMembersDTO> Tasks { get; set; }
    }

    public class TaskWithMembersDTO
    {
        public string TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Milestone { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SuggestedRole { get; set; }
        public List<MemberAssignmentDTO> Members { get; set; }
    }

    public class MemberAssignmentDTO
    {
        public int AccountId { get; set; }
        public string Position { get; set; }
        public string FullName { get; set; }
        public string Picture { get; set; }
    }

    public class GeminiResponseDTO
    {
        public List<GeminiCandidateDTO> candidates { get; set; }
    }

    public class GeminiCandidateDTO
    {
        public GeminiContentDTO content { get; set; }
        public string finishReason { get; set; }
    }

    public class GeminiContentDTO
    {
        public List<GeminiPartDTO> parts { get; set; }
    }

    public class GeminiPartDTO
    {
        public string text { get; set; }
    }

    public class ProjectMember
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int ProjectId { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime InvitedAt { get; set; }
        public string Status { get; set; }
        public string FullName { get; set; }
        public string Picture { get; set; }
        public List<ProjectPosition> ProjectPosition { get; set; }
    }

    public class ProjectPosition
    {
        public int Id { get; set; }
        public int ProjectMemberId { get; set; }
        public string Position { get; set; }
    }
}