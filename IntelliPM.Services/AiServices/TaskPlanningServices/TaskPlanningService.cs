using AutoMapper;
using IntelliPM.Data.DTOs.Project.Response;
using IntelliPM.Data.DTOs.Requirement.Request;
using IntelliPM.Data.DTOs.Requirement.Response;
using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.Entities;
using IntelliPM.Services.EpicServices;
using IntelliPM.Services.ProjectServices;
using IntelliPM.Services.RequirementServices;
using IntelliPM.Services.SprintServices;
using IntelliPM.Services.TaskServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.AiServices.TaskPlanningServices
{
    public class TaskPlanningService : ITaskPlanningService
    {
        private readonly IProjectService _projectService;
        private readonly IRequirementService _requirementService;
        private readonly ITaskService _taskService;
        private readonly IEpicService _epicService;
        private readonly ISprintService _sprintService;
        private readonly IMapper _mapper;
        private readonly ILogger<TaskPlanningService> _logger;
        private readonly HttpClient _httpClient;
        private const string _apiKey = "AIzaSyD52tMVJMjE9GxHZwshWwobgQ8bI4rGabA";
        private readonly string _url;

        public TaskPlanningService(
            IProjectService projectService,
            IRequirementService requirementService,
            ITaskService taskService,
            IEpicService epicService,
            ISprintService sprintService,
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
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        
            _url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
        }

        public async Task<List<object>> GenerateTaskPlan(int projectId)
        {
            if (projectId <= 0)
                throw new ArgumentException("Invalid request. ProjectId must be greater than 0.");

            var result = new List<object>();

            var projectResponse = await _projectService.GetProjectById(projectId);
            if (projectResponse == null)
            {
                projectResponse = GenerateFallbackProjectResponse(projectId);
            }

            var projectKey = projectResponse.ProjectKey ?? $"PROJ-{projectId}";
            var startDate = projectResponse.StartDate ?? DateTime.UtcNow;
            var endDate = projectResponse.EndDate ?? startDate.AddMonths(6);

            var requirements = await _requirementService.GetAllRequirements(projectId);
            if (requirements == null || !requirements.Any())
            {
                requirements = _mapper.Map<List<RequirementResponseDTO>>(GenerateFallbackRequirementsResponse(projectId));
            }
            var requirementRequests = _mapper.Map<List<RequirementRequestDTO>>(requirements);

            var epicTasks = await GenerateEpicTasksFromAIResponse(startDate, endDate, projectId, requirementRequests, projectKey);

            foreach (var epic in epicTasks)
            {
                if (string.IsNullOrWhiteSpace(epic.Title))
                {
                    epic.Title = $"Unnamed Epic - {epic.EpicId ?? "Unknown"}";
                }
                if (string.IsNullOrWhiteSpace(epic.Description))
                {
                    epic.Description = $"Default description for {epic.EpicId ?? "Unknown"}";
                }

                var tasks = new List<Tasks>();
                foreach (var task in epic.Tasks)
                {
                    if (string.IsNullOrWhiteSpace(task.Title))
                    {
                        task.Title = $"Unnamed Task - {task.TaskId ?? "Unknown"}";
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
                        ReporterId = 2 // Default ReporterId is 2
                    };
                    var savedTaskResponse = await _taskService.CreateTask(taskRequest);
                    tasks.Add(_mapper.Map<Tasks>(savedTaskResponse));
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
                            SuggestedRole = t.SuggestedRole
                        }).ToList()
                    }
                });
            }

            return result;
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

            var prompt = $@"⚠️ Task titles MUST be 100% unique across all Epics; no repetition or generic terms like 'Task X for...'
Generate a task plan for a project with ID {projectId}. Project Name: '{projectName}'. Project Type: '{projectType}'.
Requirements: {requirementsText}. Duration: {startDateStr} to {endDateStr}.
Create {requirements.Count} Epics based on requirements.
For each Epic, generate a unique 'title' and 'description'.
For each Epic, generate 5 tasks with unique 'title' and 'description' tailored to the Epic.
Include 'startDate' and 'endDate' (YYYY-MM-DD) within the project duration, and 'suggestedRole' (e.g., 'Designer', 'Developer', 'Tester') for each task.
Return ONLY a valid JSON array: [{{""epicId"": ""<id>"", ""title"": ""<title>"", ""description"": ""<desc>"", ""startDate"": ""<date>"", ""endDate"": ""<date>"", ""tasks"": [{{""title"": ""<title>"", ""description"": ""<desc>"", ""startDate"": ""<date>"", ""endDate"": ""<date>"", ""suggestedRole"": ""<role>""}}]}}].";

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
                    aiEpicTasks[i].EpicId = $"{projectKey}-${i + 1}";
                    aiEpicTasks[i].AIGenerated = true;
                    for (int j = 0; j < aiEpicTasks[i].Tasks.Count; j++)
                    {
                        aiEpicTasks[i].Tasks[j].TaskId = $"{projectKey}-${i + 1}-${j + 1}";
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
    }
}