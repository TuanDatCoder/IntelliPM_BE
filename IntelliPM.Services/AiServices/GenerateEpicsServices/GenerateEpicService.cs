using IntelliPM.Services.ProjectServices;
using IntelliPM.Services.RequirementServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.AiServices.GenerateEpicsServices
{
    public class GenerateEpicService : IGenerateEpicService
    {
        private readonly IProjectService _projectService;
        private readonly IRequirementService _requirementService;
        private readonly ILogger<GenerateEpicService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _url;

        public GenerateEpicService(
            IProjectService projectService,
            IRequirementService requirementService,
            ILogger<GenerateEpicService> logger,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _requirementService = requirementService ?? throw new ArgumentNullException(nameof(requirementService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            var apiKey = configuration["Gemini:ApiKey"] ?? "AIzaSyD52tMVJMjE9GxHZwshWwobgQ8bI4rGabA";
            _url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";
        }

        public async Task<List<EpicPreviewDTO>> GenerateEpics(int projectId, List<string> existingEpicTitles)
        {
            if (projectId <= 0)
                throw new ArgumentException("Invalid request. ProjectId must be greater than 0.");

            // Retrieve project details
            var projectResponse = await _projectService.GetProjectById(projectId);
            if (projectResponse == null)
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");

            var projectKey = projectResponse.ProjectKey ?? $"PROJ-{projectId}";
            var startDate = projectResponse.StartDate ?? DateTime.UtcNow;
            var endDate = projectResponse.EndDate ?? startDate.AddMonths(6);

            // Retrieve requirements
            var requirements = await _requirementService.GetAllRequirements(projectId);
            if (requirements == null || !requirements.Any())
                throw new KeyNotFoundException($"No requirements found for project ID {projectId}.");

            var requirementsText = string.Join(" | ", requirements.Select(r => $"{r.Title}: {r.Description}"));

            var existingEpicsText = string.Join(", ", existingEpicTitles);

            var prompt = $@"Generate 5 new unique epics for project ID {projectId}.

PROJECT DETAILS:
- Name: '{projectResponse.Name}'
- Type: '{projectResponse.ProjectType}'
- Requirements: {requirementsText}
- Duration: {startDate.ToString("yyyy-MM-DD")} to {endDate.ToString("yyyy-MM-DD")}

EXISTING EPICS (AVOID THESE TITLES): {existingEpicsText}

INSTRUCTIONS:
1. Create 5 new Epics based on the requirements
2. Ensure each Epic title is unique and different from existing epics
3. For each Epic, generate a unique 'title' (under 100 chars) and 'description'
4. For each Epic, set 'startDate' and 'endDate' (YYYY-MM-DD format) within project duration

IMPORTANT: Return ONLY a valid JSON array with this exact structure:
[
  {{
    ""title"": ""<epic_title>"",
    ""description"": ""<epic_description>"",
    ""startDate"": ""<YYYY-MM-DD>"",
    ""endDate"": ""<YYYY-MM-DD>""
  }}
]";

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
                var aiEpics = JsonConvert.DeserializeObject<List<EpicPreviewDTO>>(replyText);
                if (aiEpics == null || !aiEpics.Any())
                {
                    _logger.LogError("No valid epics from Gemini reply: {ReplyText}", replyText);
                    throw new Exception("No valid epics from Gemini reply.");
                }

                // Validate và clean epics
                foreach (var epic in aiEpics)
                {
                    epic.AIGenerated = true;
                    if (string.IsNullOrWhiteSpace(epic.Title))
                    {
                        epic.Title = $"Unnamed Epic - {aiEpics.IndexOf(epic) + 1}";
                    }
                    if (string.IsNullOrWhiteSpace(epic.Description))
                    {
                        epic.Description = $"Default description for epic {epic.Title}";
                    }
                    if (epic.StartDate < startDate || epic.EndDate > endDate)
                    {
                        epic.StartDate = startDate;
                        epic.EndDate = endDate;
                    }
                }

                return aiEpics;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deserializing epics from Gemini reply: {ReplyText}\n{ErrorMessage}", replyText, ex.Message);
                throw new Exception("Error deserializing epics from Gemini reply.", ex);
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

    public class EpicPreviewDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool AIGenerated { get; set; } = true;
    }

}
