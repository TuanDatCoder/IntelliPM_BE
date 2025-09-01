using AutoMapper;
using IntelliPM.Data.DTOs.Epic.Request;
using IntelliPM.Data.DTOs.ProjectMetric.Request;
using IntelliPM.Data.DTOs.ProjectRecommendation.Response;
using IntelliPM.Data.DTOs.Risk.Request;
using IntelliPM.Data.DTOs.Risk.Response;
using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.Entities;
using IntelliPM.Data.Enum.ProjectRecommendation;
using IntelliPM.Repositories.DynamicCategoryRepos;
using IntelliPM.Repositories.SystemConfigurationRepos;
using IntelliPM.Services.GeminiServices;
using Newtonsoft.Json;
using System.Text;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private const string _apiKey = "AIzaSyDWOB57CuXTor0WgXFCZ4cLOY6QnVtqJkc";
    private readonly string _url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
    private readonly IMapper _mapper;
    private readonly IDynamicCategoryRepository _dynamicCategoryRepo;
    private readonly ISystemConfigurationRepository _systemConfigRepo;

    public GeminiService(HttpClient httpClient, IMapper mapper, IDynamicCategoryRepository dynamicCategoryRepo, ISystemConfigurationRepository systemConfigRepo)
    {
        _httpClient = httpClient;
        _mapper = mapper;
        _dynamicCategoryRepo = dynamicCategoryRepo;
        _systemConfigRepo = systemConfigRepo;
    }

    public async Task<List<string>> GenerateSubtaskAsync(string taskTitle)
    {
        var prompt = @$"
Generate 5–7 clear and unique subtasks needed to accomplish the task titled: ""{taskTitle}"" 
in the context of web or software development.

Each subtask title must:
- Be under 80 characters.
- Be specific and actionable.
- Avoid duplicates.

Return the result strictly as a JSON array in the following format (no explanations, no extra text):

[
  {{
    ""title"": ""Subtask title 1"",
    ""status"": ""TO-DO"",
    ""manualInput"": false,
    ""generationAiInput"": true
  }},
  {{
    ""title"": ""Subtask title 2"",
    ""status"": ""TO-DO"",
    ""manualInput"": false,
    ""generationAiInput"": true
  }}
]

Return only valid JSON.";

        var requestData = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var requestJson = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Gemini API Error: {response.StatusCode}\nResponse: {responseString}");
        }

        if (string.IsNullOrWhiteSpace(responseString))
        {
            throw new Exception("Gemini response is empty.");
        }

        var parsedResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseString);
        var replyText = parsedResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text?.Trim();

        if (string.IsNullOrEmpty(replyText))
        {
            throw new Exception("Gemini did not return any text response.");
        }

        replyText = replyText.Trim();

        if (replyText.StartsWith("```"))
        {
            replyText = replyText.Replace("```json", "")
                                 .Replace("```", "")
                                 .Replace("json", "")
                                 .Trim();
        }

        if (!replyText.StartsWith("["))
        {
            throw new Exception("Gemini reply is not a JSON array:\n" + replyText);
        }

        try
        {
            var checklistItems = JsonConvert.DeserializeObject<List<GeminiChecklistItem>>(replyText);
            return checklistItems.Select(x => x.title).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception("Error deserializing checklist items from Gemini reply:\n" + replyText + "\n" + ex.Message);
        }
    }

    public async Task<List<TaskSuggestionRequestDTO>> GenerateTaskAsync(string projectDescription)
    {
        var prompt = @$"You are given the following project description:

""{projectDescription}""

Based on this, generate **exactly 10 to 12 distinct and actionable tasks** required to successfully implement the project. The tasks should follow an alternating pattern of types (e.g., STORY, TASK, TASK, TASK, STORY, TASK, TASK, TASK, etc.) and include **exactly 2 BUGs** within the list.

Each task must include:
- A clear and realistic **title** (limited to 80 characters).
- A concise but informative **description** that explains what the task involves.
- A valid **type**, which must be one of:
  - ""BUG"": For fixing a software/system issue or malfunction (exactly 2 BUGs).
  - ""STORY"": A feature or functionality from the user's perspective, written as ""As a [user role], I want [goal] so that [benefit].""
  - ""TASK"": Technical, administrative, or other necessary work supporting a feature or bug fix.

### Output Format:
Return a valid **JSON array** only (no markdown, no explanation). Use the exact format below for each task:

[
  {{
    ""title"": ""Task title 1"",
    ""description"": ""A short, specific description for task 1."",
    ""status"": ""TO_DO"",
    ""manualInput"": false,
    ""generationAiInput"": true,
    ""type"": ""STORY""
  }},
  ...
]

### Guidelines:
- Follow an alternating pattern (e.g., STORY, TASK, TASK, TASK, STORY, TASK, TASK, TASK, etc.) with exactly 2 BUGs included.
- Ensure **exactly 10 to 12 tasks** in total.
- Each task must be **unique** — avoid redundancy or overlapping responsibilities.
- STORY tasks must follow the format: ""As a [user role], I want [goal] so that [benefit].""
- TASK tasks should support STORY tasks (e.g., for a registration story, tasks like ""Create registration form"" or ""Save user data to database"").
- BUG tasks should represent realistic issues (e.g., fixing incorrect form validation or database errors).
- Tasks must be **clear, practical, and achievable**, reflecting real-world development steps.
- Titles must not exceed 80 characters.
- Do **not** include markdown formatting, comments, or any explanations — only return raw JSON.
- Ensure the final output is a valid, parsable JSON array starting with '[' and ending with ']'.";

        var requestData = new
        {
            contents = new[]
            {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
        };

        var requestJson = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Gemini API Error: {response.StatusCode}\nResponse: {responseString}");
        }

        if (string.IsNullOrWhiteSpace(responseString))
        {
            throw new Exception("Gemini response is empty.");
        }

        var parsedResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseString);
        var replyText = parsedResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text?.Trim();

        if (string.IsNullOrEmpty(replyText))
        {
            throw new Exception("Gemini did not return any text response.");
        }

        // Remove ``` if present
        if (replyText.StartsWith("```"))
        {
            replyText = replyText.Replace("```json", "")
                                 .Replace("```", "")
                                 .Replace("json", "")
                                 .Trim();
        }

        if (!replyText.StartsWith("["))
        {
            throw new Exception("Gemini reply is not a JSON array:\n" + replyText);
        }

        try
        {
            var checklistItems = JsonConvert.DeserializeObject<List<TaskSuggestionRequestDTO>>(replyText);
            return checklistItems;
        }
        catch (Exception ex)
        {
            throw new Exception("Error deserializing checklist items from Gemini reply:\n" + replyText + "\n" + ex.Message);
        }
    }

    public async Task<List<EpicSuggestionRequestDTO>> GenerateEpicAsync(string projectDescription)
    {
        var prompt = @$"You are given the following project description:

""{projectDescription}""

Based on this, generate **exactly 5 to 7 distinct and actionable epics** that represent high-level features or major components required to successfully implement the project. An epic is a large issue that can be broken down into smaller user stories or tasks, representing a significant feature or part of the project.

Each epic must include:
- A realistic and concise **name** (limited to 80 characters).
- A concise but informative **description** that explains what the epic involves.

### Output Format:
Return a valid **JSON array** only (no markdown, no explanation). Use the exact format below for each epic:

[
  {{
    ""name"": ""Epic name 1"",
    ""description"": ""A short, specific description for epic 1."",
    ""status"": ""TO_DO"",
  }},
  ...
]

### Guidelines:
- Each epic must be **unique** — avoid redundancy or overlapping responsibilities.
- Epics must be **clear, practical, and achievable**, reflecting high-level features or major project components.
- Do **not** include markdown formatting, comments, or any explanations — only return raw JSON.
- Make sure the final output is a valid, parsable JSON array starting with '[' and ending with ']'.";

        var requestData = new
        {
            contents = new[]
            {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
        };

        var requestJson = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Gemini API Error: {response.StatusCode}\nResponse: {responseString}");
        }

        if (string.IsNullOrWhiteSpace(responseString))
        {
            throw new Exception("Gemini response is empty.");
        }

        var parsedResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseString);
        var replyText = parsedResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text?.Trim();

        if (string.IsNullOrEmpty(replyText))
        {
            throw new Exception("Gemini did not return any text response.");
        }

        // Remove ``` if present
        if (replyText.StartsWith("```"))
        {
            replyText = replyText.Replace("```json", "")
                                 .Replace("```", "")
                                 .Replace("json", "")
                                 .Trim();
        }

        if (!replyText.StartsWith("["))
        {
            throw new Exception("Gemini reply is not a JSON array:\n" + replyText);
        }

        try
        {
            var epicItems = JsonConvert.DeserializeObject<List<EpicSuggestionRequestDTO>>(replyText);
            return epicItems;
        }
        catch (Exception ex)
        {
            throw new Exception("Error deserializing epic items from Gemini reply:\n" + replyText + "\n" + ex.Message);
        }
    }

    public async Task<List<TaskSuggestionRequestDTO>> GenerateTaskByEpicAsync(string epicDescription)
    {
        var prompt = @$"Given the following epic description:

""{epicDescription}""

Generate **exactly 10 to 12 distinct and actionable tasks** needed to implement the epic. The tasks should follow an alternating pattern of types (e.g., STORY, TASK, TASK, TASK, STORY, TASK, TASK, TASK, etc.) and include **exactly 2 BUGs** within the list.

Each task must include:
- A realistic and concise **title** (limited to 80 characters).
- A clear and actionable **description** that explains what the task involves.
- A valid **type** from the list below:
  - ""BUG"": For fixing a software/system issue or malfunction (exactly 2 BUGs).
  - ""STORY"": A feature or functionality from the user's perspective, written as ""As a [user role], I want [goal] so that [benefit].""
  - ""TASK"": Technical, administrative, or other necessary work supporting a feature or bug fix.

### Output Format:
Return a valid **JSON array** only (no markdown, no explanation). Use the exact format below for each task:

[
  {{
    ""title"": ""Task title 1"",
    ""description"": ""A short, specific description for task 1."",
    ""status"": ""TO_DO"",
    ""manualInput"": false,
    ""generationAiInput"": true,
    ""type"": ""STORY""
  }},
  ...
]

### Guidelines:
- Follow an alternating pattern (e.g., STORY, TASK, TASK, TASK, STORY, TASK, TASK, TASK, etc.) with exactly 2 BUGs included.
- Ensure **exactly 10 to 12 tasks** in total.
- Each task must be **unique** — avoid redundancy or overlapping responsibilities.
- STORY tasks must follow the format: ""As a [user role], I want [goal] so that [benefit].""
- TASK tasks should support STORY tasks (e.g., for a registration story, tasks like ""Create registration form"" or ""Save user data to database"").
- BUG tasks should represent realistic issues (e.g., fixing incorrect form validation or database errors).
- Tasks must be **clear, practical, and achievable**, reflecting real-world development steps.
- Titles must not exceed 65 characters.
- Only return a valid raw JSON array. No markdown, no explanation, no extra characters.";

        var requestData = new
        {
            contents = new[]
            {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
        };

        var requestJson = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Gemini API Error: {response.StatusCode}\nResponse: {responseString}");
        }

        if (string.IsNullOrWhiteSpace(responseString))
        {
            throw new Exception("Gemini response is empty.");
        }

        var parsedResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseString);
        var replyText = parsedResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text?.Trim();

        if (string.IsNullOrEmpty(replyText))
        {
            throw new Exception("Gemini did not return any text response.");
        }

        // Remove ``` if present
        if (replyText.StartsWith("```"))
        {
            replyText = replyText.Replace("```json", "")
                                 .Replace("```", "")
                                 .Replace("json", "")
                                 .Trim();
        }

        if (!replyText.StartsWith("["))
        {
            throw new Exception("Gemini reply is not a JSON array:\n" + replyText);
        }

        try
        {
            var checklistItems = JsonConvert.DeserializeObject<List<TaskSuggestionRequestDTO>>(replyText);
            return checklistItems;
        }
        catch (Exception ex)
        {
            throw new Exception("Error deserializing checklist items from Gemini reply:\n" + replyText + "\n" + ex.Message);
        }
    }

    public async Task<ProjectMetricRequestDTO> CalculateProjectMetricsAsync(Project project, List<Tasks> tasks)
    {
        var taskList = JsonConvert.SerializeObject(tasks.Select(t => new
        {
            t.Title,
            t.Description,
            t.PlannedStartDate,
            t.PlannedEndDate,
            t.ActualStartDate,
            t.ActualEndDate,
            t.PercentComplete,
            t.PlannedHours,
            t.ActualHours,
            t.PlannedCost,
            t.ActualCost,
            t.Status
        }), Formatting.Indented);

        var prompt = $@"
            Bạn là một chuyên gia quản lý dự án. Dưới đây là thông tin dự án và danh sách các task. 

            Hãy thực hiện:
            1. Hãy trả về kết quả dưới dạng một JSON object, bao gồm đầy đủ tất cả các trường sau (không được thiếu bất kỳ trường nào, và đúng định dạng):

                - plannedValue: Tổng giá trị kế hoạch của các công việc đã lên lịch tính đến hiện tại.
                - earnedValue: Tổng giá trị kiếm được theo % hoàn thành của từng task.
                - actualCost: Tổng chi phí thực tế.
                - spi: Schedule Performance Index = EV / PV
                - cpi: Cost Performance Index = EV / AC
                - delayDays: Số ngày dự án đang trễ so với kế hoạch (nếu có).
                - budgetOverrun: Chi phí vượt ngân sách = AC - PV
                - projectedFinishDate: Ngày kết thúc dự kiến nếu giữ nguyên tốc độ hiện tại (định dạng: yyyy-MM-ddTHH:mm:ssZ), tính bằng Project.StartDate + EDAC  (EDAC = DAC/SPI là Ước lượng tổng thời gian thực tế để hoàn thành)
                - projectedTotalCost: Tổng chi phí ước tính để hoàn thành toàn bộ dự án (EAC = BAC / CPI nếu CPI hiện tại giữ nguyên)

            **Yêu cầu:** chỉ trả về đúng JSON, không giải thích, không thêm chữ, không định dạng Markdown.

            2. Nếu phát hiện:
            - SPI < 1 → dự án đang chậm tiến độ
            - CPI < 1 → dự án vượt chi phí

            Hãy thêm vào JSON một trường 'suggestions' là mảng các giải pháp cải thiện. Mỗi phần tử trong 'suggestions' cần gồm:
            - message: gợi ý hành động cụ thể
            - reason: lý do đưa ra gợi ý này
            - label: Từ khóa ngắn gọn mô tả loại gợi ý (ví dụ: “Tiến độ”, “Chi phí”)
             - relatedTasks: nếu có, là mảng task liên quan — mỗi task chỉ xuất hiện **một lần duy nhất** trong toàn bộ danh sách suggestions.
            Cấu trúc mỗi phần tử trong relatedTasks:
            - taskTitle
            - currentPlannedEndDate
            - currentPercentComplete
            - suggestedAction: hành động cụ thể cần thực hiện (VD: tăng nhân lực, kéo dài thời hạn như thế nào ...)

            **Ràng buộc nghiêm ngặt:**
            - Mỗi task chỉ được xuất hiện **một lần** trong toàn bộ suggestions. Nếu có nhiều đề xuất áp dụng cho một task, hãy gộp lại thành một entry.
            - Nếu không có task liên quan, **bỏ qua trường relatedTasks**.
            - Trả về đúng JSON thuần, không markdown, không giải thích.

            Thông tin dự án:
            - Tên: {project.Name}
            - Ngân sách: {project.Budget}
            - Thời gian bắt đầu: {project.StartDate}
            - Thời gian kết thúc: {project.EndDate}

            Danh sách task:
            {taskList}
            ";

        var requestData = new
        {
            contents = new[]
        {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
        };

        var requestJson = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Gemini API Error: {response.StatusCode}\nResponse: {responseString}");

        if (string.IsNullOrWhiteSpace(responseString))
            throw new Exception("Gemini response is empty.");

        var parsedResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseString);
        var replyText = parsedResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text?.Trim();

        if (string.IsNullOrEmpty(replyText))
            throw new Exception("Gemini did not return any text response.");

        if (replyText.StartsWith("```") && replyText.Contains("json"))
        {
            replyText = replyText.Replace("```json", "").Replace("```", "").Trim();
        }

        if (!replyText.StartsWith("{"))
            throw new Exception("Gemini reply is not a valid JSON object:\n" + replyText);

        try
        {
            var result = JsonConvert.DeserializeObject<ProjectMetricRequestDTO>(replyText);
            result.ProjectId = project.Id;
            result.CalculatedBy = "AI";
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception("Error parsing ProjectMetricRequestDTO from Gemini reply:\n" + replyText + "\n" + ex.Message);
        }
    }

    private class GeminiChecklistItem
    {
        public string title { get; set; }
    }

    private class GeminiResponse
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

    public async Task<string> SummarizeTextAsync(string transcriptText)
    {
        var prompt = $@"Summarize the following meeting transcript in concise, clear English (max 200 words):

{transcriptText}

Summary:";

        var requestData = new
        {
            contents = new[]
            {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
        };

        var requestJson = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Gemini API Error: {response.StatusCode}\nResponse: {responseString}");

        var parsedResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseString);
        var replyText = parsedResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text?.Trim();

        if (string.IsNullOrEmpty(replyText))
            throw new Exception("Gemini did not return any text response.");

        // Clean up markdown if present
        if (replyText.StartsWith("```"))
        {
            replyText = replyText.Replace("```", "").Replace("json", "").Trim();
        }

        return replyText;
    }

    public async Task<List<RiskRequestDTO>> DetectProjectRisksAsync(Project project, List<Tasks> tasks)
    {
        var taskList = JsonConvert.SerializeObject(tasks.Select(t => new
        {
            t.Id,
            t.Title,
            t.Description,
            t.PlannedStartDate,
            t.PlannedEndDate,
            t.ActualStartDate,
            t.ActualEndDate,
            t.PercentComplete,
            t.PlannedHours,
            t.ActualHours,
            t.PlannedCost,
            t.ActualCost,
            t.Status,
            t.Priority
        }), Formatting.Indented);

        var riskTypes = await _dynamicCategoryRepo.GetByCategoryGroupAsync("risk_type");
        var probabilities = await _dynamicCategoryRepo.GetByCategoryGroupAsync("risk_probability_level");
        var impactLevels = await _dynamicCategoryRepo.GetByCategoryGroupAsync("risk_impact_level");
        //var validRiskTypes = riskTypes.Select(r => r.Name).ToList();
        var validTypes = string.Join(", ", riskTypes.Select(t => $"'{t.Name}'"));
        var validProbabilities = string.Join(", ", probabilities.Select(p => $"'{p.Name}'"));
        var validImpactLevels = string.Join(", ", impactLevels.Select(i => $"'{i.Name}'"));


        var prompt = $@"
You are a risk management expert specializing in software projects. Below is the information about a software project and its tasks.

Analyze the project and predict **5 project-level risks** that could impact its overall success. These risks should focus on high-level concerns (e.g., budget, schedule, scope, resources, or external factors) and not be tied to specific tasks. For each risk, provide a mitigation plan and a contingency plan.

Return the result as a JSON array with the following structure for each risk:

[
  {{
    title: string, // Short, descriptive title of the risk
    description: string, // Detailed explanation of the risk and its potential impact
    type: string, // Use only the following values: {validTypes}
    probability: string, // Use only the following values: {validProbabilities}
    impactLevel: string, // Use only the following values: {validImpactLevels}
    severityLevel: string, // High | Medium | Low
    mitigationPlan: string, // Plan to reduce the likelihood or impact of the risk
    contingencyPlan: string // Plan to handle the risk if it occurs
  }}
]

**Requirements:**
- Focus on project-level risks, not task-specific issues.
- Ensure risks are specific to the project's context (e.g., budget, timeline, or domain).
- Avoid generic or vague risks; use the provided project and task data to infer realistic risks.
- Return only the JSON array, without markdown, headers, or additional explanations.
- Provide all text in Engl

**Project Information:**
- Name: {project.Name}
- Budget: {project.Budget} USD
- Start Date: {project.StartDate}
- End Date: {project.EndDate}
- Project Type: Software Development
- Additional Context: {{Add any known context, e.g., ""Complex project with multiple third-party integrations"", ""Inexperienced team"", or ""Tight regulatory requirements"". If none, leave as ""N/A"".}}

**Task List (for context, do not analyze individual tasks):**
{taskList}
";

        var requestData = new
        {
            contents = new[]
            {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
        };

        var requestJson = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Gemini API Error: {response.StatusCode}\nResponse: {responseString}");

        if (string.IsNullOrWhiteSpace(responseString))
            throw new Exception("Gemini response is empty.");

        var parsedResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseString);
        var replyText = parsedResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text?.Trim();

        if (string.IsNullOrEmpty(replyText))
            throw new Exception("Gemini did not return any text response.");

        if (replyText.StartsWith("```") && replyText.Contains("json"))
        {
            replyText = replyText.Replace("```json", "").Replace("```", "").Trim();
        }

        if (!replyText.StartsWith("["))
            throw new Exception("Gemini reply is not a valid JSON array:\n" + replyText);

        try
        {
            var risks = JsonConvert.DeserializeObject<List<RiskRequestDTO>>(replyText);
            if (risks == null || risks.Count == 0)
                throw new Exception("Không tìm thấy rủi ro nào từ phản hồi Gemini.");

            foreach (var risk in risks)
            {
                risk.ProjectId = project.Id;
                //risk.ResponsibleId = 1;
                risk.TaskId = null;
                //risk.GeneratedBy = "AI";
                //risk.RiskScope = "Project";
                //risk.IsApproved = false;
            }
            return risks;
        }
        catch (Exception ex)
        {
            throw new Exception("Error parsing RiskDTO from Gemini reply:\n" + replyText + "\n" + ex.Message);
        }
    }

    public async Task<List<AIRiskResponseDTO>> ViewAIProjectRisksAsync(Project project, List<Tasks> tasks)
    {
        var taskList = JsonConvert.SerializeObject(tasks.Select(t => new
        {
            t.Title,
            t.Description,
            t.PlannedStartDate,
            t.PlannedEndDate,
            t.ActualStartDate,
            t.ActualEndDate,
            t.PercentComplete,
            t.PlannedHours,
            t.ActualHours,
            t.PlannedCost,
            t.ActualCost,
            t.Status
        }), Formatting.Indented);

        var riskTypes = await _dynamicCategoryRepo.GetByCategoryGroupAsync("risk_type");
        var probabilities = await _dynamicCategoryRepo.GetByCategoryGroupAsync("risk_probability_level");
        var impactLevels = await _dynamicCategoryRepo.GetByCategoryGroupAsync("risk_impact_level");
        //var validRiskTypes = riskTypes.Select(r => r.Name).ToList();
        var validTypes = string.Join(", ", riskTypes.Select(t => $"'{t.Name}'"));
        var validProbabilities = string.Join(", ", probabilities.Select(p => $"'{p.Name}'"));
        var validImpactLevels = string.Join(", ", impactLevels.Select(i => $"'{i.Name}'"));

        var defaultRiskScopeConfig = await _systemConfigRepo.GetByConfigKeyAsync("default_risk_scope");
        var defaultRiskScope = defaultRiskScopeConfig?.ValueConfig ?? "Project";

        var prompt = $@"
You are a risk management expert specializing in software projects. Below is the information about a software project and its tasks.

Analyze the project and predict **5 project-level risks** that could impact its overall success. These risks should focus on high-level concerns (e.g., budget, schedule, scope, resources, or external factors) and not be tied to specific tasks. For each risk, provide a mitigation plan and a contingency plan.

Return the result as a JSON array with the following structure for each risk:

[
  {{
    title: string, // Short, descriptive title of the risk
    description: string, // Detailed explanation of the risk and its potential impact
    type: string, // Use only the following values: {validTypes}
    probability: string, // Use only the following values: {validProbabilities}
    impactLevel: string, // Use only the following values: {validImpactLevels}
    mitigationPlan: string, // Plan to reduce the likelihood or impact of the risk
    contingencyPlan: string // Plan to handle the risk if it occurs
  }}
]

**Requirements:**
- Focus on project-level risks, not task-specific issues.
- Ensure risks are specific to the project's context (e.g., budget, timeline, or domain).
- Avoid generic or vague risks; use the provided project and task data to infer realistic risks.
- Return only the JSON array, without markdown, headers, or additional explanations.
- Provide all text in Engl

**Project Information:**
- Name: {project.Name}
- Budget: {project.Budget} USD
- Start Date: {project.StartDate}
- End Date: {project.EndDate}
- Project Type: Software Development

**Task List (for context, do not analyze individual tasks):**
{taskList}
";

        var requestData = new
        {
            contents = new[]
            {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
        };

        var requestJson = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Gemini API Error: {response.StatusCode}\nResponse: {responseString}");

        if (string.IsNullOrWhiteSpace(responseString))
            throw new Exception("Gemini response is empty.");

        var parsedResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseString);
        var replyText = parsedResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text?.Trim();

        if (string.IsNullOrEmpty(replyText))
            throw new Exception("Gemini did not return any text response.");

        if (replyText.StartsWith("```") && replyText.Contains("json"))
        {
            replyText = replyText.Replace("```json", "").Replace("```", "").Trim();
        }

        if (!replyText.StartsWith("["))
            throw new Exception("Gemini reply is not a valid JSON array:\n" + replyText);

        try
        {
            var risks = JsonConvert.DeserializeObject<List<AIRiskResponseDTO>>(replyText);
            if (risks == null || risks.Count == 0)
                throw new Exception("Không tìm thấy rủi ro nào từ phản hồi Gemini.");

            foreach (var risk in risks)
            {
                risk.ProjectId = project.Id;
                risk.TaskId = null;
                risk.RiskScope = defaultRiskScope;
            }
            return risks;
        }
        catch (Exception ex)
        {
            throw new Exception("Error parsing RiskDTO from Gemini reply:\n" + replyText + "\n" + ex.Message);
        }
    }

    public async Task<List<AIRecommendationDTO>> GenerateProjectRecommendationsAsync(
    Project project,
    ProjectMetric metric,
    List<Tasks> tasks,
    List<Sprint> sprints,
    List<Milestone> milestones,
    List<Subtask> subtasks,
    List<Account> accounts,
    List<ProjectPosition> projectPositions,
    List<ProjectMember> projectMembers,
    List<TaskAssignment> taskAssignments)
    {
        var validRecommendationTypes = await _dynamicCategoryRepo.GetByCategoryGroupAsync("recommendation_type");
        var recommendationTypes = validRecommendationTypes
            .Where(c => c.Name == RecommendationTypeEnum.COST.ToString() || c.Name == RecommendationTypeEnum.SCHEDULE.ToString())
            .Select(c => c.Name)
            .ToList();

        if (!recommendationTypes.Any())
            throw new Exception("No valid recommendation types (Cost or Schedule) found in dynamic_category.");

        var spiWarningThresholdConfig = await _systemConfigRepo.GetByConfigKeyAsync("spi_warning_threshold");
        var cpiWarningThresholdConfig = await _systemConfigRepo.GetByConfigKeyAsync("cpi_warning_threshold");
        decimal spiWarningThreshold = spiWarningThresholdConfig != null ? decimal.Parse(spiWarningThresholdConfig.ValueConfig) : 1m;
        decimal cpiWarningThreshold = cpiWarningThresholdConfig != null ? decimal.Parse(cpiWarningThresholdConfig.ValueConfig) : 1m;

        // Determine recommendation type based on CPI and SPI
        string recommendationType;
        if (metric.CostPerformanceIndex >= cpiWarningThreshold && metric.SchedulePerformanceIndex < spiWarningThreshold)
        {
            recommendationType = RecommendationTypeEnum.SCHEDULE.ToString();
        }
        else if (metric.SchedulePerformanceIndex >= spiWarningThreshold && metric.CostPerformanceIndex < cpiWarningThreshold)
        {
            recommendationType = RecommendationTypeEnum.COST.ToString();
        }
        else
        {
            // Randomly select Cost or Schedule if both CPI and SPI are < 1
            recommendationType = new Random().Next(0, 2) == 0 ? RecommendationTypeEnum.COST.ToString() : RecommendationTypeEnum.SCHEDULE.ToString();
        }

        // Serialize input data
        var taskList = JsonConvert.SerializeObject(tasks.Select(t => new
        {
            t.Id,
            t.Title,
            t.Description,
            t.PlannedStartDate,
            t.PlannedEndDate,
            t.ActualStartDate,
            t.ActualEndDate,
            t.PercentComplete,
            t.PlannedHours,
            t.ActualHours,
            t.PlannedCost,
            t.ActualCost,
            t.Status,
            t.Priority
        }), Formatting.Indented);

        var subtaskList = JsonConvert.SerializeObject(subtasks.Select(st => new
        {
            st.Id,
            st.Title,
            st.Description,
            st.TaskId,
            st.Status,
            st.PlannedStartDate,
            st.PlannedEndDate,
            st.ActualStartDate,
            st.ActualEndDate,
            st.PercentComplete,
            st.PlannedHours,
            st.ActualHours,
            st.AssignedBy,
        }), Formatting.Indented);

        var sprintList = JsonConvert.SerializeObject(sprints.Select(s => new
        {
            s.Id,
            s.Name,
            s.Goal,
            s.StartDate,
            s.EndDate,
            s.Status
        }), Formatting.Indented);

        var milestoneList = JsonConvert.SerializeObject(milestones.Select(m => new
        {
            m.Id,
            m.Name,
            m.Key,
            m.Description,
            m.StartDate,
            m.EndDate,
            m.Status
        }), Formatting.Indented);

        var accountList = JsonConvert.SerializeObject(accounts.Select(a => new
        {
            a.Id,
            a.FullName,
            a.Username,
            a.Role, 
        }), Formatting.Indented);

        var projectPositionList = JsonConvert.SerializeObject(projectPositions.Select(pp => new
        {
            pp.Id,
            pp.ProjectMemberId,
            pp.Position,
        }), Formatting.Indented);

        var projectMemberList = JsonConvert.SerializeObject(projectMembers.Select(pm => new
        {
            pm.Id,
            pm.ProjectId,
            pm.AccountId,
            pm.HourlyRate,
        }), Formatting.Indented);

        var taskAssignmentList = JsonConvert.SerializeObject(taskAssignments.Select(ta => new
        {
            ta.Id,
            ta.AccountId,
            ta.TaskId,
            ta.ActualHours,
        }), Formatting.Indented);

        var prompt = $@"
You are an expert in software project management with a focus on data-driven decision-making. Below is detailed information about a software project, including tasks, subtasks, sprints, milestones, and key performance metrics.

The project is currently underperforming:
- SPI (Schedule Performance Index) = {metric.SchedulePerformanceIndex}
- CPI (Cost Performance Index) = {metric.CostPerformanceIndex}

**Special Note**: Focus recommendations on improving **{recommendationType}**. If SPI or CPI is 0, it indicates no tasks have started or no costs have been incurred. In such cases, prioritize initiating critical tasks for Schedule or establishing cost baselines for Cost.

**Task**: Analyze the provided data and propose exactly 5 specific, actionable, and data-driven recommendations to improve the project's {recommendationType} performance. Each recommendation must be based on specific evidence from the tasks, subtasks, sprints, or milestones provided.

**Requirements for Each Recommendation**:
1. **Goal**: Clearly state the objective (e.g., reduce cost overrun, accelerate schedule).
2. **Root Cause**: Identify a specific issue with evidence (e.g., ""Task TASK-003 has ActualHours 20% above PlannedHours"" for Cost, or ""Task TASK-004 is 30% behind schedule"" for Schedule).
3. **Action Steps**: Provide precise actions, such as:
   - Adjust specific fields (e.g., 'Set PlannedHours of TASK-003 to 80').
   - Reassign personnel (e.g., 'Assign 1 senior developer to SUBTASK-005').
   - Reschedule tasks or milestones (e.g., 'Move TASK-004 to Sprint 2 starting 2025-08-22').
   - Merge or split tasks/subtasks to optimize scope.
   - Set realistic cost estimates based on task complexity and average resource cost ($50/hour for senior developers, $30/hour for junior developers).
4. **Expected Impact**: Quantify the expected outcome (e.g., ""Reduce schedule delay by 3 days"", ""Cut costs by 10%"").
5. **Priority**: Assign a priority level (1 = High, 2 = Medium, 3 = Low) based on urgency and impact.

**Output Format**:
Return a JSON array with exactly 5 items, each with the following structure:
[
  {{
    recommendation: string,         // Short description of the recommendation
    details: string,               // Detailed explanation of root cause and actions
    type: string,                  // Must be '{recommendationType}'
    affectedTasks: string[],       // List of affected task IDs, subtask IDs, milestone Keys
    expectedImpact: string,        // Quantified impact (e.g., ""Reduce cost by 10%"")
    suggestedChanges: string,      // Clear description of changes (e.g., ""Increase PlannedHours of TASK-003 to 40""), use FullName or Username for personnel (e.g., ""Assign Chi to TASK-003"")
    priority: number              // 1 (High), 2 (Medium), 3 (Low)
  }}
]

**Strict Constraints**:
- Recommendations **must** reference specific data (e.g., task IDs like 'TASK-001', subtask IDs like 'SUBTASK-005', sprint names, milestone keys).
- All recommendations must be of type '{recommendationType}'.
- Avoid generic suggestions (e.g., ""Improve communication"")—focus on measurable actions.
- Ensure actions are feasible within the project's context (budget: {project.Budget}, timeline: {project.StartDate} to {project.EndDate}).
- Do not return markdown or additional text outside the JSON array.
- Assume personnel costs: $50/hour for senior developers (e.g., John Doe), $30/hour for junior developers (e.g., Bob Jones).

Project Information:
- Name: {project.Name}
- Budget: {project.Budget}
- Start Date: {project.StartDate}
- End Date: {project.EndDate}

Metric Data:
- PV: {metric.PlannedValue}
- EV: {metric.EarnedValue}
- AC: {metric.ActualCost}
- SPI: {metric.SchedulePerformanceIndex}
- CPI: {metric.CostPerformanceIndex}

Task List:
{taskList}

Subtasks:
{subtaskList}

Sprint List:
{sprintList}

Milestone List:
{milestoneList}

Account List:
{accountList}

Project Position List:
{projectPositionList}

Project Member List:
{projectMemberList}

Task Assignment List:
{taskAssignmentList}

All output must be written in English.
";

        var requestData = new
        {
            contents = new[]
            {
            new
            {
                parts = new[] { new { text = prompt } }
            }
        }
        };

        var requestJson = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Gemini API Error: {response.StatusCode}\nResponse: {responseString}");

        if (string.IsNullOrWhiteSpace(responseString))
            throw new Exception("Gemini response is empty.");

        var parsedResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseString);
        var replyText = parsedResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text?.Trim();

        if (string.IsNullOrEmpty(replyText))
            throw new Exception("Gemini did not return any text response.");

        if (replyText.StartsWith("```") && replyText.Contains("json"))
        {
            replyText = replyText.Replace("```json", "").Replace("```", "").Trim();
        }

        if (!replyText.StartsWith("["))
            throw new Exception("Gemini reply is not a valid JSON array:\n" + replyText);

        try
        {
            var aiRecommendations = JsonConvert.DeserializeObject<List<AIRecommendationDTO>>(replyText);
            if (aiRecommendations == null || aiRecommendations.Count != 5)
                throw new Exception($"Expected exactly 5 recommendations, received {aiRecommendations?.Count ?? 0} from AI.");

            // Validate that all recommendations are of the correct type
            if (aiRecommendations.Any(r => r.Type != recommendationType))
                throw new Exception($"All recommendations must be of type '{recommendationType}'.");

            return aiRecommendations;
        }
        catch (Exception ex)
        {
            throw new Exception("Error parsing ProjectRecommendation from Gemini reply:\n" + replyText + "\n" + ex.Message);
        }
    }

    public async Task<SimulatedMetricDTO> SimulateProjectMetricsAfterRecommendationsAsync(
    Project project,
    ProjectMetric currentMetric,
    List<Tasks> tasks,
    List<Sprint> sprints,
    List<Milestone> milestones,
    List<Subtask> subtasks,
    List<ProjectRecommendation> approvedRecommendations)
    {
        var changesDescription = string.Join("\n- ", approvedRecommendations
            .Where(r => !string.IsNullOrWhiteSpace(r.Recommendation))
            .Select(r => r.Recommendation.Trim()));

        if (string.IsNullOrWhiteSpace(changesDescription))
            throw new Exception("No approved suggested changes found to simulate.");

        var taskList = JsonConvert.SerializeObject(tasks.Select(t => new
        {
            t.Id,
            t.Title,
            t.Description,
            t.PlannedStartDate,
            t.PlannedEndDate,
            t.ActualStartDate,
            t.ActualEndDate,
            t.PercentComplete,
            t.PlannedHours,
            t.ActualHours,
            t.PlannedCost,
            t.ActualCost,
            t.Status,
            t.Priority
        }), Formatting.Indented);

        var subtaskList = JsonConvert.SerializeObject(subtasks.Select(st => new
        {
            st.Id,
            st.Title,
            st.Description,
            st.TaskId,
            st.Status,
            st.PlannedStartDate,
            st.PlannedEndDate,
            st.ActualStartDate,
            st.ActualEndDate,
            st.PercentComplete,
            st.PlannedHours,
            st.ActualHours,
        }), Formatting.Indented);

        var sprintList = JsonConvert.SerializeObject(sprints.Select(s => new
        {
            s.Id,
            s.Name,
            s.Goal,
            s.StartDate,
            s.EndDate,
            s.Status
        }), Formatting.Indented);

        var milestoneList = JsonConvert.SerializeObject(milestones.Select(m => new
        {
            m.Id,
            m.Name,
            m.Description,
            m.StartDate,
            m.EndDate,
            m.Status
        }), Formatting.Indented);

        var prompt = $@"
You are an expert project control analyst specializing in Earned Value Management (EVM) for software projects. Your task is to simulate the impact of approved recommendations on the project's key performance indicators (KPIs) and assess whether these changes improve the project's performance compared to the current state.

**Current Project Metrics**:
- Budget at Completion (BAC): {currentMetric.BudgetAtCompletion}
- Duration at Completion (DAC): {currentMetric.DurationAtCompletion}
- Planned Value (PV): {currentMetric.PlannedValue}
- Earned Value (EV): {currentMetric.EarnedValue}
- Actual Cost (AC): {currentMetric.ActualCost}
- Schedule Performance Index (SPI): {currentMetric.SchedulePerformanceIndex}
- Cost Performance Index (CPI): {currentMetric.CostPerformanceIndex}

**Approved Recommendations**:
- {changesDescription}

**Project Context**:
- Name: {project.Name}
- Budget: {project.Budget}
- Start Date: {project.StartDate}
- End Date: {project.EndDate}

**Task**: Simulate the new KPIs after applying the approved recommendations. Use the provided task, subtask, sprint, and milestone data to estimate realistic changes. For each recommendation, consider its specific impact (e.g., rescheduling a task affects SPI, reducing hours affects CPI). Compare the simulated metrics to the current metrics to determine if the project's performance improves.

**Requirements**:
1. **Simulate Metrics**: Calculate new values for SPI, CPI, EAC, ETC, VAC, and EDAC using EVM formulas:
   - EAC = BAC / CPI
   - ETC = EAC - AC
   - VAC = BAC - EAC
   - EDAC = DAC / SPI
2. **Improvement Assessment**: Determine if the project improves (e.g., SPI or CPI closer to or above 1, reduced EAC or EDAC).
3. **Confidence Score**: Provide a confidence score (0–100) based on data completeness and recommendation feasibility.
4. **Improvement Summary**: Summarize key improvements (e.g., ""SPI increased by 0.1 due to rescheduling task X"") or risks if no improvement.
5. **Constraints**:
   - Ensure simulations respect project budget ({project.Budget}) and timeline ({project.StartDate} to {project.EndDate}).
   - Base estimates on specific recommendation actions (e.g., hours reduced, tasks rescheduled).
   - Assume resource cost of $100/hour unless specified in data.

**Output Format** (JSON only, no markdown or extra text):
{{
  ""SchedulePerformanceIndex"": number,    // Simulated SPI
  ""CostPerformanceIndex"": number,        // Simulated CPI
  ""EstimateAtCompletion"": number,        // Simulated EAC
  ""EstimateToComplete"": number,          // Simulated ETC
  ""VarianceAtCompletion"": number,        // Simulated VAC
  ""EstimatedDurationAtCompletion"": number, // Simulated EDAC
  ""IsImproved"": boolean,                 // True if SPI or CPI improves significantly
  ""ImprovementSummary"": string,          // Summary of improvements or risks (e.g., ""SPI improved by 0.1 due to task rescheduling"")
  ""ConfidenceScore"": number              // Confidence in simulation (0–100)
}}

**Data**:
Task List:
{taskList}

Subtasks:
{subtaskList}

Sprint List:
{sprintList}

Milestone List:
{milestoneList}

All output must be in English.
";

        var requestData = new
        {
            contents = new[]
            {
            new
            {
                parts = new[] { new { text = prompt } }
            }
        }
        };

        var requestJson = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Gemini API Error: {response.StatusCode}\nResponse: {responseString}");

        var parsedResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseString);
        var replyText = parsedResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text?.Trim();

        if (string.IsNullOrWhiteSpace(replyText))
            throw new Exception("Gemini response is empty.");

        if (replyText.StartsWith("```") && replyText.Contains("json"))
        {
            replyText = replyText.Replace("```json", "").Replace("```", "").Trim();
        }

        try
        {
            var simulatedMetric = JsonConvert.DeserializeObject<SimulatedMetricDTO>(replyText);
            if (simulatedMetric == null)
                throw new Exception("Could not parse simulated metrics.");
            return simulatedMetric;
        }
        catch (Exception ex)
        {
            throw new Exception("Error parsing simulated metrics from Gemini reply:\n" + replyText + "\n" + ex.Message);
        }
    }

    public async Task<List<AIRiskResponseDTO>> DetectTaskRisksAsync(Project project, List<Tasks> tasks)
    {
        var risks = new List<AIRiskResponseDTO>();
        var errors = new List<string>();

        if (tasks == null || !tasks.Any())
        {
            errors.Add("No tasks provided for risk analysis.");
            return risks; // Return empty list with logged error
        }

        foreach (var task in tasks)
        {
            // Validate task data
            if (string.IsNullOrEmpty(task.Title) || task.PlannedStartDate == null || task.PlannedEndDate == null)
            {
                errors.Add($"Task {task.Id} has missing or invalid data (Title, PlannedStartDate, or PlannedEndDate).");
                continue;
            }

            var prompt = $@"
You are a risk management expert specializing in software projects. Below is the information about a specific task in a software project.

Analyze the task and predict **1-2 task-specific risks** that could impact its completion or the project's success. Focus on risks directly related to the task's attributes (e.g., delays, cost overruns, resource issues, or quality concerns). For each risk, provide a mitigation plan and a contingency plan.

Return the result as a JSON array with the following structure for each risk:

[
  {{
    title: string,
    description: string,
    type: string, // SCHEDULE, FINANCIAL, RESOURCE, QUALITY, SCOPE, TECHNICAL, SECURITY
    probability: string, // High | Medium | Low
    impactLevel: string, // High | Medium | Low
    severityLevel: string, // High | Medium | Low
    mitigationPlan: string,
    contingencyPlan: string
  }}
]

**Requirements:**
- Focus on risks specific to the provided task.
- Use the task's attributes to infer realistic risks.
- If any attribute is missing (e.g., 'N/A'), make reasonable assumptions but note them in the risk description.
- Avoid generic or vague risks.
- Return only the JSON array, without markdown, headers, or additional explanations.
- Provide all text in English.

**Task Information:**
- Title: {task.Title}
- Description: {task.Description ?? "N/A"}
- Planned Start Date: {task.PlannedStartDate}
- Planned End Date: {task.PlannedEndDate}
- Actual Start Date: {task.ActualStartDate?.ToString() ?? "N/A"}
- Actual End Date: {task.ActualEndDate?.ToString() ?? "N/A"}
- Percent Complete: {task.PercentComplete}%
- Planned Hours: {task.PlannedHours?.ToString() ?? "N/A"}
- Actual Hours: {task.ActualHours?.ToString() ?? "N/A"}
- Planned Cost: {task.PlannedCost?.ToString() ?? "N/A"}
- Actual Cost: {task.ActualCost?.ToString() ?? "N/A"}
- Status: {task.Status ?? "N/A"}

**Project Context (for reference):**
- Name: {project.Name}
- Budget: {project.Budget} USD
- Start Date: {project.StartDate}
- End Date: {project.EndDate}
";

            try
            {
                var requestData = new
                {
                    contents = new[]
                    {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
                };

                var requestJson = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    errors.Add($"Gemini API error for task {task.Id}: {response.StatusCode} - {responseString}");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(responseString))
                {
                    errors.Add($"Empty response from Gemini API for task {task.Id}.");
                    continue;
                }

                var parsedResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseString);
                var replyText = parsedResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text?.Trim();

                if (string.IsNullOrEmpty(replyText))
                {
                    errors.Add($"No text response from Gemini API for task {task.Id}.");
                    continue;
                }

                if (replyText.StartsWith("```") && replyText.Contains("json"))
                {
                    replyText = replyText.Replace("```json", "").Replace("```", "").Trim();
                }

                if (!replyText.StartsWith("["))
                {
                    errors.Add($"Invalid JSON array response from Gemini for task {task.Id}: {replyText}");
                    continue;
                }

                var taskRisks = JsonConvert.DeserializeObject<List<AIRiskResponseDTO>>(replyText);
                if (taskRisks == null || !taskRisks.Any())
                {
                    errors.Add($"No risks identified by Gemini for task {task.Id}. Response: {replyText}");
                    continue;
                }

                foreach (var risk in taskRisks)
                {
                    risk.ProjectId = project.Id;
                    risk.TaskId = task.Id;
                    risk.RiskScope = "Task";
                    risks.Add(risk);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error processing task {task.Id}: {ex.Message}");
                continue;
            }
        }
        return risks;
    }
}
