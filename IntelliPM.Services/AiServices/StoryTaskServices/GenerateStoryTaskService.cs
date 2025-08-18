using AutoMapper;
using IntelliPM.Data.DTOs.Ai.GenerateStoryTask.Request;
using IntelliPM.Data.DTOs.Project.Response;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.DTOs.Requirement.Request;
using IntelliPM.Data.DTOs.Requirement.Response;
using IntelliPM.Services.DynamicCategoryServices;
using IntelliPM.Services.ProjectMemberServices;
using IntelliPM.Services.ProjectServices;
using IntelliPM.Services.RequirementServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace IntelliPM.Services.AiServices.StoryTaskServices
{
    public class GenerateStoryTaskService : IGenerateStoryTaskService
    {
        private readonly IProjectService _projectService;
        private readonly IRequirementService _requirementService;
        private readonly IProjectMemberService _projectMemberService;
        private readonly IDynamicCategoryService _dynamicCategoryService;
        private readonly IMapper _mapper;
        private readonly ILogger<GenerateStoryTaskService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _url;

        private const string StoryPromptTemplate = @"⚠️ Story titles MUST be 100% unique; no repetition or generic terms like 'Story X for...'. Keep all titles concise (under 100 characters).
⚠️ Do NOT use any of the following existing titles: {0}

Generate 5 stories for project ID {1} within epic '{2}':

PROJECT DETAILS:
- Name: '{3}'
- Type: '{4}'
- Requirements: {5}
- Epic: '{2}'
- Epic Duration: {6} to {7}

AVAILABLE ROLES FROM DATABASE: {8}

USER STORY FORMAT EXAMPLES:
- ""As a project manager, I want to track project progress so that I can ensure timely delivery""
- ""As a team leader, I want to assign tasks efficiently so that team productivity is maximized""
- ""As a business analyst, I want to gather requirements so that development aligns with business needs""
- ""As a frontend developer, I want to create responsive UI so that users have optimal experience""
- ""As a backend developer, I want to implement secure APIs so that data integrity is maintained""
- ""As a tester, I want to validate functionality so that bugs are identified early""
- ""As a designer, I want to create user-friendly interfaces so that user engagement increases""

INSTRUCTIONS:
1. Generate 5 stories based on the requirements and epic context
2. Each story must have:
   - UNIQUE 'title' (under 100 chars) in User Story format: ""As a [lowercase_readable_role] I want [functionality] so that [benefit]""
   - Detailed 'description' with acceptance criteria and implementation details
   - 'startDate' and 'endDate' (YYYY-MM-DD) within epic duration ({6} to {7})
   - 'suggestedRole' using EXACT role names from the database (e.g., 'BUSINESS_ANALYST', 'FRONTEND_DEVELOPER')
3. Stories should be implementable within 1-2 weeks
4. Distribute roles evenly for balanced workload
5. Ensure titles do not match any in the existing titles list: {0}

RETURN ONLY a valid JSON array with this structure:
[
  {{
    ""itemId"": ""<unique_id>"",
    ""title"": ""As a [lowercase_readable_role] I want [functionality] so that [benefit]"",
    ""description"": ""<detailed_acceptance_criteria_and_implementation_details>"",
    ""startDate"": ""<YYYY-MM-DD>"",
    ""endDate"": ""<YYYY-MM-DD>"",
    ""suggestedRole"": ""<EXACT_DATABASE_ROLE_NAME>""
  }}
]";

        private const string TaskPromptTemplate = @"⚠️ Task titles MUST be 100% unique; no repetition or generic terms like 'Task X for...'. Keep all titles concise (under 100 characters).
⚠️ Do NOT use any of the following existing titles: {0}

Generate 5 tasks for project ID {1} within epic '{2}' and story '{3}':

PROJECT DETAILS:
- Name: '{4}'
- Type: '{5}'
- Requirements: {6}
- Epic: '{2}'
- Epic Duration: {7} to {8}
- Story: '{3}'
- Story Duration: {9} to {10}

AVAILABLE ROLES FROM DATABASE: {11}

USER STORY FORMAT EXAMPLES:
- ""As a project manager, I want to track project progress so that I can ensure timely delivery""
- ""As a team leader, I want to prawassign tasks efficiently so that team productivity is maximized""
- ""As a business analyst, I want to gather requirements so that development aligns with business needs""
- ""As a frontend developer, I want to create responsive UI so that users have optimal experience""
- ""As a backend developer, I want to implement secure APIs so that data integrity is maintained""
- ""As a tester, I want to validate functionality so that bugs are identified early""
- ""As a designer, I want to create user-friendly interfaces so that user engagement increases""

INSTRUCTIONS:
1. Generate 5 tasks based on the requirements, epic, and story context
2. Each task must have:
   - UNIQUE 'title' (under 100 chars) in User Story format: ""As a [lowercase_readable_role] I want [functionality] so that [benefit]""
   - Detailed 'description' with acceptance criteria and implementation details
   - 'startDate' and 'endDate' (YYYY-MM-DD) within story duration ({9} to {10})
   - 'suggestedRole' using EXACT role names from the database (e.g., 'BUSINESS_ANALYST', 'FRONTEND_DEVELOPER')
3. Tasks should be implementable within 3-5 days
4. Distribute roles evenly for balanced workload
5. Ensure titles do not match any in the existing titles list: {0}

RETURN ONLY a valid JSON array with this structure:
[
  {{
    ""itemId"": ""<unique_id>"",
    ""title"": ""As a [lowercase_readable_role] I want [functionality] so that [benefit]"",
    ""description"": ""<detailed_acceptance_criteria_and_implementation_details>"",
    ""startDate"": ""<YYYY-MM-DD>"",
    ""endDate"": ""<YYYY-MM-DD>"",
    ""suggestedRole"": ""<EXACT_DATABASE_ROLE_NAME>""
  }}
]";

        public GenerateStoryTaskService(
            IProjectService projectService,
            IRequirementService requirementService,
            IProjectMemberService projectMemberService,
            IDynamicCategoryService dynamicCategoryService,
            IMapper mapper,
            ILogger<GenerateStoryTaskService> logger,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _requirementService = requirementService ?? throw new ArgumentNullException(nameof(requirementService));
            _projectMemberService = projectMemberService ?? throw new ArgumentNullException(nameof(projectMemberService));
            _dynamicCategoryService = dynamicCategoryService ?? throw new ArgumentNullException(nameof(dynamicCategoryService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            var apiKey = configuration["Gemini:ApiKey"] ?? "AIzaSyD52tMVJMjE9GxHZwshWwobgQ8bI4rGabA";
            _url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";
        }

        public async Task<List<object>> GenerateStoryOrTask(int projectId, GenerateStoryTaskRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.EpicTitle))
                throw new ArgumentException("Epic title is required.");
            if (request.Type != "STORY" && request.Type != "TASK")
                throw new ArgumentException("Type must be either 'STORY' or 'TASK'.");
            if (request.Type == "TASK" && string.IsNullOrWhiteSpace(request.StoryTitle))
                throw new ArgumentException("Story title is required for TASK type.");
            if (request.EpicStartDate >= request.EpicEndDate)
                throw new ArgumentException("Epic end date must be after start date.");
            if (request.Type == "TASK" && (request.StoryStartDate >= request.StoryEndDate ||
                request.StoryStartDate < request.EpicStartDate || request.StoryEndDate > request.EpicEndDate))
                throw new ArgumentException("Story dates must be within epic dates and valid.");

            var result = new List<object>();

            // Retrieve project details
            var projectResponse = await _projectService.GetProjectById(projectId);
            if (projectResponse == null)
            {
                projectResponse = GenerateFallbackProjectResponse(projectId);
            }

            var projectKey = projectResponse.ProjectKey ?? $"PROJ-{projectId}";
            var projectName = projectResponse.Name;
            var projectType = projectResponse.ProjectType;

            // Retrieve requirements
            var requirements = await _requirementService.GetAllRequirements(projectId);
            if (requirements == null || !requirements.Any())
            {
                requirements = _mapper.Map<List<RequirementResponseDTO>>(GenerateFallbackRequirementsResponse(projectId));
            }
            var requirementRequests = _mapper.Map<List<RequirementRequestDTO>>(requirements);

            // Retrieve roles and positions
            var accountRoles = await _dynamicCategoryService.GetDynamicCategoryByCategoryGroup("account_role");
            var accountPositions = await _dynamicCategoryService.GetDynamicCategoryByCategoryGroup("account_position");

            var allRoles = accountRoles.Where(r => r.IsActive).Select(r => r.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var allPositions = accountPositions.Where(p => p.IsActive).Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Define excluded roles and positions
            var excludedRoleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CLIENT", "ADMIN" };
            var excludedPositionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CLIENT", "PROJECT_MANAGER", "ADMIN" };

            var excludedRoles = excludedRoleNames.Where(r => allRoles.Contains(r)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var excludedPositionNamesSet = excludedPositionNames.Where(p => allPositions.Contains(p)).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Retrieve project members
            List<ProjectMemberWithPositionsResponseDTO> projectMembers;
            try
            {
                projectMembers = await _projectMemberService.GetProjectMemberWithPositionsByProjectId(projectId);
                projectMembers = projectMembers
                    .Where(m => m.ProjectPositions != null &&
                              m.ProjectPositions.Any(p => !excludedPositionNamesSet.Contains(p.Position)))
                    .OrderBy(m => m.FullName)
                    .ToList();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("No project members found for Project ID {ProjectId}: {Message}", projectId, ex.Message);
                projectMembers = new List<ProjectMemberWithPositionsResponseDTO>();
            }

            // Create role labels for AI prompt
            var roleLabels = accountRoles.Where(r => r.IsActive && !excludedRoles.Contains(r.Name))
                                       .ToDictionary(r => r.Name, r => r.Label, StringComparer.OrdinalIgnoreCase);

            // Member assignment tracker
            var memberAssignmentTracker = new Dictionary<int, int>();

            // Generate stories or tasks
            var generatedItems = await GenerateItemsFromAIResponse(
                request, projectName, projectType, projectKey, projectId, requirementRequests, roleLabels);

            foreach (var item in generatedItems)
            {
                // Validate and clean item data
                if (string.IsNullOrWhiteSpace(item.Title))
                {
                    item.Title = $"Unnamed {request.Type} - {item.ItemId ?? "Unknown"}";
                }
                else if (item.Title.Length > 100)
                {
                    item.Title = item.Title.Substring(0, 100) + "...";
                }
                if (string.IsNullOrWhiteSpace(item.Description))
                {
                    item.Description = $"Default description for {item.ItemId ?? "Unknown"}";
                }

                // Assign members
                item.AssignedMembers = AssignMembersToTaskIntelligently(item.SuggestedRole, projectMembers, allPositions, allRoles, memberAssignmentTracker);

                if (item.AssignedMembers == null || !item.AssignedMembers.Any())
                {
                    item.AssignedMembers = AssignFallbackMembers(projectMembers, memberAssignmentTracker);
                    if (item.AssignedMembers.Any())
                    {
                        _logger.LogWarning("No specific members found for role {SuggestedRole}, assigned fallback members", item.SuggestedRole);
                    }
                    else
                    {
                        _logger.LogError("No project members available to assign to {Type} {Title}", request.Type, item.Title);
                    }
                }

                result.Add(new
                {
                    Type = request.Type,
                    AIGenerated = true,
                    Data = new
                    {
                        ItemId = $"{projectKey}-{request.Type.ToLower()}-{result.Count + 1}",
                        item.Title,
                        item.Description,
                        item.StartDate,
                        item.EndDate,
                        item.SuggestedRole,
                        AssignedMembers = item.AssignedMembers?
                            .Select(m => (object)new
                            {
                                m.AccountId,
                                m.Picture,
                                m.FullName
                            })
                            .ToList()
                            ?? new List<object>()
                    }
                });
            }

            return result;
        }

        private async Task<List<StoryTaskDTO>> GenerateItemsFromAIResponse(
            GenerateStoryTaskRequestDTO request,
            string projectName,
            string projectType,
            string projectKey,
            int projectId,
            List<RequirementRequestDTO> requirements,
            Dictionary<string, string> roleLabels)
        {
            var requirementsText = string.Join(" | ", requirements.Select(r => $"{r.Title}: {r.Description}"));
            var availableRoles = string.Join(", ", roleLabels.Select(r => $"{r.Key} ({r.Value})"));
            var epicStartDateStr = request.EpicStartDate.ToString("yyyy-MM-dd");
            var epicEndDateStr = request.EpicEndDate.ToString("yyyy-MM-dd");
            var existingTitlesText = request.ExistingTitles != null && request.ExistingTitles.Any()
                ? string.Join(", ", request.ExistingTitles)
                : "None";

            string prompt;
            if (request.Type == "STORY")
            {
                prompt = string.Format(StoryPromptTemplate,
                    existingTitlesText, projectId, request.EpicTitle, projectName, projectType,
                    requirementsText, epicStartDateStr, epicEndDateStr, availableRoles);
            }
            else
            {
                var storyStartDateStr = request.StoryStartDate?.ToString("yyyy-MM-dd") ?? epicStartDateStr;
                var storyEndDateStr = request.StoryEndDate?.ToString("yyyy-MM-dd") ?? epicEndDateStr;
                prompt = string.Format(TaskPromptTemplate,
                    existingTitlesText, projectId, request.EpicTitle, request.StoryTitle, projectName, projectType,
                    requirementsText, epicStartDateStr, epicEndDateStr, storyStartDateStr, storyEndDateStr, availableRoles);
            }

            var requestData = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { maxOutputTokens = 4000, temperature = 0.7 }
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
                var aiItems = JsonConvert.DeserializeObject<List<StoryTaskDTO>>(replyText);
                if (aiItems == null || !aiItems.Any())
                {
                    _logger.LogError("No valid {Type} items from Gemini reply: {ReplyText}", request.Type, replyText);
                    throw new Exception($"No valid {request.Type} items from Gemini reply.");
                }

                // Validate titles against ExistingTitles
                if (request.ExistingTitles != null && request.ExistingTitles.Any())
                {
                    foreach (var item in aiItems)
                    {
                        if (request.ExistingTitles.Contains(item.Title, StringComparer.OrdinalIgnoreCase))
                        {
                            _logger.LogWarning("Duplicate title detected: {Title}. Generating new title.", item.Title);
                            item.Title = $"As a {item.SuggestedRole.ToLower().Replace("_", " ")} I want unique functionality {Guid.NewGuid().ToString().Substring(0, 8)} so that benefit is achieved";
                        }
                    }
                }

                foreach (var item in aiItems)
                {
                    item.ItemId = $"{projectKey}-{request.Type.ToLower()}-{aiItems.IndexOf(item) + 1}";
                    if (!string.IsNullOrEmpty(item.SuggestedRole) && !roleLabels.ContainsKey(item.SuggestedRole))
                    {
                        _logger.LogWarning("Invalid role {Role} from AI, defaulting to TEAM_MEMBER", item.SuggestedRole);
                        item.SuggestedRole = "TEAM_MEMBER";
                    }
                }

                return aiItems;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deserializing {Type} items from Gemini reply: {ReplyText}\n{ErrorMessage}", request.Type, replyText, ex.Message);
                throw new Exception($"Error deserializing {request.Type} items from Gemini reply.", ex);
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

        private List<TaskMemberDTO> AssignMembersToTaskIntelligently(
            string suggestedRole,
            List<ProjectMemberWithPositionsResponseDTO> projectMembers,
            HashSet<string> allPositions,
            HashSet<string> allRoles,
            Dictionary<int, int> memberAssignmentTracker)
        {
            var assignedMembers = new List<TaskMemberDTO>();

            if (projectMembers == null || !projectMembers.Any())
            {
                _logger.LogWarning("No project members available for assignment");
                return assignedMembers;
            }

            var roleToPositionsMapping = GetRoleToPositionsMapping(suggestedRole, allPositions);

            if (!roleToPositionsMapping.Any())
            {
                _logger.LogWarning("No position mapping found for role: {SuggestedRole}", suggestedRole);
                return AssignFallbackMembers(projectMembers, memberAssignmentTracker);
            }

            var eligibleMembers = projectMembers
                .Where(m => m.ProjectPositions != null &&
                          m.ProjectPositions.Any(p => roleToPositionsMapping.Contains(p.Position)))
                .ToList();

            var assignmentCount = DetermineOptimalAssignmentCount(suggestedRole, eligibleMembers.Count);

            if (eligibleMembers.Any())
            {
                var selectedMembers = SelectDiverseMembersWithLoadBalancing(eligibleMembers, roleToPositionsMapping, assignmentCount, memberAssignmentTracker);

                assignedMembers = selectedMembers
                    .Select(m => new TaskMemberDTO
                    {
                        AccountId = m.AccountId,
                        Picture = m.Picture,
                        FullName = m.FullName
                    })
                    .ToList();

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
                        matchingPositions.Add("DESsouthIGNER");
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
                    if (allPositions.Contains("FRONTEND_DEVELOPER"))
                        matchingPositions.Add("FRONTEND_DEVELOPER");
                    if (allPositions.Contains("BACKEND_DEVELOPER"))
                        matchingPositions.Add("BACKEND_DEVELOPER");
                    break;

                default:
                    _logger.LogWarning("Unknown SuggestedRole: {SuggestedRole}, using default developer positions", suggestedRole);
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

            switch (suggestedRole?.ToLowerInvariant())
            {
                case "developer":
                case "full-stack developer":
                    return Math.Min(3, availableCount);
                case "team leader":
                case "team_leader":
                    return Math.Min(2, availableCount);
                case "tester":
                case "designer":
                case "business analyst":
                case "business_analyst":
                    return Math.Min(2, availableCount);
                default:
                    return Math.Min(2, availableCount);
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

            var sortedMembers = eligibleMembers
                .OrderBy(m => memberAssignmentTracker.GetValueOrDefault(m.AccountId, 0))
                .ThenBy(m => m.FullName)
                .ToList();

            foreach (var member in sortedMembers)
            {
                if (selectedMembers.Count >= assignmentCount) break;

                var memberPositions = member.ProjectPositions.Select(p => p.Position).ToList();
                var matchingPositions = memberPositions.Where(p => roleToPositionsMapping.Contains(p)).ToList();

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

            if (selectedMembers.Count < assignmentCount)
            {
                var remainingMembers = sortedMembers
                    .Except(selectedMembers)
                    .Take(assignmentCount - selectedMembers.Count);
                selectedMembers.AddRange(remainingMembers);
            }

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

        private List<TaskMemberDTO> AssignFallbackMembers(
            List<ProjectMemberWithPositionsResponseDTO> projectMembers,
            Dictionary<int, int> memberAssignmentTracker)
        {
            if (!projectMembers.Any()) return new List<TaskMemberDTO>();

            var sortedMembers = projectMembers
                .OrderBy(m => memberAssignmentTracker.GetValueOrDefault(m.AccountId, 0))
                .ThenBy(m => Guid.NewGuid())
                .Take(2)
                .ToList();

            var assignedMembers = sortedMembers
                .Select(m => new TaskMemberDTO
                {
                    AccountId = m.AccountId,
                    Picture = m.Picture,
                    FullName = m.FullName
                })
                .ToList();

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

    public class StoryTaskDTO
    {
        public string ItemId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
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