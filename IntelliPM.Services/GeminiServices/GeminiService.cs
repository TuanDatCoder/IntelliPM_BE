using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using IntelliPM.Services.GeminiServices;
using IntelliPM.Data.DTOs.ProjectMetric.Request;
using IntelliPM.Data.Entities;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using FirebaseAdmin.Messaging;
using System.Threading.Tasks;
using IntelliPM.Data.DTOs.Risk.Request;
using AutoMapper;
using IntelliPM.Data.DTOs.ProjectRecommendation.Response;
using IntelliPM.Data.DTOs.Risk.Response;
using IntelliPM.Data.DTOs.AiResponseHistory.Request;
using IntelliPM.Repositories.AiResponseHistoryRepos;
using Org.BouncyCastle.Asn1.Ocsp;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private const string _apiKey = "AIzaSyD52tMVJMjE9GxHZwshWwobgQ8bI4rGabA";
    private readonly string _url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
    private readonly IMapper _mapper;
    private readonly IAiResponseHistoryRepository _aiResponseHistoryRepo;

    public GeminiService(HttpClient httpClient, IMapper mapper, IAiResponseHistoryRepository aiResponseHistoryRepo)
    {
        _httpClient = httpClient;
        _mapper = mapper;
        _aiResponseHistoryRepo = aiResponseHistoryRepo;
    }

    public async Task<List<string>> GenerateSubtaskAsync(string taskTitle)
    {
        var prompt = @$"Please list 5 to 7 specific Subtask items required to complete the task titled: ""{taskTitle}"".
Each Subtask item must follow **this exact JSON format** and be returned as a JSON array only (no explanation):

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
  }},
  ...
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

        // Clean replyText trước khi parse
        replyText = replyText.Trim();

        // Nếu reply bắt đầu bằng ``` thì loại bỏ
        if (replyText.StartsWith("```"))
        {
            // Loại bỏ tất cả ``` và từ 'json' nếu có
            replyText = replyText.Replace("```json", "")
                                 .Replace("```", "")
                                 .Replace("json", "")
                                 .Trim();
        }

        // Kiểm tra lại nếu vẫn không bắt đầu bằng dấu [ thì báo lỗi
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

    //public async Task<ProjectMetricRequestDTO> CalculateProjectMetricsAsync(Project project, List<Tasks> tasks)
    //{
    //    var taskList = JsonConvert.SerializeObject(tasks.Select(t => new
    //    {
    //        t.Title,
    //        t.Description,
    //        t.PlannedStartDate,
    //        t.PlannedEndDate,
    //        t.ActualStartDate,
    //        t.ActualEndDate,
    //        t.PercentComplete,
    //        t.PlannedHours,
    //        t.ActualHours,
    //        t.PlannedCost,
    //        t.ActualCost,
    //        t.Status
    //    }), Formatting.Indented);

    //    var prompt = $@"
    //    Bạn là một chuyên gia quản lý dự án. Dưới đây là thông tin dự án và danh sách các task. 

    //    Hãy thực hiện:
    //    1. Hãy trả về kết quả dưới dạng một JSON object, bao gồm đầy đủ tất cả các trường sau (không được thiếu bất kỳ trường nào, và đúng định dạng):

    //        - plannedValue: Tổng giá trị kế hoạch của các công việc đã lên lịch tính đến hiện tại.
    //        - earnedValue: Tổng giá trị kiếm được theo % hoàn thành của từng task.
    //        - actualCost: Tổng chi phí thực tế.
    //        - spi: Schedule Performance Index = EV / PV
    //        - cpi: Cost Performance Index = EV / AC
    //        - delayDays: Số ngày dự án đang trễ so với kế hoạch (nếu có).
    //        - budgetOverrun: Chi phí vượt ngân sách = AC - PV
    //        - projectedFinishDate: Ngày kết thúc dự kiến nếu giữ nguyên tốc độ hiện tại (định dạng: yyyy-MM-ddTHH:mm:ssZ), tính bằng Project.StartDate + EDAC  (EDAC = DAC/SPI là Ước lượng tổng thời gian thực tế để hoàn thành)
    //        - projectedTotalCost: Tổng chi phí ước tính để hoàn thành toàn bộ dự án (EAC = BAC / CPI nếu CPI hiện tại giữ nguyên)

    //    **Yêu cầu:** chỉ trả về đúng JSON, không giải thích, không thêm chữ, không định dạng Markdown.

    //    2. Nếu phát hiện:
    //    - SPI < 1 → dự án đang chậm tiến độ
    //    - CPI < 1 → dự án vượt chi phí

    //    Hãy thêm vào JSON một trường 'suggestions' là mảng các giải pháp cải thiện. Mỗi phần tử trong 'suggestions' cần gồm:
    //    - message: gợi ý hành động cụ thể
    //    - reason: lý do đưa ra gợi ý này
    //    - label: Từ khóa ngắn gọn mô tả loại gợi ý (ví dụ: “Tiến độ”, “Chi phí”)
    //     - relatedTasks: nếu có, là mảng task liên quan — mỗi task chỉ xuất hiện **một lần duy nhất** trong toàn bộ danh sách suggestions.
    //    Cấu trúc mỗi phần tử trong relatedTasks:
    //    - taskTitle
    //    - currentPlannedEndDate
    //    - currentPercentComplete
    //    - suggestedAction: hành động cụ thể cần thực hiện (VD: tăng nhân lực, kéo dài thời hạn như thế nào ...)

    //    **Ràng buộc nghiêm ngặt:**
    //    - Mỗi task chỉ được xuất hiện **một lần** trong toàn bộ suggestions. Nếu có nhiều đề xuất áp dụng cho một task, hãy gộp lại thành một entry.
    //    - Nếu không có task liên quan, **bỏ qua trường relatedTasks**.
    //    - Trả về đúng JSON thuần, không markdown, không giải thích.

    //    Thông tin dự án:
    //    - Tên: {project.Name}
    //    - Ngân sách: {project.Budget}
    //    - Thời gian bắt đầu: {project.StartDate}
    //    - Thời gian kết thúc: {project.EndDate}

    //    Danh sách task:
    //    {taskList}
    //    ";

    //    var requestData = new
    //{
    //    contents = new[]
    //    {
    //        new
    //        {
    //            parts = new[]
    //            {
    //                new { text = prompt }
    //            }
    //        }
    //    }
    //};

    //    var requestJson = JsonConvert.SerializeObject(requestData);
    //    var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

    //    var response = await _httpClient.PostAsync(_url, content);
    //    var responseString = await response.Content.ReadAsStringAsync();

    //    if (!response.IsSuccessStatusCode)
    //        throw new Exception($"Gemini API Error: {response.StatusCode}\nResponse: {responseString}");

    //    if (string.IsNullOrWhiteSpace(responseString))
    //        throw new Exception("Gemini response is empty.");

    //    var parsedResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseString);
    //    var replyText = parsedResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text?.Trim();

    //    if (string.IsNullOrEmpty(replyText))
    //        throw new Exception("Gemini did not return any text response.");

    //    if (replyText.StartsWith("```") && replyText.Contains("json"))
    //    {
    //        replyText = replyText.Replace("```json", "").Replace("```", "").Trim();
    //    }

    //    if (!replyText.StartsWith("{"))
    //        throw new Exception("Gemini reply is not a valid JSON object:\n" + replyText);

    //    try
    //    {
    //        var result = JsonConvert.DeserializeObject<ProjectMetricRequestDTO>(replyText);
    //        result.ProjectId = project.Id;
    //        result.CalculatedBy = "AI";
    //        return result;
    //    }
    //    catch (Exception ex)
    //    {
    //        throw new Exception("Error parsing ProjectMetricRequestDTO from Gemini reply:\n" + replyText + "\n" + ex.Message);
    //    }
    //}
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
Bạn là một chuyên gia quản lý rủi ro. Dưới đây là thông tin dự án phần mềm và danh sách các task.

Hãy phân tích và dự đoán 3 rủi ro tổng thể có thể xảy ra trong dự án này (không đi sâu vào chi tiết từng task).

Trả về kết quả dưới dạng JSON array. Mỗi phần tử là một rủi ro tiềm ẩn của dự án và các giải pháp đi kèm như sau:

[
  {{
    title: string,
    description: string,
    type: string, // SCHEDULE, FINANCIAL, RESOURCE, QUALITY, SCOPE, TECHNICAL, SECURITY
    probability: string, // High | Medium | Low
    impactLevel: string, // High | Medium | Low
    severityLevel: string, // High | Medium | Low
    mitigationPlan: string, // kế hoạch giảm thiểu rủi ro
    contingencyPlan: string // kế hoạch dự phòng nếu rủi ro xảy ra
  }}
]

**Yêu cầu:**
- Không đánh giá chi tiết vào từng task cụ thể.
- Tập trung vào rủi ro ở cấp độ tổng thể dự án.
- Không thêm markdown, tiêu đề hoặc giải thích bên ngoài JSON.
- Phân tích đúng và logic, tránh liệt kê rủi ro chung chung hoặc quá mơ hồ.
- Trả về dữ liệu bằng tiếng anh.

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
                risk.ResponsibleId = 1;
                risk.TaskId = null;
                risk.GeneratedBy = "AI";
                risk.RiskScope = "Project"; 
                risk.IsApproved = false;
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

        var prompt = $@"
Bạn là một chuyên gia quản lý rủi ro. Dưới đây là thông tin dự án phần mềm và danh sách các task của dự án đó.

Hãy phân tích và dự đoán 3 rủi ro tổng thể có thể xảy ra trong dự án này (không đi sâu vào chi tiết từng task).

Trả về kết quả dưới dạng JSON array. Mỗi phần tử là một rủi ro tiềm ẩn của dự án và các giải pháp đi kèm như sau:

[
  {{
    title: string,
    description: string,
    type: string, // SCHEDULE, FINANCIAL, RESOURCE, QUALITY, SCOPE, TECHNICAL, SECURITY
    probability: string, // High | Medium | Low
    impactLevel: string, // High | Medium | Low
    severityLevel: string, // High | Medium | Low
    mitigationPlan: string, // kế hoạch giảm thiểu rủi ro
    contingencyPlan: string // kế hoạch dự phòng nếu rủi ro xảy ra
  }}
]

**Yêu cầu:**
- Không đánh giá chi tiết vào từng task cụ thể.
- Tập trung vào rủi ro ở cấp độ tổng thể dự án.
- Không thêm markdown, tiêu đề hoặc giải thích bên ngoài JSON.
- Phân tích đúng và logic, tránh liệt kê rủi ro chung chung hoặc quá mơ hồ.
- Trả về dữ liệu bằng tiếng anh.

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
                risk.RiskScope = "Project";
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
    List<Subtask> subtasks)
    {
        if (metric.SchedulePerformanceIndex >= 1 && metric.CostPerformanceIndex >= 1)
            return new List<AIRecommendationDTO>();

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
            m.Key,
            m.Description,
            m.StartDate,
            m.EndDate,
            m.Status
        }), Formatting.Indented);

        var prompt = $@"
    You are an expert in software project management with a focus on data-driven decision-making. Below is detailed information about a software project, including tasks, subtasks, sprints, milestones, and key performance metrics.

    The project is currently underperforming:
- SPI (Schedule Performance Index) = {metric.SchedulePerformanceIndex}
- CPI (Cost Performance Index) = {metric.CostPerformanceIndex}

    **Special Note**: If SPI and CPI are 0, this indicates no tasks have started or no costs have been incurred. In this case, focus on initiating critical tasks, setting realistic schedules, and establishing cost baselines.

    **Task**: Analyze the provided data and propose some specific, actionable, and data-driven recommendations** to improve the project's performance. Each recommendation must be based on specific evidence from the tasks, subtasks, sprints, or milestones provided.
    
**Requirements for Each Recommendation**:
1. **Goal**: Clearly state the objective (e.g., reduce cost overrun, accelerate schedule, improve resource allocation).
2. **Root Cause**: Identify a specific issue with evidence from the data (e.g., ""Task PROJA-3 has ActualHours 20% above PlannedHours"").
3. **Action Steps**: Provide precise actions, such as:
   - Adjust specific fields (e.g., 'Set PlannedHours of task INTELLIPM-3 to 80').
   - Reassign personnel (e.g., 'Assign 1 senior developer to task INTELLIPM-5').
   - Reschedule tasks or milestones (e.g., 'Move task INTELLIPM-4 to Sprint 2 starting 2025-08-22').
   - Merge or split tasks/subtasks to optimize scope.
   - Set realistic cost estimates based on task complexity and average resource cost (assume $100/hour unless specified).
4. **Expected Impact**: Quantify the expected outcome (e.g., ""Reduce schedule delay by 3 days"", ""Cut costs by 10%"").
5. **Priority**: Assign a priority level (1 = High, 2 = Medium, 3 = Low) based on urgency and impact.

    **Output Format**:
Return a JSON array with exactly 5 items, each with the following structure:
[
  {{{{
    recommendation: string,         // Short description of the recommendation
    details: string,               // Detailed explanation of root cause and actions
    type: string,                  // Schedule | Cost | Scope | Resource | Performance | Design | Testing
    affectedTasks: string[],       // List of affected task, subtask IDs, milestone Keys
    expectedImpact: string,        // Quantified impact (e.g., ""Reduce cost by 10%"")
    suggestedChanges: string,      // Clear description of changes (e.g., ""Increase PlannedHours of PROJA-3 to 40"")
    priority: number,              // 1 (High), 2 (Medium), 3 (Low)
  }}}}
]

    **Strict Constraints**:
- Recommendations **must** reference specific data (e.g., task IDs, sprint names, milestone dates).
- Avoid generic suggestions (e.g., ""Improve communication"")—focus on measurable actions.
- Ensure actions are feasible within the project's context (budget: {{project.Budget}}, timeline: {{project.StartDate}} to {{project.EndDate}}).
- Do not return markdown or additional text outside the JSON array.

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
            if (aiRecommendations == null || aiRecommendations.Count == 0)
                throw new Exception("No recommendations received from AI.");

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


}
