
using IntelliPM.Data.DTOs;
using IntelliPM.Data.DTOs.Ai.SprintTaskPlanning.Request;
using IntelliPM.Data.DTOs.Project.Response;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Services.ProjectServices;
using IntelliPM.Services.SprintServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.AiServices.SprintTaskPlanningServices
{
    public class SprintTaskPlanningService : ISprintTaskPlanningService
    {
        private readonly IProjectService _projectService;
        private readonly ISprintService _sprintService;
        private readonly ILogger<SprintTaskPlanningService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _url;

        public SprintTaskPlanningService(
            IProjectService projectService,
            ISprintService sprintService,
            ILogger<SprintTaskPlanningService> logger,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _sprintService = sprintService ?? throw new ArgumentNullException(nameof(sprintService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = configuration["Gemini:ApiKey"] ?? "AIzaSyD52tMVJMjE9GxHZwshWwobgQ8bI4rGabA";
            _url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
        }

        public async Task<List<AITaskForSprintDTO>> GenerateTasksForSprintAsync(int sprintId, string projectKey)
        {
            if (sprintId <= 0)
                throw new ArgumentException("Sprint ID must be greater than 0.");
            if (string.IsNullOrWhiteSpace(projectKey))
                throw new ArgumentException("Project key is required.");

            // Fetch sprint details
            var sprints = await _sprintService.GetSprintsByProjectKeyWithTasksAsync(projectKey);
            var sprint = sprints.FirstOrDefault(s => s.Id == sprintId);
            if (sprint == null)
                throw new KeyNotFoundException($"Sprint with ID {sprintId} not found for project '{projectKey}'.");

            // Fetch project details
            var project = await _projectService.GetProjectByKey(projectKey);
            if (project == null)
                throw new KeyNotFoundException($"Project with key '{projectKey}' not found.");

            // Generate tasks using AI
            var existingTasks = sprint.Tasks ?? new List<TaskBacklogResponseDTO>();
            var tasksText = string.Join(" | ", existingTasks.Select(t => $"{t.Id}: {t.Title} (Type: {t.Type}, Priority: {t.Priority}, PlannedHours: {t.PlannedHours ?? 0})"));
            var existingTitles = existingTasks.Select(t => t.Title.ToLower()).ToList();

            var prompt = $@"
Generate 5 to 10 additional tasks for a sprint with ID {sprintId} in project '{projectKey}' (Name: '{project.Name}', Type: '{project.ProjectType}').
Sprint Goal: '{sprint.Goal ?? "No goal specified"}'.
Existing Tasks: {(tasksText.Length > 0 ? tasksText : "None")}.
Project Timeline: {project.StartDate?.ToString("yyyy-MM-dd")} to {project.EndDate?.ToString("yyyy-MM-dd")}.
Sprint Timeline: {sprint.StartDate?.ToString("yyyy-MM-dd")} to {sprint.EndDate?.ToString("yyyy-MM-dd")}.
Generate tasks that align with the sprint goal and project context. Ensure task titles are unique (not matching existing titles: {JsonConvert.SerializeObject(existingTitles)}).

Assign task types as follows:
- 'TASK': For small tasks like documentation, testing, or code improvements.
- 'BUG': For software errors or issues.
- 'STORY': For user requirements written as user stories (e.g., 'As a user, I want...').

Each task must include:
- 'title' (unique, for STORY type tasks, do NOT prefix with 'STORY: ', use only the user story, e.g., 'As a user, I want...')
- 'type' (TASK, BUG, or STORY in uppercase)
- 'priority' (HIGHEST, HIGH, MEDIUM, LOW, LOWEST in uppercase)
- 'plannedHours' (1-40)
- optional 'description'

Return ONLY a valid JSON array in this exact format:
[{{""title"":""<title>"",""type"":""<type>"",""priority"":""<priority>"",""plannedHours"":<hours>,""description"":""<desc>""}}]
";


            _logger.LogInformation("Generated prompt: {Prompt}", prompt);

            var requestData = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { maxOutputTokens = 2000, temperature = 0.7 }
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

            _logger.LogInformation("Gemini response: {Response}", responseString);

            var parsedResponse = JsonConvert.DeserializeObject<GeminiResponseDTO>(responseString);
            var replyText = parsedResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text?.Trim();

            if (string.IsNullOrEmpty(replyText))
            {
                _logger.LogError("Gemini did not return any text response.");
                throw new Exception("Gemini did not return any text response.");
            }

            _logger.LogInformation("Parsed reply: {Reply}", replyText);

            replyText = CleanJsonResponse(replyText);

            try
            {
                var settings = new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                };
                var aiTasks = JsonConvert.DeserializeObject<List<AITaskForSprintDTO>>(replyText, settings);
                if (aiTasks == null || !aiTasks.Any())
                {
                    _logger.LogError("No valid tasks from Gemini reply: {ReplyText}", replyText);
                    throw new Exception("No valid tasks from Gemini reply.");
                }

                // Validate and sanitize tasks
                foreach (var task in aiTasks)
                {
                    // Remove 'STORY: ' prefix if present
                    if (task.Type?.ToUpper() == "STORY" && task.Title != null && task.Title.StartsWith("STORY: ", StringComparison.OrdinalIgnoreCase))
                    {
                        task.Title = task.Title.Substring(7).Trim();
                    }

                    // Ensure title is unique
                    if (existingTitles.Contains(task.Title?.ToLower()))
                    {
                        task.Title = $"{task.Title} (AI-Generated {Guid.NewGuid().ToString("N").Substring(0, 4)})";
                    }

                    // Validate priority
                    task.Priority = task.Priority?.ToUpper() switch
                    {
                        "HIGHEST" => "HIGHEST",
                        "HIGH" => "HIGH",
                        "MEDIUM" => "MEDIUM",
                        "LOW" => "LOW",
                        "LOWEST" => "LOWEST",
                        _ => "MEDIUM" // Default to MEDIUM if invalid
                    };

                    // Validate planned hours
                    task.PlannedHours = task.PlannedHours > 0 && task.PlannedHours <= 40 ? task.PlannedHours : 8;

                    // Validate type
                    task.Type = task.Type?.ToUpper() switch
                    {
                        "TASK" => "TASK",
                        "BUG" => "BUG",
                        "STORY" => "STORY",
                        _ => "TASK" // Default to TASK if invalid
                    };
                }

                // Ensure task count is between 5 and 10
                if (aiTasks.Count > 10)
                {
                    aiTasks = aiTasks.Take(10).ToList();
                }
                else if (aiTasks.Count < 5)
                {
                    for (int i = aiTasks.Count; i < 5; i++)
                    {
                        aiTasks.Add(new AITaskForSprintDTO
                        {
                            Title = $"Additional Task {i + 1} (AI-Generated)",
                            Type = "TASK",
                            Priority = "MEDIUM",
                            PlannedHours = 8,
                            Description = "Fallback task to meet minimum count"
                        });
                    }
                }

                return aiTasks;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deserializing tasks from Gemini reply: {ReplyText}\n{ErrorMessage}", replyText, ex.Message);
                throw new Exception("Error deserializing tasks from Gemini reply.", ex);
            }
        }

        private string CleanJsonResponse(string rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson)) return JsonConvert.SerializeObject(new List<AITaskForSprintDTO>());
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
                _logger.LogWarning("Invalid JSON response, returning empty list: {RawJson}", rawJson);
                return JsonConvert.SerializeObject(new List<AITaskForSprintDTO>());
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
}
