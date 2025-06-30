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

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private const string _apiKey = "AIzaSyD52tMVJMjE9GxHZwshWwobgQ8bI4rGabA";
    private readonly string _url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";

    public GeminiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
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
}
