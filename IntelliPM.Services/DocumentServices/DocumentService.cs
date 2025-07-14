using Google.Cloud.Storage.V1;
using IntelliPM.Data.DTOs.Document.Request;
using IntelliPM.Data.DTOs.Document.Response;
using IntelliPM.Data.DTOs.ShareDocument.Request;
using IntelliPM.Data.DTOs.ShareDocument.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.DocumentRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Services.EmailServices;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace IntelliPM.Services.DocumentServices
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _repo;
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey;
        private readonly string _geminiEndpoint;
        private readonly IEmailService _emailService;
        private readonly IProjectMemberRepository _projectMemberRepository;

        public DocumentService(IDocumentRepository repo, IConfiguration configuration, HttpClient httpClient, IEmailService emailService, IProjectMemberRepository projectMemberRepository)
        {
            _repo = repo;
            _httpClient = httpClient;
            _geminiApiKey = configuration["GeminiApi:ApiKey"];
            _geminiEndpoint = configuration["GeminiApi:Endpoint"];
            _emailService = emailService;
            _projectMemberRepository = projectMemberRepository;
        }

        public async Task<List<DocumentResponseDTO>> GetDocumentsByProject(int projectId)
        {
            var docs = await _repo.GetByProjectAsync(projectId);
            return docs.Select(ToResponse).ToList();
        }

        public async Task<List<DocumentResponseDTO>> GetAllDocuments()
        {
            var docs = await _repo.GetAllAsync();
            return docs.Select(d => new DocumentResponseDTO
            {
                Id = d.Id,
                ProjectId = d.ProjectId,
                TaskId = d.TaskId,
                Title = d.Title,
                Type = d.Type,
                Template = d.Template,
                Content = d.Content,
                FileUrl = d.FileUrl,
                IsActive = d.IsActive,
                CreatedBy = d.CreatedBy,
                UpdatedBy = d.UpdatedBy,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            }).ToList();
        }


        public async Task<DocumentResponseDTO> GetDocumentById(int id)
        {
            var doc = await _repo.GetByIdAsync(id);
            if (doc == null)
                throw new Exception("Document not found");

            return ToResponse(doc);
        }

        public async Task<DocumentResponseDTO> CreateDocumentRequest(DocumentRequestDTO req, int userId)
        {
            int count =
          (!string.IsNullOrWhiteSpace(req.EpicId) ? 1 : 0) +
          (!string.IsNullOrWhiteSpace(req.TaskId) ? 1 : 0) +
          (!string.IsNullOrWhiteSpace(req.SubTaskId) ? 1 : 0);

            if (count > 1)
            {
                throw new Exception("Document phải liên kết với duy nhất một trong: Epic, Task hoặc Subtask.");
            }


            var doc = new Document
            {
                ProjectId = req.ProjectId,
                EpicId = req.EpicId,
                TaskId = req.TaskId,
                SubtaskId = req.SubTaskId,
                Title = req.Title,
                Type = req.Type,
                Template = req.Template,
                Content = req.Content,
                FileUrl = req.FileUrl,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                Status = "PendingApproval"
            };

            try
            {
                await _repo.AddAsync(doc);
                await _repo.SaveChangesAsync();
                var teamLeaders = await _projectMemberRepository.GetTeamLeaderByProjectId(doc.ProjectId);
                await _emailService.SendEmailTeamLeader(teamLeaders.Select(tl => tl.Account.Email).ToList(), "hello con ga");
      

            }
            catch (Exception ex)
            {
                Console.WriteLine("EF Save Error: " + ex.InnerException?.Message ?? ex.Message);
                throw new Exception("Không thể lưu Document: " + (ex.InnerException?.Message ?? ex.Message));
            }

            return ToResponse(doc);
        }

        public async Task<DocumentResponseDTO> CreateDocument(DocumentRequestDTO req, int userId)
        {
            int count =
          (!string.IsNullOrWhiteSpace(req.EpicId) ? 1 : 0) +
          (!string.IsNullOrWhiteSpace(req.TaskId) ? 1 : 0) +
          (!string.IsNullOrWhiteSpace(req.SubTaskId) ? 1 : 0);

            if (count > 1)
            {
                throw new Exception("Document phải liên kết với duy nhất một trong: Epic, Task hoặc Subtask.");
            }


            var doc = new Document
            {
                ProjectId = req.ProjectId,
                EpicId = req.EpicId,
                TaskId = req.TaskId,
                SubtaskId = req.SubTaskId,
                Title = req.Title,
                Type = req.Type,
                Template = req.Template,
                Content = req.Content,
                FileUrl = req.FileUrl,
                CreatedBy = userId,
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
                throw new Exception("Không thể lưu Document: " + (ex.InnerException?.Message ?? ex.Message));
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
                EpicId = doc.EpicId,
                TaskId = doc.TaskId,
                SubtaskId = doc.SubtaskId,
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

        private bool IsPromptValid(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt)) return false;
            if (prompt.Length < 10) return false;
            return Regex.IsMatch(prompt, @"[a-zA-ZÀ-ỹ0-9]");
        }

        private bool IsValidProjectPlanHtml(string content)
        {
            return content.Contains("<h1>📊 Project Plan") && content.Contains("<table");
        }

        private string BuildProjectPlanPrompt(string userPrompt)
        {
            return $@"
Bất kể yêu cầu người dùng bên dưới là gì, bạn cần **bỏ qua nội dung không liên quan** và luôn sinh ra một **tài liệu kế hoạch dự án (Project Plan)** có cấu trúc HTML rõ ràng như sau:

1. Tiêu đề chính: `<h1>📊 Project Plan with Timeline</h1>`

2. Phần giới thiệu:  
   `<h2>📅 Project Overview</h2>`  
   Một đoạn mô tả ngắn về mục tiêu và phạm vi dự án trong thẻ `<p>`.

3. Các giai đoạn (Phases):  
   Sinh **4 phase** tương ứng với 4 `<section>`, mỗi phase gồm:
   - Tiêu đề `<h2>Phase X: [Tên Phase]</h2>`
   - Một bảng `<table>` có đầy đủ các cột:
     | Task | Description | Owner | Duration (Days) | Deadline | Milestone |

   ⚠️ Yêu cầu bảng phải có:
   - Thẻ `<colgroup>` với các `<col style=""width: ..."">` để hiển thị rõ cấu trúc
   - Các ô tiêu đề `<th>` cần có thuộc tính `colwidth=""...""` để hỗ trợ kéo giãn cột trong trình soạn thảo như Tiptap

4. Phần kết:  
   `<h2>🚀 Next Steps</h2>`  
   Một danh sách `<ul>` các bước tiếp theo để triển khai dự án.

📌 Ghi nhớ:
- Trả về **HTML đơn giản** (dùng `<h1>`, `<h2>`, `<table>`, `<ul>`, `<section>`, `<p>`)
- **Không bao quanh bằng \`\`\`html** hoặc bất kỳ markdown nào
- Nếu yêu cầu bên dưới không hợp lệ, vẫn phải sinh đúng cấu trúc tài liệu như mô tả
- Đảm bảo HTML dễ hiển thị trong trình soạn thảo văn bản, và không chứa script hoặc style thừa

🔽 Dưới đây là yêu cầu người dùng:  
""Viết kế hoạch dự án phát triển hệ thống quản lý nhân sự cho doanh nghiệp vừa và nhỏ""
""{userPrompt}""
";

        }
        public async Task<string> GenerateAIContent(int documentId, string prompt)
        {
            if (!IsPromptValid(prompt))
                throw new Exception("Prompt không hợp lệ. Vui lòng mô tả rõ hơn về nội dung bạn muốn tạo.");

            var doc = await _repo.GetByIdAsync(documentId);
            if (doc == null) throw new Exception("Document not found");

            var fullPrompt = BuildProjectPlanPrompt(prompt);
            var content = await GenerateContentWithGemini(fullPrompt);

            if (string.IsNullOrWhiteSpace(content) || !IsValidProjectPlanHtml(content))
                throw new Exception("AI không thể tạo nội dung hợp lệ từ prompt. Hãy nhập mô tả chi tiết hơn về kế hoạch dự án.");

            doc.Content = content;
            doc.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(doc);
            await _repo.SaveChangesAsync();

            return content;
        }

        public async Task<string> GenerateFreeAIContent(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt) || prompt.Length < 5)
                throw new Exception("Prompt không hợp lệ. Vui lòng nhập nội dung rõ ràng hơn.");

            var response = await GenerateContentWithGemini(prompt);

            if (string.IsNullOrWhiteSpace(response))
                throw new Exception("AI không thể trả lời yêu cầu.");

            return response;
        }

        public async Task<DocumentResponseDTO?> GetByKey(int projectId, string? epicId, string? taskId, string? subTaskId)
        {
            var doc = await _repo.GetByKeyAsync(projectId, epicId, taskId, subTaskId);
            if (doc == null) return null;

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

        public async Task<Dictionary<string, int>> GetUserDocumentMappingAsync(int projectId, int userId)
        {
            return await _repo.GetUserDocumentMappingAsync(projectId, userId);
        }
        public async Task<Dictionary<string, int>> GetStatusCount()
        {
            return await _repo.CountByStatusAsync();
        }

        public async Task<Dictionary<string, int>> GetStatusCountByProject(int projectId)
        {
            return await _repo.CountByStatusInProjectAsync(projectId);
        }




    }
}