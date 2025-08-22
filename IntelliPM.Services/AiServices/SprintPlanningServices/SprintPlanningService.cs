using AutoMapper;
using IntelliPM.Data.DTOs.Project.Response;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Repositories.SprintRepos;
using IntelliPM.Services.ProjectServices;
using IntelliPM.Services.SprintServices;
using IntelliPM.Services.TaskServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.AiServices.SprintPlanningServices
{
    public class SprintPlanningService : ISprintPlanningService
    {
        private readonly IProjectService _projectService;
        private readonly ITaskService _taskService;
        private readonly ISprintService _sprintService;
        private readonly ISprintRepository _sprintRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<SprintPlanningService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _url;

        public SprintPlanningService(
            IProjectService projectService,
            ITaskService taskService,
            ISprintService sprintService,
            IMapper mapper,
            ILogger<SprintPlanningService> logger,
            IConfiguration configuration,
            ISprintRepository sprintRepo,
            HttpClient httpClient)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _sprintService = sprintService ?? throw new ArgumentNullException(nameof(sprintService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = configuration["GeminiApiDAT:ApiKey"];
            _url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
            _sprintRepo = sprintRepo ?? throw new ArgumentNullException(nameof(sprintRepo));
        }

        public async Task<List<SprintWithTasksDTO>> GenerateSprintPlan(int projectId, int numberOfSprints, int weeksPerSprint)
        {
            if (projectId <= 0)
                throw new ArgumentException("Invalid request. ProjectId must be greater than 0.");
            if (numberOfSprints <= 0)
                throw new ArgumentException("Invalid request. NumberOfSprints must be greater than 0.");
            if (weeksPerSprint <= 0)
                throw new ArgumentException("Invalid request. WeeksPerSprint must be greater than 0.");

            // Retrieve project details
            var projectResponse = await _projectService.GetProjectById(projectId);
            if (projectResponse == null)
            {
                projectResponse = GenerateFallbackProjectResponse(projectId);
            }

            var projectKey = projectResponse.ProjectKey ?? $"PROJ-{projectId}";
            var startDate = projectResponse.StartDate?.ToUniversalTime() ?? DateTime.UtcNow;
            var endDate = projectResponse.EndDate?.ToUniversalTime() ?? startDate.AddMonths(6);

            // Validate sprint duration within project timeline
            var totalWeeks = numberOfSprints * weeksPerSprint;
            var projectDurationDays = (endDate - startDate).TotalDays;
            if (totalWeeks * 7 > projectDurationDays)
            {
                _logger.LogWarning("Requested sprint plan duration ({TotalWeeks} weeks) exceeds project duration ({ProjectDays} days).", totalWeeks, projectDurationDays);
                throw new ArgumentException("Requested sprint plan duration exceeds project duration.");
            }

            // Retrieve backlog tasks
            var backlogTasks = await _taskService.GetBacklogTasksAsync(projectKey);
            if (backlogTasks == null || !backlogTasks.Any())
            {
                _logger.LogWarning("No backlog tasks found for Project ID {ProjectId}", projectId);
                return new List<SprintWithTasksDTO>();
            }

            // Generate sprint plan using AI
            var sprintPlan = await GenerateSprintPlanFromAIResponse(projectId, projectResponse, backlogTasks, numberOfSprints, weeksPerSprint);

            // Assign sprint IDs and map tasks
            for (int i = 0; i < sprintPlan.Count; i++)
            {
                sprintPlan[i].SprintId = $"{projectKey}-SPRINT-{i + 1}";
                sprintPlan[i].AIGenerated = true;
                foreach (var task in sprintPlan[i].Tasks)
                {
                    task.TaskId = task.TaskId ?? $"TASK-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                }
            }

            return sprintPlan;
        }

        private async Task<List<SprintWithTasksDTO>> GenerateSprintPlanFromAIResponse(int projectId, ProjectResponseDTO project, List<TaskBacklogResponseDTO> backlogTasks, int numberOfSprints, int weeksPerSprint)
        {
            var projectName = project.Name;
            var projectType = project.ProjectType;
            var startDateStr = project.StartDate?.ToUniversalTime().ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
            var endDateStr = project.EndDate?.ToUniversalTime().ToString("yyyy-MM-dd") ?? DateTime.UtcNow.AddMonths(6).ToString("yyyy-MM-dd");

            // Get existing sprints to avoid date overlap
            var existingSprints = await _sprintRepo.GetByProjectIdAsync(projectId);
            var occupiedDateRanges = existingSprints
                .Where(s => s.StartDate.HasValue && s.EndDate.HasValue)
                .Select(s => new DateRange
                {
                    Start = s.StartDate.Value.ToUniversalTime(),
                    End = s.EndDate.Value.ToUniversalTime()
                })
                .OrderBy(s => s.Start)
                .ToList();

            var tasksText = string.Join(" | ", backlogTasks.Select(t => $"{t.Id}: {t.Title} (Priority: {t.Priority}, PlannedHours: {t.PlannedHours ?? 0})"));

            // Updated prompt to enforce Priority and PlannedHours
            var prompt = $@"Generate a sprint plan for a project with ID {projectId}. Project Name: '{projectName}'. Project Type: '{projectType}'.
Backlog Tasks: {tasksText}. Project Duration: {startDateStr} to {endDateStr}.
Group the tasks into exactly {numberOfSprints} sprints, each lasting exactly {weeksPerSprint} weeks.
For each sprint, generate a unique 'title' and 'description', and assign tasks from the backlog based on priority and logical grouping.
Include 'startDate' and 'endDate' (YYYY-MM-DD) strictly within the project duration ({startDateStr} to {endDateStr}) for each sprint. Ensure sprints are sequential (each starts after the previous ends) and {(occupiedDateRanges.Any() ? $"do not overlap with these existing sprint date ranges: {JsonConvert.SerializeObject(occupiedDateRanges)}" : "no existing sprints exist, so assign dates starting from the project start date")}.
For each task, include its 'taskId', 'title', 'priority', and 'plannedHours'. **MANDATORY**: Every task MUST have a non-null 'priority' (valid values: 'HIGH', 'MEDIUM', 'LOW') and a non-null 'plannedHours' (a positive decimal number). Do not omit these fields or provide null/empty values. Use the provided backlog task priority and plannedHours where applicable.
Return ONLY a valid JSON array: [{{""title"": ""<title>"", ""description"": ""<desc>"", ""startDate"": ""<date>"", ""endDate"": ""<date>"", ""tasks"": [{{""taskId"": ""<id>"", ""title"": ""<title>"", ""priority"": ""<priority>"", ""plannedHours"": <hours>}}]}}].";

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
                var settings = new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                };
                var aiSprintPlan = JsonConvert.DeserializeObject<List<SprintWithTasksDTO>>(replyText, settings);
                if (aiSprintPlan == null || !aiSprintPlan.Any())
                {
                    _logger.LogError("No valid sprint plan from Gemini reply: {ReplyText}", replyText);
                    throw new Exception("No valid sprint plan from Gemini reply.");
                }

                // Validate and sanitize AI-generated data
                var projectStart = project.StartDate?.ToUniversalTime() ?? DateTime.UtcNow;
                var projectEnd = project.EndDate?.ToUniversalTime() ?? projectStart.AddMonths(6);
                foreach (var sprint in aiSprintPlan)
                {
                    // Validate tasks and ensure Priority and PlannedHours
                    if (sprint.Tasks == null || !sprint.Tasks.Any())
                    {
                        _logger.LogWarning("Sprint '{SprintTitle}' has no tasks.", sprint.Title);
                        sprint.Tasks = new List<SprintTaskDTO>();
                        continue;
                    }

                    // Filter tasks to match backlog and validate fields
                    sprint.Tasks = sprint.Tasks
                        .Where(t => backlogTasks.Select(bt => bt.Id).Contains(t.TaskId))
                        .Select(t =>
                        {
                            var backlogTask = backlogTasks.FirstOrDefault(bt => bt.Id == t.TaskId);
                            if (string.IsNullOrEmpty(t.Priority) || !new[] { "HIGH", "MEDIUM", "LOW" }.Contains(t.Priority.ToUpper()))
                            {
                                _logger.LogWarning("Task '{TaskId}' in sprint '{SprintTitle}' has invalid or missing Priority. Using backlog or default 'MEDIUM'.", t.TaskId, sprint.Title);
                                t.Priority = backlogTask?.Priority?.ToUpper() ?? "MEDIUM";
                            }
                            if (t.PlannedHours <= 0)
                            {
                                _logger.LogWarning("Task '{TaskId}' in sprint '{SprintTitle}' has invalid or missing PlannedHours. Using backlog or default 8.", t.TaskId, sprint.Title);
                                t.PlannedHours = backlogTask?.PlannedHours ?? 8;
                            }
                            return t;
                        })
                        .ToList();

                    // Ensure UTC for sprint dates
                    sprint.StartDate = sprint.StartDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(sprint.StartDate, DateTimeKind.Utc) : sprint.StartDate.ToUniversalTime();
                    sprint.EndDate = sprint.EndDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(sprint.EndDate, DateTimeKind.Utc) : sprint.EndDate.ToUniversalTime();

                    // Sanitize dates to project timeline
                    if (sprint.StartDate < projectStart)
                    {
                        sprint.StartDate = projectStart;
                        sprint.EndDate = projectStart.AddDays(weeksPerSprint * 7);
                    }
                    if (sprint.EndDate > projectEnd)
                    {
                        sprint.EndDate = projectEnd;
                        sprint.StartDate = projectEnd.AddDays(-weeksPerSprint * 7);
                        if (sprint.StartDate < projectStart)
                        {
                            sprint.StartDate = projectStart;
                            sprint.EndDate = projectStart.AddDays(weeksPerSprint * 7);
                        }
                    }
                }

                // Adjust sprint dates
                var adjustedPlan = await AdjustSprintDates(project.ProjectKey, aiSprintPlan, numberOfSprints, weeksPerSprint, project, occupiedDateRanges);
                if (adjustedPlan.Count != numberOfSprints)
                {
                    _logger.LogWarning("Adjusted plan has {ActualCount} sprints, but {ExpectedCount} were requested.", adjustedPlan.Count, numberOfSprints);
                    return await AdjustSprintCount(adjustedPlan, numberOfSprints, weeksPerSprint, project, backlogTasks);
                }

                return adjustedPlan.Where(s => s.Tasks.Any()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deserializing sprint plan from Gemini reply: {ReplyText}\n{ErrorMessage}", replyText, ex.Message);
                throw new Exception("Error deserializing sprint plan from Gemini reply.", ex);
            }
        }

        private async Task<List<SprintWithTasksDTO>> AdjustSprintDates(string projectKey, List<SprintWithTasksDTO> sprintPlan, int numberOfSprints, int weeksPerSprint, ProjectResponseDTO project, List<DateRange> occupiedDateRanges)
        {
            var adjustedPlan = new List<SprintWithTasksDTO>();
            var startDate = project.StartDate?.ToUniversalTime() ?? DateTime.UtcNow;
            var endDate = project.EndDate?.ToUniversalTime() ?? startDate.AddMonths(6);

            // If no existing sprints, start from project start date
            var latestEndDate = occupiedDateRanges.Any() ? occupiedDateRanges.Max(r => r.End) : startDate;

            // Sort sprints by start date
            var sortedSprints = sprintPlan.OrderBy(s => s.StartDate).ToList();

            foreach (var sprint in sortedSprints)
            {
                // Ensure sprint dates are within project timeline
                if (sprint.StartDate < startDate || sprint.EndDate > endDate)
                {
                    _logger.LogWarning("Sprint '{SprintTitle}' has invalid dates ({StartDate} to {EndDate}) outside project timeline ({ProjectStart} to {ProjectEnd}). Adjusting to valid range.",
                        sprint.Title, sprint.StartDate, sprint.EndDate, startDate, endDate);
                    sprint.StartDate = latestEndDate;
                    sprint.EndDate = latestEndDate.AddDays(weeksPerSprint * 7);
                    if (sprint.EndDate > endDate)
                    {
                        _logger.LogWarning("Sprint '{SprintTitle}' exceeds project end date after adjustment.", sprint.Title);
                        continue;
                    }
                }

                // If no existing sprints, use adjusted dates
                if (!occupiedDateRanges.Any())
                {
                    adjustedPlan.Add(sprint);
                    occupiedDateRanges.Add(new DateRange { Start = sprint.StartDate, End = sprint.EndDate });
                    latestEndDate = sprint.EndDate;
                    continue;
                }

                // Otherwise, validate and adjust dates
                var (isValid, message) = await _sprintService.CheckSprintDatesAsync(projectKey, sprint.StartDate, sprint.EndDate);
                if (!isValid)
                {
                    _logger.LogWarning("Sprint '{SprintTitle}' failed validation: {Message}. Adjusting dates.", sprint.Title, message);
                    var newStartDate = await FindNextValidStartDate(projectKey, latestEndDate.AddDays(1), weeksPerSprint, occupiedDateRanges, startDate, endDate);
                    var newEndDate = newStartDate.AddDays(weeksPerSprint * 7);
                    if (newEndDate > endDate)
                    {
                        _logger.LogWarning("Adjusted sprint '{SprintTitle}' exceeds project end date.", sprint.Title);
                        continue;
                    }
                    sprint.StartDate = newStartDate;
                    sprint.EndDate = newEndDate;
                    occupiedDateRanges.Add(new DateRange { Start = newStartDate, End = newEndDate });
                    latestEndDate = newEndDate;
                }
                else
                {
                    occupiedDateRanges.Add(new DateRange { Start = sprint.StartDate, End = sprint.EndDate });
                    latestEndDate = sprint.EndDate;
                }
                adjustedPlan.Add(sprint);
            }

            return adjustedPlan;
        }

        private async Task<List<SprintWithTasksDTO>> AdjustSprintCount(List<SprintWithTasksDTO> sprintPlan, int numberOfSprints, int weeksPerSprint, ProjectResponseDTO project, List<TaskBacklogResponseDTO> backlogTasks)
        {
            var adjustedPlan = new List<SprintWithTasksDTO>();
            var startDate = project.StartDate?.ToUniversalTime() ?? DateTime.UtcNow;
            var endDate = project.EndDate?.ToUniversalTime() ?? startDate.AddMonths(6);
            var projectKey = project.ProjectKey ?? $"PROJ-{project.Id}";
            var existingSprints = await _sprintRepo.GetByProjectIdAsync(project.Id);

            // Get occupied date ranges
            var occupiedDateRanges = existingSprints
                .Where(s => s.StartDate.HasValue && s.EndDate.HasValue)
                .Select(s => new DateRange
                {
                    Start = s.StartDate.Value.ToUniversalTime(),
                    End = s.EndDate.Value.ToUniversalTime()
                })
                .OrderBy(s => s.Start)
                .ToList();

            // If no existing sprints, use project start date
            var latestEndDate = occupiedDateRanges.Any() ? occupiedDateRanges.Max(r => r.End) : startDate;

            if (sprintPlan.Count > numberOfSprints)
            {
                // Take only the first numberOfSprints
                adjustedPlan = sprintPlan.Take(numberOfSprints).ToList();
            }
            else if (sprintPlan.Count < numberOfSprints)
            {
                // Add missing sprints with valid dates
                adjustedPlan.AddRange(sprintPlan);
                for (int i = sprintPlan.Count; i < numberOfSprints; i++)
                {
                    var sprintStart = await FindNextValidStartDate(projectKey, latestEndDate.AddDays(1), weeksPerSprint, occupiedDateRanges, startDate, endDate);
                    var sprintEnd = sprintStart.AddDays(weeksPerSprint * 7);
                    if (sprintEnd > endDate)
                    {
                        _logger.LogWarning("Cannot add sprint {SprintNumber} as it exceeds project end date.", i + 1);
                        continue;
                    }
                    adjustedPlan.Add(new SprintWithTasksDTO
                    {
                        Title = $"Sprint {i + 1}",
                        Description = $"Generated sprint {i + 1} to meet requested count",
                        StartDate = sprintStart,
                        EndDate = sprintEnd,
                        AIGenerated = true,
                        Tasks = new List<SprintTaskDTO>()
                    });
                    occupiedDateRanges.Add(new DateRange { Start = sprintStart, End = sprintEnd });
                    latestEndDate = sprintEnd;
                }
            }
            else
            {
                adjustedPlan = sprintPlan;
            }

            // Redistribute tasks if needed
            var allTasks = adjustedPlan.SelectMany(s => s.Tasks).ToList();
            var unassignedTasks = backlogTasks.Where(t => !allTasks.Any(st => st.TaskId == t.Id)).ToList();
            if (unassignedTasks.Any())
            {
                var tasksPerSprint = (int)Math.Ceiling((double)unassignedTasks.Count / adjustedPlan.Count);
                for (int i = 0; i < adjustedPlan.Count && unassignedTasks.Any(); i++)
                {
                    var tasksToAdd = unassignedTasks.Take(tasksPerSprint).ToList();
                    adjustedPlan[i].Tasks.AddRange(tasksToAdd.Select(t => new SprintTaskDTO
                    {
                        TaskId = t.Id,
                        Title = t.Title,
                        Priority = t.Priority?.ToUpper() ?? "MEDIUM",
                        PlannedHours = t.PlannedHours ?? 8
                    }));
                    unassignedTasks.RemoveRange(0, Math.Min(tasksPerSprint, unassignedTasks.Count));
                }
            }

            return adjustedPlan.Where(s => s.Tasks.Any()).ToList();
        }

        private async Task<DateTime> FindNextValidStartDate(string projectKey, DateTime proposedStart, int weeksPerSprint, List<DateRange> occupiedDateRanges, DateTime projectStart, DateTime projectEnd)
        {
            var maxAttempts = 10; // Prevent infinite loops
            var currentStart = proposedStart.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(proposedStart, DateTimeKind.Utc) : proposedStart.ToUniversalTime();

            // Ensure start date is not before project start
            if (currentStart < projectStart)
            {
                currentStart = projectStart;
            }

            // If no existing sprints, ensure within project timeline
            if (!occupiedDateRanges.Any())
            {
                if (currentStart.AddDays(weeksPerSprint * 7) <= projectEnd)
                {
                    return currentStart;
                }
                _logger.LogWarning("Proposed start date {StartDate} for sprint exceeds project end date {EndDate}.", currentStart, projectEnd);
                throw new Exception("Unable to find a valid start date within the project timeline.");
            }

            for (int i = 0; i < maxAttempts; i++)
            {
                var proposedEnd = currentStart.AddDays(weeksPerSprint * 7);
                if (proposedEnd > projectEnd)
                {
                    _logger.LogWarning("Proposed sprint end date {EndDate} exceeds project end date {ProjectEnd}.", proposedEnd, projectEnd);
                    break;
                }
                var (isValid, message) = await _sprintService.CheckSprintDatesAsync(projectKey, currentStart, proposedEnd);
                if (isValid)
                {
                    return currentStart;
                }
                currentStart = currentStart.AddDays(1); // Try next day
                if (currentStart < projectStart)
                {
                    currentStart = projectStart;
                }
            }

            _logger.LogWarning("Could not find valid start date after {MaxAttempts} attempts.", maxAttempts);
            throw new Exception("Unable to find a valid start date for the sprint within the project timeline.");
        }

        private string CleanJsonResponse(string rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson)) return JsonConvert.SerializeObject(GenerateFallbackSprintPlan());
            var cleaned = rawJson.Trim();
            if (cleaned.StartsWith("```json")) cleaned = cleaned.Substring(7).Trim();
            if (cleaned.EndsWith("```")) cleaned = cleaned.Substring(0, cleaned.Length - 3).Trim();
            if (!cleaned.StartsWith("[")) cleaned = "[" + cleaned;
            if (!cleaned.EndsWith("]")) cleaned += "]";
            try
            {
                var parsed = JsonConvert.DeserializeObject<List<SprintWithTasksDTO>>(cleaned);
                // Validate that all tasks have Priority and PlannedHours
                foreach (var sprint in parsed)
                {
                    if (sprint.Tasks != null)
                    {
                        foreach (var task in sprint.Tasks)
                        {
                            if (string.IsNullOrEmpty(task.Priority) || !new[] { "HIGH", "MEDIUM", "LOW" }.Contains(task.Priority.ToUpper()))
                            {
                                _logger.LogWarning("Task '{TaskId}' in sprint '{SprintTitle}' missing or invalid Priority in JSON response. Assigned default 'MEDIUM'.", task.TaskId, sprint.Title);
                                task.Priority = "MEDIUM";
                            }
                            if (task.PlannedHours <= 0)
                            {
                                _logger.LogWarning("Task '{TaskId}' in sprint '{SprintTitle}' missing or invalid PlannedHours in JSON response. Assigned default 8.", task.TaskId, sprint.Title);
                                task.PlannedHours = 8;
                            }
                        }
                    }
                }
                return JsonConvert.SerializeObject(parsed);
            }
            catch
            {
                _logger.LogWarning("Invalid JSON response, returning fallback sprint plan: {RawJson}", rawJson);
                return JsonConvert.SerializeObject(GenerateFallbackSprintPlan());
            }
        }

        private List<SprintWithTasksDTO> GenerateFallbackSprintPlan()
        {
            return new List<SprintWithTasksDTO>
            {
                new SprintWithTasksDTO
                {
                    Title = "Fallback Sprint",
                    Description = "Generated due to API failure",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(14),
                    AIGenerated = true,
                    Tasks = new List<SprintTaskDTO>
                    {
                        new SprintTaskDTO
                        {
                            TaskId = "FALLBACK-TASK-1",
                            Title = "Fallback Task",
                            Priority = "MEDIUM",
                            PlannedHours = 8
                        }
                    }
                }
            };
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

        private class DateRange
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }
    }

    public class SprintWithTasksDTO
    {
        public string SprintId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool AIGenerated { get; set; }
        public List<SprintTaskDTO> Tasks { get; set; }
    }

    public class SprintTaskDTO
    {
        public string TaskId { get; set; }
        public string Title { get; set; }
        public string Priority { get; set; }
        public decimal PlannedHours { get; set; }
    }
}