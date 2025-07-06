using IntelliPM.Data.DTOs.Document.Request;
using IntelliPM.Data.DTOs.Document.Response;
using IntelliPM.Data.DTOs.ShareDocument.Request;
using IntelliPM.Data.DTOs.ShareDocument.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.DocumentRepos;
using IntelliPM.Services.EmailServices;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace IntelliPM.Services.DocumentServices
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _repo;
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey;
        private readonly string _geminiEndpoint;
        private readonly IEmailService _emailService;


        public DocumentService(IDocumentRepository repo, IConfiguration configuration, HttpClient httpClient, IEmailService emailService)
        {
            _repo = repo;
            _httpClient = httpClient;
            _geminiApiKey = configuration["GeminiApi:ApiKey"];
            _geminiEndpoint = configuration["GeminiApi:Endpoint"];
            _emailService = emailService;

        }

        public async Task<List<DocumentResponseDTO>> GetDocumentsByProject(int projectId)
        {
            var docs = await _repo.GetByProjectAsync(projectId);
            return docs.Select(ToResponse).ToList();
        }

        public async Task<DocumentResponseDTO> GetDocumentById(int id)
        {
            var doc = await _repo.GetByIdAsync(id);
            if (doc == null)
                throw new Exception("Document not found");

            return ToResponse(doc);
        }

        public async Task<DocumentResponseDTO> CreateDocument(DocumentRequestDTO req)
        {
            string content = req.Content;

           
            if (!string.IsNullOrWhiteSpace(req.Prompt))
            {
                var prompt = $@"
{req.Prompt}

Hãy tạo một tài liệu HTML trình bày kế hoạch dự án theo cấu trúc sau:

1. Tiêu đề chính: 📊 Project Plan with Timeline
2. Phần giới thiệu: 📅 Project Overview — gồm 1 đoạn giới thiệu ngắn.
3. Nội dung chính: gồm 4 Phase (Giai đoạn) sau. Mỗi Phase phải có tiêu đề riêng (ví dụ: Phase 1: Initiation), và dưới đó là bảng trình bày các công việc theo định dạng:

| Task | Description | Owner | Duration | Deadline | Milestone |

Mỗi Phase phải có ít nhất 3 dòng dữ liệu, với thông tin hợp lý, dễ hiểu.

4. Phần cuối: 🚀 Next Steps — danh sách các hành động tiếp theo dưới dạng danh sách gạch đầu dòng.

Yêu cầu:
- Trả về HTML đơn giản. Nếu có bảng <table>, hãy thêm <colgroup> với chiều rộng (width) cho từng cột để hỗ trợ hiển thị đẹp và resize được trong trình soạn thảo.
- Dùng các thẻ HTML đơn giản: <h1>, <h2>, <p>, <table>, <thead>, <tbody>, <tr>, <th>, <td>, <ul>, <li>.
- Cho phép sử dụng emoji như 📊, 📅, 🚀 ở phần tiêu đề nếu phù hợp.
- Nội dung cần có cấu trúc rõ ràng, dễ chèn vào editor như Tiptap.

Hãy đảm bảo HTML dễ đọc và đẹp khi render trên trình duyệt.
";

                content = await GenerateContentWithGemini(prompt);
                if (string.IsNullOrWhiteSpace(content))
                {
                    content = req.Content ?? ""; // fallback nếu Gemini không trả kết quả
                }
            }

            var doc = new Document
            {
                ProjectId = req.ProjectId,
                TaskId = req.TaskId,
                Title = req.Title,
                Type = req.Type,
                Template = req.Template,
                Content = content,
                FileUrl = req.FileUrl,
                CreatedBy = req.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            try
            {
                await _repo.AddAsync(doc);
                await _repo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("EF Save Error: " + ex.InnerException?.Message ?? ex.Message);
                throw;
            }

            return ToResponse(doc);
        }



        public async Task<DocumentResponseDTO> UpdateDocument(int id, UpdateDocumentRequest req)
        {
            var doc = await _repo.GetByIdAsync(id);
            if (doc == null) throw new Exception("Document not found");

            doc.Title = req.Title ?? doc.Title;
            doc.Content = req.Content ?? doc.Content;
            doc.FileUrl = req.FileUrl ?? doc.FileUrl;
            doc.UpdatedBy = req.UpdatedBy;
            doc.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(doc);
            await _repo.SaveChangesAsync();

            return ToResponse(doc);
        }

        public async Task<List<DocumentResponseDTO>> GetDocumentsCreatedByUser(int userId)
        {
            var docs = await _repo.GetByUserIdAsync(userId);
            return docs.Select(ToResponse).ToList();
        }

        private async Task<string?> GenerateContentWithGemini(string prompt)
        {
            try
            {
                var requestBody = new
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

                var json = JsonSerializer.Serialize(requestBody);
                Console.WriteLine("JSON gửi Gemini: " + json); // debug

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_geminiEndpoint}?key={_geminiApiKey}")
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(request);
                var responseJson = await response.Content.ReadAsStringAsync();

                Console.WriteLine("Gemini trả về: " + responseJson); // debug

                response.EnsureSuccessStatusCode();

                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                var text = root
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Gemini API error: " + ex.Message);
                return null;
            }
        }

        public async Task<string> SummarizeContent(int documentId)
        {
            var doc = await _repo.GetByIdAsync(documentId);
            if (doc == null || string.IsNullOrWhiteSpace(doc.Content))
                throw new Exception("Document not found or empty content.");

            var prompt = $@"
Bạn là một trợ lý AI. Dưới đây là một nội dung tài liệu HTML:

{doc.Content}

Hãy đọc và tóm tắt nội dung tài liệu này, giữ lại ý chính, cấu trúc dự án, và mục tiêu. Trả lời bằng văn bản thường (không phải HTML).
";

            var summary = await GenerateContentWithGemini(prompt);
            return summary ?? "Không thể tóm tắt nội dung.";
        }



        private static DocumentResponseDTO ToResponse(Document doc)
        {
            return new DocumentResponseDTO
            {
                Id = doc.Id,
                ProjectId = doc.ProjectId,
                TaskId = doc.TaskId,
                Title = doc.Title,
                Type = doc.Type,
                Template = doc.Template,
                Content = doc.Content,
                FileUrl = doc.FileUrl,
                IsActive = doc.IsActive,
                CreatedBy = doc.CreatedBy,
                UpdatedBy = doc.UpdatedBy,
                CreatedAt = doc.CreatedAt,
                UpdatedAt = doc.UpdatedAt
            };
        }
        public async Task<ShareDocumentResponseDTO> ShareDocumentByEmail(int documentId, ShareDocumentRequestDTO req)
        {
            var document = await _repo.GetByIdAsync(documentId);
            if (document == null || !document.IsActive)
                throw new Exception("Document not found");

            var failed = new List<string>();
            var link = $"https://yourdomain.com/documents/{document.Id}";

            foreach (var email in req.Emails)
            {
                try
                {
                    await _emailService.SendShareDocumentEmail(
    email,
    document.Title,
    req.Message,
    $"https://yourdomain.com/documents/{document.Id}"
);

                }
                catch
                {
                    failed.Add(email);
                }
            }

            return new ShareDocumentResponseDTO
            {
                Success = failed.Count == 0,
                FailedEmails = failed
            };
        }

        public async Task<DocumentResponseDTO> SubmitForApproval(int documentId)
        {
            var doc = await _repo.GetByIdAsync(documentId);
            if (doc == null) throw new Exception("Document not found");
            if (doc.Status != "Draft") throw new Exception("Only Draft documents can be submitted");

            doc.Status = "PendingApproval";
            doc.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(doc);
            await _repo.SaveChangesAsync();

            return ToResponse(doc);
        }

        public async Task<DocumentResponseDTO> UpdateApprovalStatus(int documentId, UpdateDocumentStatusRequest request)
        {
            var doc = await _repo.GetByIdAsync(documentId);
            if (doc == null) throw new Exception("Document not found");

            if (doc.Status != "PendingApproval") throw new Exception("Document is not waiting for approval");

            if (request.Status != "Approved" && request.Status != "Rejected")
                throw new Exception("Invalid approval status");

            doc.Status = request.Status;
            doc.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(doc);
            await _repo.SaveChangesAsync();

            return ToResponse(doc);
        }

        public async Task<List<DocumentResponseDTO>> GetDocumentsByStatus(string status)
        {
            var docs = await _repo.GetByStatusAsync(status);
            return docs.Select(ToResponse).ToList();
        }

        public async Task<List<DocumentResponseDTO>> GetDocumentsByStatusAndProject(string status, int projectId)
        {
            var docs = await _repo.GetByStatusAndProjectAsync(status, projectId);
            return docs.Select(ToResponse).ToList();
        }




    }


}