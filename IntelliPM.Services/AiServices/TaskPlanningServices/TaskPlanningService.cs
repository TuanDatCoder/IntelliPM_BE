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

            // Lấy danh sách account_role và account_position từ dynamic categories
            var accountRoles = await _dynamicCategoryService.GetDynamicCategoryByCategoryGroup("account_role");
            var accountPositions = await _dynamicCategoryService.GetDynamicCategoryByCategoryGroup("account_position");

            var allRoles = accountRoles.Where(r => r.IsActive).Select(r => r.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var allPositions = accountPositions.Where(p => p.IsActive).Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Define excluded roles and positions từ DB
            var excludedRoleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CLIENT", "ADMIN" };
            var excludedPositionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CLIENT", "PROJECT_MANAGER", "ADMIN" };

            var excludedRoles = excludedRoleNames.Where(r => allRoles.Contains(r)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var excludedPositions = excludedPositionNames.Where(p => allPositions.Contains(p)).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Retrieve project members và filter out excluded roles/positions
            List<ProjectMemberWithPositionsResponseDTO> projectMembers;
            try
            {
                projectMembers = await _projectMemberService.GetProjectMemberWithPositionsByProjectId(projectId);
                projectMembers = projectMembers
                    .Where(m => m.ProjectPositions != null &&
                              m.ProjectPositions.Any(p => !excludedPositions.Contains(p.Position)))
                    .OrderBy(m => m.FullName)
                    .ToList();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("No project members found for Project ID {ProjectId}: {Message}", projectId, ex.Message);
                projectMembers = new List<ProjectMemberWithPositionsResponseDTO>();
            }

            // Tạo mapping roles từ DB cho AI prompt (với readable labels)
            var roleLabels = accountRoles.Where(r => r.IsActive && !excludedRoles.Contains(r.Name))
                                       .ToDictionary(r => r.Name, r => r.Label, StringComparer.OrdinalIgnoreCase);

            // Tạo member assignment tracker để đảm bảo đa dạng
            var memberAssignmentTracker = new Dictionary<int, int>(); // AccountId -> số lần được assign

            // Generate epic tasks với roles từ DB
            var epicTasks = await GenerateEpicTasksFromAIResponse(startDate, endDate, projectId, requirementRequests, projectKey, roleLabels);

            foreach (var epic in epicTasks)
            {
                // Validate và clean epic data
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
                    // Validate và clean task data
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

                    // Assign members dựa trên role và position một cách thông minh và đa dạng
                    task.AssignedMembers = AssignMembersToTaskIntelligently(task.SuggestedRole, projectMembers, allPositions, allRoles, memberAssignmentTracker);

                    // Fallback assignment nếu không tìm được ai phù hợp
                    if (task.AssignedMembers == null || !task.AssignedMembers.Any())
                    {
                        task.AssignedMembers = AssignFallbackMembers(projectMembers, memberAssignmentTracker);
                        if (task.AssignedMembers.Any())
                        {
                            _logger.LogWarning("No specific members found for role {SuggestedRole}, assigned fallback members", task.SuggestedRole);
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
                        Type = "EPIC", // Added Type property for Epic
                        Title = epic.Title,
                        Description = epic.Description,
                        StartDate = epic.StartDate,
                        EndDate = epic.EndDate,
                        Tasks = epic.Tasks.Select((t, taskIndex) => new
                        {
                            TaskId = $"{projectKey}-{result.Count + 1}-{taskIndex + 1}",
                            Type = t.Type, // Include Task Type (e.g., STORY)
                            Title = t.Title,
                            Description = t.Description,
                            StartDate = t.StartDate,
                            EndDate = t.EndDate,
                            SuggestedRole = t.SuggestedRole,
                            AssignedMembers = t.AssignedMembers?.Select(m => (object)new
                            {
                                m.AccountId,
                                m.Picture,
                                m.FullName
                            }).ToList() ?? new List<object>()
                        }).ToList()
                    }
                });
            }

            return result;
        }

        private List<TaskMemberDTO> AssignMembersToTaskIntelligently(string suggestedRole, List<ProjectMemberWithPositionsResponseDTO> projectMembers, HashSet<string> allPositions, HashSet<string> allRoles, Dictionary<int, int> memberAssignmentTracker)
        {
            var assignedMembers = new List<TaskMemberDTO>();

            if (projectMembers == null || !projectMembers.Any())
            {
                _logger.LogWarning("No project members available for assignment");
                return assignedMembers;
            }

            // Mapping intelligent từ SuggestedRole sang positions tương ứng
            var roleToPositionsMapping = GetRoleToPositionsMapping(suggestedRole, allPositions);

            if (!roleToPositionsMapping.Any())
            {
                _logger.LogWarning("No position mapping found for role: {SuggestedRole}", suggestedRole);
                return AssignFallbackMembers(projectMembers, memberAssignmentTracker);
            }

            // Tìm members có positions phù hợp
            var eligibleMembers = projectMembers
                .Where(m => m.ProjectPositions != null &&
                          m.ProjectPositions.Any(p => roleToPositionsMapping.Contains(p.Position)))
                .ToList();

            // Phân chia thông minh: 1-3 members tùy theo complexity và role
            var assignmentCount = DetermineOptimalAssignmentCount(suggestedRole, eligibleMembers.Count);

            if (eligibleMembers.Any())
            {
                // Chọn members đa dạng với load balancing
                var selectedMembers = SelectDiverseMembersWithLoadBalancing(eligibleMembers, roleToPositionsMapping, assignmentCount, memberAssignmentTracker);

                assignedMembers = selectedMembers
                    .Select(m => new TaskMemberDTO
                    {
                        AccountId = m.AccountId,
                        Picture = m.Picture,
                        FullName = m.FullName
                    })
                    .ToList();

                // Update assignment tracker
                foreach (var member in selectedMembers)
                {
                    memberAssignmentTracker[member.AccountId] = memberAssignmentTracker.GetValueOrDefault(member.AccountId, 0) + 1;
                }

                _logger.LogInformation("Assigned {Count} members to role {SuggestedRole}: {Members}",
                    assignedMembers.Count, suggestedRole, string.Join(", ", assignedMembers.Select(m => m.FullName)));
            }
            else
            {
                _logger.LogWarning("No eligible members found for role: {SuggestedRole}", suggestedRole);
            }

            return assignedMembers;
        }

        private HashSet<string> GetRoleToPositionsMapping(string suggestedRole, HashSet<string> allPositions)
        {
            var matchingPositions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            switch (suggestedRole?.ToLowerInvariant())
            {
                case "business analyst":
                case "business_analyst":
                    if (allPositions.Contains("BUSINESS_ANALYST"))
                        matchingPositions.Add("BUSINESS_ANALYST");
                    break;

                case "designer":
                    if (allPositions.Contains("DESIGNER"))
                        matchingPositions.Add("DESIGNER");
                    break;

                case "frontend developer":
                case "frontend_developer":
                case "front-end developer":
                    if (allPositions.Contains("FRONTEND_DEVELOPER"))
                        matchingPositions.Add("FRONTEND_DEVELOPER");
                    break;

                case "backend developer":
                case "backend_developer":
                case "back-end developer":
                    if (allPositions.Contains("BACKEND_DEVELOPER"))
                        matchingPositions.Add("BACKEND_DEVELOPER");
                    break;

                case "developer":
                case "full-stack developer":
                    // Developer chung có thể là cả frontend và backend
                    if (allPositions.Contains("FRONTEND_DEVELOPER"))
                        matchingPositions.Add("FRONTEND_DEVELOPER");
                    if (allPositions.Contains("BACKEND_DEVELOPER"))
                        matchingPositions.Add("BACKEND_DEVELOPER");
                    break;

                case "tester":
                case "qa":
                case "quality assurance":
                    if (allPositions.Contains("TESTER"))
                        matchingPositions.Add("TESTER");
                    break;

                case "team leader":
                case "team_leader":
                case "tech lead":
                    if (allPositions.Contains("TEAM_LEADER"))
                        matchingPositions.Add("TEAM_LEADER");
                    // Team leader cũng có thể có technical skills
                    if (allPositions.Contains("FRONTEND_DEVELOPER"))
                        matchingPositions.Add("FRONTEND_DEVELOPER");
                    if (allPositions.Contains("BACKEND_DEVELOPER"))
                        matchingPositions.Add("BACKEND_DEVELOPER");
                    break;

                default:
                    _logger.LogWarning("Unknown SuggestedRole: {SuggestedRole}, using default developer positions", suggestedRole);
                    // Default fallback cho unknown roles
                    if (allPositions.Contains("FRONTEND_DEVELOPER"))
                        matchingPositions.Add("FRONTEND_DEVELOPER");
                    if (allPositions.Contains("BACKEND_DEVELOPER"))
                        matchingPositions.Add("BACKEND_DEVELOPER");
                    break;
            }

            return matchingPositions;
        }

        private int DetermineOptimalAssignmentCount(string suggestedRole, int availableCount)
        {
            if (availableCount == 0) return 0;

            // Các role phức tạp cần nhiều người hơn
            switch (suggestedRole?.ToLowerInvariant())
            {
                case "developer":
                case "full-stack developer":
                    return Math.Min(3, availableCount); // Developer tasks có thể cần nhiều người

                case "team leader":
                case "team_leader":
                    return Math.Min(2, availableCount); // Team leader + 1 member

                case "tester":
                case "designer":
                case "business analyst":
                case "business_analyst":
                    return Math.Min(2, availableCount); // Thường 1-2 người là đủ

                default:
                    return Math.Min(2, availableCount); // Default 1-2 người
            }
        }

        private List<ProjectMemberWithPositionsResponseDTO> SelectDiverseMembersWithLoadBalancing(
            List<ProjectMemberWithPositionsResponseDTO> eligibleMembers,
            HashSet<string> roleToPositionsMapping,
            int assignmentCount,
            Dictionary<int, int> memberAssignmentTracker)
        {
            var selectedMembers = new List<ProjectMemberWithPositionsResponseDTO>();
            var usedPositions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Sắp xếp members theo số lần được assign (ít nhất trước) và sau đó theo tên
            var sortedMembers = eligibleMembers
                .OrderBy(m => memberAssignmentTracker.GetValueOrDefault(m.AccountId, 0)) // Ít được assign nhất trước
                .ThenBy(m => m.FullName) // Sau đó sort theo tên
                .ToList();

            // Phase 1: Ưu tiên chọn members có position đa dạng và ít được assign
            foreach (var member in sortedMembers)
            {
                if (selectedMembers.Count >= assignmentCount) break;

                var memberPositions = member.ProjectPositions.Select(p => p.Position).ToList();
                var matchingPositions = memberPositions.Where(p => roleToPositionsMapping.Contains(p)).ToList();

                // Ưu tiên members có position chưa được chọn HOẶC ít được assign nhất
                bool hasNewPosition = matchingPositions.Any(p => !usedPositions.Contains(p));
                int currentAssignmentCount = memberAssignmentTracker.GetValueOrDefault(member.AccountId, 0);

                if (hasNewPosition || selectedMembers.Count == 0 ||
                    currentAssignmentCount <= selectedMembers.Min(sm => memberAssignmentTracker.GetValueOrDefault(sm.AccountId, 0)))
                {
                    selectedMembers.Add(member);
                    foreach (var pos in matchingPositions)
                    {
                        usedPositions.Add(pos);
                    }
                }
            }

            // Phase 2: Nếu chưa đủ người, chọn thêm từ remaining members (ưu tiên ít được assign)
            if (selectedMembers.Count < assignmentCount)
            {
                var remainingMembers = sortedMembers
                    .Except(selectedMembers)
                    .Take(assignmentCount - selectedMembers.Count);
                selectedMembers.AddRange(remainingMembers);
            }

            // Shuffle randomly trong nhóm có cùng assignment count để tăng đa dạng
            var finalSelection = new List<ProjectMemberWithPositionsResponseDTO>();
            var groupedByAssignmentCount = selectedMembers
                .GroupBy(m => memberAssignmentTracker.GetValueOrDefault(m.AccountId, 0))
                .OrderBy(g => g.Key);

            var random = new Random();
            foreach (var group in groupedByAssignmentCount)
            {
                var shuffledGroup = group.OrderBy(x => random.Next()).ToList();
                finalSelection.AddRange(shuffledGroup);
            }

            return finalSelection.Take(assignmentCount).ToList();
        }

        private List<TaskMemberDTO> AssignFallbackMembers(List<ProjectMemberWithPositionsResponseDTO> projectMembers, Dictionary<int, int> memberAssignmentTracker)
        {
            if (!projectMembers.Any()) return new List<TaskMemberDTO>();

            // Sắp xếp theo số lần được assign (ít nhất trước) và shuffle trong cùng nhóm
            var sortedMembers = projectMembers
                .OrderBy(m => memberAssignmentTracker.GetValueOrDefault(m.AccountId, 0))
                .ThenBy(m => Guid.NewGuid()) // Random shuffle trong cùng assignment count
                .Take(2) // Default assign 2 members
                .ToList();

            var assignedMembers = sortedMembers
                .Select(m => new TaskMemberDTO
                {
                    AccountId = m.AccountId,
                    Picture = m.Picture,
                    FullName = m.FullName
                })
                .ToList();

            // Update assignment tracker
            foreach (var member in sortedMembers)
            {
                memberAssignmentTracker[member.AccountId] = memberAssignmentTracker.GetValueOrDefault(member.AccountId, 0) + 1;
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

        private async Task<List<EpicWithTasksDTO>> GenerateEpicTasksFromAIResponse(DateTime startDate, DateTime endDate, int projectId, List<RequirementRequestDTO> requirements, string projectKey, Dictionary<string, string> roleLabels)
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

            // Tạo danh sách roles từ DB cho AI
            var availableRoles = string.Join(", ", roleLabels.Select(r => $"{r.Key} ({r.Value})"));

            var prompt = $@"⚠️ Task titles MUST be 100% unique across all Epics; no repetition or generic terms like 'Task X for...'. Keep all titles concise (under 100 characters).

Generate a comprehensive task plan for project ID {projectId}:

PROJECT DETAILS:
- Name: '{projectName}'
- Type: '{projectType}'
- Requirements: {requirementsText}
- Duration: {startDateStr} to {endDateStr}

AVAILABLE ROLES FROM DATABASE: {availableRoles}

USER STORY FORMAT EXAMPLES:
- ""As a project manager, I want to track project progress so that I can ensure timely delivery""
- ""As a team leader, I want to assign tasks efficiently so that team productivity is maximized""
- ""As a business analyst, I want to gather requirements so that development aligns with business needs""
- ""As a frontend developer, I want to create responsive UI so that users have optimal experience""
- ""As a backend developer, I want to implement secure APIs so that data integrity is maintained""
- ""As a tester, I want to validate functionality so that bugs are identified early""
- ""As a designer, I want to create user-friendly interfaces so that user engagement increases""

INSTRUCTIONS:
1. Create {requirements.Count} Epics based on the requirements
2. For each Epic, generate a unique 'title' (under 100 chars), 'description' that represents a major feature, and 'type' set to 'EPIC'
3. For each Epic, generate 5 tasks with:
   - UNIQUE 'title' (under 100 chars) in User Story format using lowercase readable role names (e.g., 'project manager', 'frontend developer', 'business analyst')
   - Detailed 'description' explaining acceptance criteria and implementation requirements
   - 'startDate' and 'endDate' (YYYY-MM-DD format) within project duration
   - 'suggestedRole' using EXACT role names from the database (e.g., 'BUSINESS_ANALYST', 'FRONTEND_DEVELOPER', etc.)
   - 'type' set to 'STORY'

TASK WRITING GUIDELINES:
- Title: Use lowercase readable role names in User Story format (""As a frontend developer, I want...""), keep under 100 characters, Always start with capital ""A"" in ""As""
- Description: Include detailed acceptance criteria and technical requirements
- Focus on user value and business outcomes
- Each task should be implementable within 1-2 weeks
- Distribute roles evenly across tasks for balanced workload

IMPORTANT: Return ONLY a valid JSON array with this exact structure:
[
  {{
    ""epicId"": ""<unique_id>"",
    ""title"": ""<epic_title>"",
    ""description"": ""<epic_description>"",
    ""type"": ""EPIC"",
    ""startDate"": ""<YYYY-MM-DD>"",
    ""endDate"": ""<YYYY-MM-DD>"",
    ""tasks"": [
      {{
        ""title"": ""As a [lowercase_readable_role] I want [functionality] so that [benefit]"",
        ""description"": ""<detailed_acceptance_criteria_and_implementation_details>"",
        ""type"": ""STORY"",
        ""startDate"": ""<YYYY-MM-DD>"",
        ""endDate"": ""<YYYY-MM-DD>"",
        ""suggestedRole"": ""<EXACT_DATABASE_ROLE_NAME>""
      }}
    ]
  }}
]";

            var requestData = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { maxOutputTokens = 6000, temperature = 0.7 }
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

                // Validate và clean roles từ AI response
                foreach (var epic in aiEpicTasks)
                {
                    epic.EpicId = $"{projectKey}-{aiEpicTasks.IndexOf(epic) + 1}";
                    epic.AIGenerated = true;

                    foreach (var task in epic.Tasks)
                    {
                        task.TaskId = $"{epic.EpicId}-{epic.Tasks.IndexOf(task) + 1}";

                        // Validate suggestedRole against database roles
                        if (!string.IsNullOrEmpty(task.SuggestedRole) && !roleLabels.ContainsKey(task.SuggestedRole))
                        {
                            _logger.LogWarning("Invalid role {Role} from AI, defaulting to TEAM_MEMBER", task.SuggestedRole);
                            task.SuggestedRole = "TEAM_MEMBER"; // Default fallback role
                        }
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

            // Remove markdown code blocks
            if (cleaned.StartsWith("```json"))
                cleaned = cleaned.Substring(7).Trim();
            if (cleaned.StartsWith("```"))
                cleaned = cleaned.Substring(3).Trim();
            if (cleaned.EndsWith("```"))
                cleaned = cleaned.Substring(0, cleaned.Length - 3).Trim();

            // Ensure proper JSON array format
            if (!cleaned.StartsWith("["))
                cleaned = "[" + cleaned;
            if (!cleaned.EndsWith("]"))
                cleaned += "]";

            // Validate JSON
            try
            {
                JsonConvert.DeserializeObject<object>(cleaned);
                return cleaned;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Invalid JSON response, returning empty array. Error: {Error}\nRaw JSON: {RawJson}", ex.Message, rawJson);
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
        public string Type { get; set; } = "EPIC"; // Added Type property for Epic
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool AIGenerated { get; set; }
        public List<TaskWithMembersDTO> Tasks { get; set; } = new List<TaskWithMembersDTO>();
    }

    public class TaskWithMembersDTO
    {
        public string TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } = "STORY";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SuggestedRole { get; set; }
        public List<TaskMemberDTO> AssignedMembers { get; set; } = new List<TaskMemberDTO>();
    }

    public class TaskMemberDTO
    {
        public int AccountId { get; set; }
        public string Picture { get; set; }
        public string FullName { get; set; }
    }
}