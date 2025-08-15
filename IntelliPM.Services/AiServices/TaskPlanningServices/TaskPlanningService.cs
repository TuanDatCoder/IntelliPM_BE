using AutoMapper;
using IntelliPM.Data.DTOs.Project.Response;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.DTOs.Requirement.Request;
using IntelliPM.Data.DTOs.Requirement.Response;
using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Services.DynamicCategoryServices;
using IntelliPM.Services.EpicServices;
using IntelliPM.Services.ProjectMemberServices;
using IntelliPM.Services.ProjectServices;
using IntelliPM.Services.RequirementServices;
using IntelliPM.Services.SprintServices;
using IntelliPM.Services.TaskServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace IntelliPM.Services.AiServices.TaskPlanningServices
{
    public class TaskPlanningService : ITaskPlanningService
    {
        private readonly IProjectService _projectService;
        private readonly IRequirementService _requirementService;
        private readonly ITaskService _taskService;
        private readonly IEpicService _epicService;
        private readonly ISprintService _sprintService;
        private readonly IProjectMemberService _projectMemberService;
        private readonly IDynamicCategoryService _dynamicCategoryService;
        private readonly IMapper _mapper;
        private readonly ILogger<TaskPlanningService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _url;

        public TaskPlanningService(
            IProjectService projectService,
            IRequirementService requirementService,
            ITaskService taskService,
            IEpicService epicService,
            ISprintService sprintService,
            IProjectMemberService projectMemberService,
            IDynamicCategoryService dynamicCategoryService,
            IMapper mapper,
            ILogger<TaskPlanningService> logger,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _requirementService = requirementService ?? throw new ArgumentNullException(nameof(requirementService));
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _epicService = epicService ?? throw new ArgumentNullException(nameof(epicService));
            _sprintService = sprintService ?? throw new ArgumentNullException(nameof(sprintService));
            _projectMemberService = projectMemberService ?? throw new ArgumentNullException(nameof(projectMemberService));
            _dynamicCategoryService = dynamicCategoryService ?? throw new ArgumentNullException(nameof(dynamicCategoryService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            // Move API key to configuration for security
            var apiKey = configuration["Gemini:ApiKey"] ?? "AIzaSyD52tMVJMjE9GxHZwshWwobgQ8bI4rGabA";
            _url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";
        }

        public async Task<List<object>> GenerateTaskPlan(int projectId)
        {
            if (projectId <= 0)
                throw new ArgumentException("Invalid request. ProjectId must be greater than 0.");

            var result = new List<object>();

            // Retrieve project details
            var projectResponse = await _projectService.GetProjectById(projectId);
            if (projectResponse == null)
            {
                projectResponse = GenerateFallbackProjectResponse(projectId);
            }

            var projectKey = projectResponse.ProjectKey ?? $"PROJ-{projectId}";
            var startDate = projectResponse.StartDate ?? DateTime.UtcNow;
            var endDate = projectResponse.EndDate ?? startDate.AddMonths(6);

            // Retrieve requirements
            var requirements = await _requirementService.GetAllRequirements(projectId);
            if (requirements == null || !requirements.Any())
            {
                requirements = _mapper.Map<List<RequirementResponseDTO>>(GenerateFallbackRequirementsResponse(projectId));
            }
            var requirementRequests = _mapper.Map<List<RequirementRequestDTO>>(requirements);

            // Retrieve all account positions from dynamic categories
            var accountPositions = await _dynamicCategoryService.GetDynamicCategoryByCategoryGroup("account_position");
            var allPositions = accountPositions.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Define excluded positions (validated against DB positions)
            var excludedPositionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CLIENT", "PROJECT_MANAGER", "ADMIN" };
            var excludedPositions = excludedPositionNames.Where(p => allPositions.Contains(p)).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Retrieve project members and filter out excluded positions
            List<ProjectMemberWithPositionsResponseDTO> projectMembers;
            try
            {
                projectMembers = await _projectMemberService.GetProjectMemberWithPositionsByProjectId(projectId);
                projectMembers = projectMembers
                    .Where(m => m.ProjectPositions != null && m.ProjectPositions.Any(p => !excludedPositions.Contains(p.Position)))
                    .OrderBy(m => m.FullName)
                    .ToList();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("No project members found for Project ID {ProjectId}: {Message}", projectId, ex.Message);
                projectMembers = new List<ProjectMemberWithPositionsResponseDTO>();
            }

            // Generate epic tasks
            var epicTasks = await GenerateEpicTasksFromAIResponse(startDate, endDate, projectId, requirementRequests, projectKey);

            foreach (var epic in epicTasks)
            {
                if (string.IsNullOrWhiteSpace(epic.Title))
                {
                    epic.Title = $"Unnamed Epic - {epic.EpicId ?? "Unknown"}";
                }
                else if (epic.Title.Length > 100)
                {
                    epic.Title = epic.Title.Substring(0, 100) + "...";
                }
                if (string.IsNullOrWhiteSpace(epic.Description))
                {
                    epic.Description = $"Default description for {epic.EpicId ?? "Unknown"}";
                }

                foreach (var task in epic.Tasks)
                {
                    if (string.IsNullOrWhiteSpace(task.Title))
                    {
                        task.Title = $"Unnamed Task - {task.TaskId ?? "Unknown"}";
                    }
                    else if (task.Title.Length > 100)
                    {
                        task.Title = task.Title.Substring(0, 100) + "...";
                    }
                    if (string.IsNullOrWhiteSpace(task.Description))
                    {
                        task.Description = $"Default description for {task.TaskId ?? "Unknown"}";
                    }

                    var taskRequest = new TaskRequestDTO
                    {
                        Title = task.Title,
                        Description = task.Description,
                        ProjectId = projectId,
                        ReporterId = 2
                    };

                    // Assign 1 to 3 members based on role, sorted by FullName
                    task.AssignedMembers = AssignMembersToTask(task.SuggestedRole, projectMembers, allPositions);

                    // If no members assigned, assign default members (up to 3, sorted)
                    if (task.AssignedMembers == null || !task.AssignedMembers.Any())
                    {
                        if (projectMembers.Any())
                        {
                            task.AssignedMembers = projectMembers
                                .Take(3)
                                .Select(m => new TaskMemberDTO
                                {
                                    AccountId = m.AccountId,
                                    Picture = m.Picture,
                                    FullName = m.FullName
                                })
                                .OrderBy(m => m.FullName)
                                .ToList();
                            _logger.LogWarning("No eligible members found for role {SuggestedRole}, assigned up to 3 default members (sorted by name)",
                                task.SuggestedRole);
                        }
                        else
                        {
                            _logger.LogError("No project members available to assign to task {TaskTitle}", task.Title);
                        }
                    }
                }

                result.Add(new
                {
                    Type = "Epic",
                    AIGenerated = epic.AIGenerated,
                    Data = new
                    {
                        EpicId = $"{projectKey}-{result.Count + 1}",
                        Title = epic.Title,
                        Description = epic.Description,
                        StartDate = epic.StartDate,
                        EndDate = epic.EndDate,
                        Tasks = epic.Tasks.Select((t, taskIndex) => new
                        {
                            TaskId = $"{projectKey}-{result.Count + 1}-{taskIndex + 1}",
                            Title = t.Title,
                            Description = t.Description,
                            StartDate = t.StartDate,
                            EndDate = t.EndDate,
                            SuggestedRole = t.SuggestedRole,
                            AssignedMembers = t.AssignedMembers != null
                                ? t.AssignedMembers.Select(m => (object)new
                                {
                                    m.AccountId,
                                    m.Picture,
                                    m.FullName
                                }).ToList()
                                : new List<object>()
                        }).ToList()
                    }
                });
            }

            return result;
        }

        private List<TaskMemberDTO> AssignMembersToTask(string suggestedRole, List<ProjectMemberWithPositionsResponseDTO> projectMembers, HashSet<string> allPositions)
        {
            var assignedMembers = new List<TaskMemberDTO>();

            if (projectMembers == null || !projectMembers.Any())
            {
                _logger.LogWarning("No project members available for assignment");
                return assignedMembers;
            }

            // Map SuggestedRole to matching positions (using DB positions for validation)
            var matchingPositionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            switch (suggestedRole?.ToLowerInvariant())
            {
                case "designer":
                    matchingPositionNames.Add("DESIGNER");
                    break;
                case "developer":
                    matchingPositionNames.Add("FRONTEND_DEVELOPER");
                    matchingPositionNames.Add("BACKEND_DEVELOPER");
                    break;
                case "tester":
                    matchingPositionNames.Add("TESTER");
                    break;
                default:
                    _logger.LogWarning("Unknown SuggestedRole: {SuggestedRole}, defaulting to developers", suggestedRole);
                    matchingPositionNames.Add("FRONTEND_DEVELOPER");
                    matchingPositionNames.Add("BACKEND_DEVELOPER");
                    break;
            }

            // Filter to only valid positions from DB
            var matchingPositions = matchingPositionNames.Where(p => allPositions.Contains(p)).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Filter eligible members with matching positions, sorted by FullName
            var eligibleMembers = projectMembers
                .Where(m => m.ProjectPositions != null && m.ProjectPositions.Any(p => matchingPositions.Contains(p.Position)))
                .OrderBy(m => m.FullName)
                .Take(3) // Limit to 3 members
                .ToList();

            if (eligibleMembers.Any())
            {
                assignedMembers = eligibleMembers
                    .Select(m => new TaskMemberDTO
                    {
                        AccountId = m.AccountId,
                        Picture = m.Picture,
                        FullName = m.FullName
                    })
                    .ToList();

                _logger.LogInformation("Assigned {Count} members to role {SuggestedRole}", assignedMembers.Count, suggestedRole);
            }
            else
            {
                _logger.LogWarning("No eligible members found for role: {SuggestedRole}", suggestedRole);
            }

            return assignedMembers;
        }

        private ProjectResponseDTO GenerateFallbackProjectResponse(int projectId)
        {
            return new ProjectResponseDTO
            {
                Id = projectId,
                ProjectKey = $"PROJ-{projectId}",
                Name = "Fallback Project",
                ProjectType = "WEB_APPLICATION",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6)
            };
        }

        private List<RequirementRequestDTO> GenerateFallbackRequirementsResponse(int projectId)
        {
            return new List<RequirementRequestDTO>
            {
                new RequirementRequestDTO { ProjectId = projectId, Title = "Fallback Requirement 1", Description = "Initial setup" }
            };
        }

        private async Task<List<EpicWithTasksDTO>> GenerateEpicTasksFromAIResponse(DateTime startDate, DateTime endDate, int projectId, List<RequirementRequestDTO> requirements, string projectKey)
        {
            var projectResponse = await _projectService.GetProjectById(projectId);
            if (projectResponse == null)
            {
                projectResponse = GenerateFallbackProjectResponse(projectId);
            }

            var projectName = projectResponse.Name;
            var projectType = projectResponse.ProjectType;
            var startDateStr = startDate.ToString("yyyy-MM-dd");
            var endDateStr = endDate.ToString("yyyy-MM-dd");

            var requirementsText = string.Join(" | ", requirements.Select(r => $"{r.Title}: {r.Description}"));

            var prompt = $@"⚠️ Task titles MUST be 100% unique across all Epics; no repetition or generic terms like 'Task X for...'. Keep all titles concise (under 100 characters).

Generate a comprehensive task plan for project ID {projectId}:

PROJECT DETAILS:
- Name: '{projectName}'
- Type: '{projectType}'
- Requirements: {requirementsText}
- Duration: {startDateStr} to {endDateStr}

INSTRUCTIONS:
1. Create {requirements.Count} Epics based on the requirements
2. For each Epic, generate a unique 'title' (under 100 chars) and 'description' that represents a major feature
3. For each Epic, generate 5 tasks with:
   - UNIQUE 'title' (under 100 chars) in User Story format: ""As a [user type], I want [functionality] so that [benefit]""
   - Detailed 'description' explaining acceptance criteria and implementation requirements
   - 'startDate' and 'endDate' (YYYY-MM-DD format) within project duration
   - 'suggestedRole' (Designer/Developer/Tester) based on task nature

TASK WRITING GUIDELINES:
- Title: Use User Story format (As a... I want... so that...), keep under 100 characters
- Description: Include detailed acceptance criteria and technical requirements
- Focus on user value and business outcomes
- Each task should be implementable within 1-2 weeks

IMPORTANT: Return ONLY a valid JSON array with this exact structure:
[
  {{
    ""epicId"": ""<unique_id>"",
    ""title"": ""<epic_title>"",
    ""description"": ""<epic_description>"",
    ""startDate"": ""<YYYY-MM-DD>"",
    ""endDate"": ""<YYYY-MM-DD>"",
    ""tasks"": [
      {{
        ""title"": ""As a [user] I want [functionality] so that [benefit]"",
        ""description"": ""<detailed_acceptance_criteria_and_implementation_details>"",
        ""startDate"": ""<YYYY-MM-DD>"",
        ""endDate"": ""<YYYY-MM-DD>"",
        ""suggestedRole"": ""<Designer|Developer|Tester>""
      }}
    ]
  }}
]";

            var requestData = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { maxOutputTokens = 5000, temperature = 0.7 }
            };

            var requestJson = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API Error: {StatusCode}\nResponse: {ResponseString}", response.StatusCode, responseString);
                throw new Exception($"Gemini API Error: {response.StatusCode}");
            }

            if (string.IsNullOrWhiteSpace(responseString))
            {
                _logger.LogError("Gemini response is empty.");
                throw new Exception("Gemini response is empty.");
            }

            var parsedResponse = JsonConvert.DeserializeObject<GeminiResponseDTO>(responseString);
            var replyText = parsedResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text?.Trim();

            if (string.IsNullOrEmpty(replyText))
            {
                _logger.LogError("Gemini did not return any text response.");
                throw new Exception("Gemini did not return any text response.");
            }

            replyText = CleanJsonResponse(replyText);

            try
            {
                var aiEpicTasks = JsonConvert.DeserializeObject<List<EpicWithTasksDTO>>(replyText);
                if (aiEpicTasks == null || !aiEpicTasks.Any())
                {
                    _logger.LogError("No valid epic tasks from Gemini reply: {ReplyText}", replyText);
                    throw new Exception("No valid epic tasks from Gemini reply.");
                }

                for (int i = 0; i < aiEpicTasks.Count; i++)
                {
                    aiEpicTasks[i].EpicId = $"{projectKey}-{i + 1}";
                    aiEpicTasks[i].AIGenerated = true;
                    for (int j = 0; j < aiEpicTasks[i].Tasks.Count; j++)
                    {
                        aiEpicTasks[i].Tasks[j].TaskId = $"{projectKey}-{i + 1}-{j + 1}";
                    }
                }

                return aiEpicTasks;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deserializing epic tasks from Gemini reply: {ReplyText}\n{ErrorMessage}", replyText, ex.Message);
                throw new Exception("Error deserializing epic tasks from Gemini reply.", ex);
            }
        }

        private string CleanJsonResponse(string rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson)) return "[]";
            var cleaned = rawJson.Trim();
            if (cleaned.StartsWith("```json")) cleaned = cleaned.Substring(7).Trim();
            if (cleaned.EndsWith("```")) cleaned = cleaned.Substring(0, cleaned.Length - 3).Trim();
            if (!cleaned.StartsWith("[")) cleaned = "[" + cleaned;
            if (!cleaned.EndsWith("]")) cleaned += "]";
            try
            {
                JsonConvert.DeserializeObject<object>(cleaned);
                return cleaned;
            }
            catch
            {
                _logger.LogWarning("Invalid JSON response, returning empty array: {RawJson}", rawJson);
                return "[]";
            }
        }

        private class GeminiResponseDTO
        {
            public List<Candidate> candidates { get; set; }

            public class Candidate
            {
                public Content content { get; set; }
            }

            public class Content
            {
                public List<Part> parts { get; set; }
            }

            public class Part
            {
                public string text { get; set; }
            }
        }
    }

    public class EpicWithTasksDTO
    {
        public string EpicId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool AIGenerated { get; set; }
        public List<TaskWithMembersDTO> Tasks { get; set; }
    }

    public class TaskWithMembersDTO
    {
        public string TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SuggestedRole { get; set; }
        public List<TaskMemberDTO> AssignedMembers { get; set; }
    }

    public class TaskMemberDTO
    {
        public int AccountId { get; set; }
        public string Picture { get; set; }
        public string FullName { get; set; }
    }
}