using AutoMapper;
using CloudinaryDotNet;
using IntelliPM.Data.DTOs.ProjectMetric.Request;
using IntelliPM.Data.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace IntelliPM.Services.ChatGPTServices
{
    public class ChatGPTService : IChatGPTService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly IMapper _mapper;

        public ChatGPTService(HttpClient httpClient, IMapper mapper, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _mapper = mapper;
            _apiKey = configuration["OpenAI:ApiKey"]!;
            _apiUrl = configuration["OpenAI:ApiUrl"]!;
        }

    //    public async Task<ProjectMetricRequestDTO> CalculateProjectMetricsAsync(Project project, List<Tasks> tasks)
    //    {
    //        var taskList = JsonConvert.SerializeObject(tasks.Select(t => new
    //        {
    //            t.Title,
    //            t.Description,
    //            t.PlannedStartDate,
    //            t.PlannedEndDate,
    //            t.ActualStartDate,
    //            t.ActualEndDate,
    //            t.PercentComplete,
    //            t.PlannedHours,
    //            t.ActualHours,
    //            t.PlannedCost,
    //            t.ActualCost,
    //            t.Status
    //        }), Formatting.Indented);

    //        var prompt = $@"
    //        Bạn là một chuyên gia quản lý dự án. Dưới đây là thông tin dự án và danh sách các task. 

    //        Hãy thực hiện:
    //        1. Hãy trả về kết quả dưới dạng một JSON object, bao gồm đầy đủ tất cả các trường sau (không được thiếu bất kỳ trường nào, và đúng định dạng):

    //            - plannedValue: Tổng giá trị kế hoạch của các công việc đã lên lịch tính đến hiện tại.
    //            - earnedValue: Tổng giá trị kiếm được theo % hoàn thành của từng task.
    //            - actualCost: Tổng chi phí thực tế.
    //            - spi: Schedule Performance Index = EV / PV
    //            - cpi: Cost Performance Index = EV / AC
    //            - delayDays: Số ngày dự án đang trễ so với kế hoạch (nếu có).
    //            - budgetOverrun: Chi phí vượt ngân sách = AC - PV
    //            - projectedFinishDate: Ngày kết thúc dự kiến nếu giữ nguyên tốc độ hiện tại (định dạng: yyyy-MM-ddTHH:mm:ssZ), tính bằng Project.StartDate + EDAC  (EDAC = DAC/SPI là Ước lượng tổng thời gian thực tế để hoàn thành)
    //            - projectedTotalCost: Tổng chi phí ước tính để hoàn thành toàn bộ dự án (EAC = BAC / CPI nếu CPI hiện tại giữ nguyên)

    //        **Yêu cầu:** chỉ trả về đúng JSON, không giải thích, không thêm chữ, không định dạng Markdown.

    //        2. Nếu phát hiện:
    //        - SPI < 1 → dự án đang chậm tiến độ
    //        - CPI < 1 → dự án vượt chi phí

    //        Hãy thêm vào JSON một trường 'suggestions' là mảng các giải pháp cải thiện. Mỗi phần tử trong 'suggestions' cần gồm:
    //        - message: gợi ý hành động cụ thể
    //        - reason: lý do đưa ra gợi ý này
    //        - label: Từ khóa ngắn gọn mô tả loại gợi ý (ví dụ: “Tiến độ”, “Chi phí”)
    //         - relatedTasks: nếu có, là mảng task liên quan — mỗi task chỉ xuất hiện **một lần duy nhất** trong toàn bộ danh sách suggestions.
    //        Cấu trúc mỗi phần tử trong relatedTasks:
    //        - taskTitle
    //        - currentPlannedEndDate
    //        - currentPercentComplete
    //        - suggestedAction: hành động cụ thể cần thực hiện (VD: tăng nhân lực, kéo dài thời hạn như thế nào ...)

    //        **Ràng buộc nghiêm ngặt:**
    //        - Mỗi task chỉ được xuất hiện **một lần** trong toàn bộ suggestions. Nếu có nhiều đề xuất áp dụng cho một task, hãy gộp lại thành một entry.
    //        - Nếu không có task liên quan, **bỏ qua trường relatedTasks**.
    //        - Trả về đúng JSON thuần, không markdown, không giải thích.

    //        Thông tin dự án:
    //        - Tên: {project.Name}
    //        - Ngân sách: {project.Budget}
    //        - Thời gian bắt đầu: {project.StartDate}
    //        - Thời gian kết thúc: {project.EndDate}

    //        Danh sách task:
    //        {taskList}
    //        ";

    //        var requestData = new
    //        {
    //            model = "gpt-4", // hoặc "gpt-4o" nếu muốn nhanh hơn
    //            messages = new[]
    //{
    //    new { role = "system", content = "You are a helpful assistant." },
    //    new { role = "user", content = prompt }
    //},
    //            temperature = 0.3
    //        };

    //        var requestJson = JsonConvert.SerializeObject(requestData);
    //        var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, _apiUrl) ;
    //        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
    //        request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");


    //        var response = await _httpClient.SendAsync(request);
    //        var responseString = await response.Content.ReadAsStringAsync();

    //        if (!response.IsSuccessStatusCode)
    //            throw new Exception($"Gemini API Error: {response.StatusCode}\nResponse: {responseString}");

    //        if (string.IsNullOrWhiteSpace(responseString))
    //            throw new Exception("Gemini response is empty.");

    //        var parsedResponse = JsonConvert.DeserializeObject<OpenAIChatResponse>(responseString);
    //        var replyText = parsedResponse?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

    //        if (string.IsNullOrEmpty(replyText))
    //            throw new Exception("Gemini did not return any text response.");

    //        if (replyText.StartsWith("```") && replyText.Contains("json"))
    //        {
    //            replyText = replyText.Replace("```json", "").Replace("```", "").Trim();
    //        }

    //        if (!replyText.StartsWith("{"))
    //            throw new Exception("Gemini reply is not a valid JSON object:\n" + replyText);

    //        try
    //        {
    //            var result = JsonConvert.DeserializeObject<ProjectMetricRequestDTO>(replyText);
    //            result.ProjectId = project.Id;
    //            result.CalculatedBy = "AI";
    //            return result;
    //        }
    //        catch (Exception ex)
    //        {
    //            throw new Exception("Error parsing ProjectMetricRequestDTO from Gemini reply:\n" + replyText + "\n" + ex.Message);
    //        }
    //    }
    }

    public class OpenAIChatResponse
    {
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        public Message Message { get; set; }
    }

    public class Message
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

}
