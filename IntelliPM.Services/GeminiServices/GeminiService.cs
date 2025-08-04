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

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private const string _apiKey = "AIzaSyD52tMVJMjE9GxHZwshWwobgQ8bI4rGabA";
    private readonly string _url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
    private readonly IMapper _mapper;

    public GeminiService(HttpClient httpClient, IMapper mapper)
    {
        _httpClient = httpClient;
        _mapper = mapper;
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
            m.Description,
            m.StartDate,
            m.EndDate,
            m.Status
        }), Formatting.Indented);

        var prompt = $@"
You are an experienced project management expert. Below is the detailed information of a software project including tasks, sprints, milestones, and key performance metrics.

The project is currently underperforming:
- SPI (Schedule Performance Index) = {metric.SchedulePerformanceIndex}
- CPI (Cost Performance Index) = {metric.CostPerformanceIndex}

Please analyze all data and propose **5 specific and feasible recommendations** to help improve the current situation. Each recommendation must include:

1. **Goal of the recommendation** (e.g., reduce cost, speed up progress, increase efficiency, etc.)
2. **Specific root cause** of the issue (cite concrete evidence from a task/sprint/milestone)
3. **Detailed action steps** to take, such as:
    - Change which fields in which task (e.g., increase `plannedHours` for task ID `PROJA-3`)
    - Add specific personnel to which task (e.g., assign 1 developer to task `PROJA-5`)
    - Shorten duration or move a task to another sprint
    - Merge tasks or adjust task scope
    - Modify specific milestones

4. **Expected impact** of the proposed changes (e.g., ""Estimated to shorten timeline by 5 days"", ""Reduce cost by 15%"", etc.)

Return the result as a **JSON array**, where each item has the following structure:

[
  {{
    recommendation: string,         // Short description of the recommendation
    details: string,                // Explanation of the root cause and actions to take
    type: string,                   // One of: Schedule | Cost | Scope | Resource | Performance | Design | Testing
    affectedTasks: string[],        // List of affected task IDs (if any)
    suggestedTask: string,          // The task ID where action is proposed
    expectedImpact: string,         // e.g., ""Estimated to improve schedule by 5 days""
    suggestedChanges: string        // A clear description in plain English of what changes should be made, such as ""Increase planned hours of task PROJA-3 to 40 and assign one additional developer to help.""

  }}
]

**Strict Requirements:**
- The analysis **must be based on specific data** from tasks, sprints, and milestones.
- **Avoid vague or generic recommendations** without clear actions.
- **Do not** return in markdown format, and **do not** include any explanation outside the JSON.

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
                throw new Exception("Không nhận được gợi ý nào từ AI.");

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

        var prompt = $@"
You are a project control analyst. Your task is to estimate the updated key performance indicators (KPIs) of a software project after applying a set of approved changes. 

Current project metrics:
- Planned Value (PV): {currentMetric.PlannedValue}
- Earned Value (EV): {currentMetric.EarnedValue}
- Actual Cost (AC): {currentMetric.ActualCost}
- Schedule Performance Index (SPI): {currentMetric.SchedulePerformanceIndex}
- Cost Performance Index (CPI): {currentMetric.CostPerformanceIndex}

Changes Approved by Management:
- {changesDescription}

Please simulate the new metrics **after** applying the above changes. Return the result in the following JSON format:

{{
  ""SchedulePerformanceIndex"": number,    // new SPI
  ""CostPerformanceIndex"": number,        // new CPI
  ""EstimateAtCompletion"": number,        // new EAC
  ""EstimateToComplete"": number,          // new ETC
  ""VarianceAtCompletion"": number,        // new VAC = BAC - EAC
  ""EstimatedDurationAtCompletion"": number // new EDAC = DAC / SPI
}}

Rules:
- BAC (Budget at Completion) = {currentMetric.BudgetAtCompletion}
- DAC (Duration at Completion) = {currentMetric.DurationAtCompletion}
- Use the standard EVM formulas to simulate realistic values
- Output should be in JSON only. Do not include explanations or markdown.

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
