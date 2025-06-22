using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using IntelliPM.Services.GeminiServices;

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
}
