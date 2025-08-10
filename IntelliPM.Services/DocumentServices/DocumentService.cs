using IntelliPM.Data.DTOs.Document.Request;
using IntelliPM.Data.DTOs.Document.Response;
using IntelliPM.Data.DTOs.ProjectMetric.Response;
using IntelliPM.Data.DTOs.ShareDocument.Request;
using IntelliPM.Data.DTOs.ShareDocument.Response;
using IntelliPM.Data.DTOs.ShareDocumentViaEmail;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.DocumentPermissionRepos;
using IntelliPM.Repositories.DocumentRepos;
using IntelliPM.Repositories.DocumentRepos.DocumentRepository;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Services.EmailServices;
using IntelliPM.Services.External.ProjectMetricApi;
using IntelliPM.Services.External.TaskApi;
using IntelliPM.Services.NotificationServices;
using MailKit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Ocsp;
using System.Net.Http.Json;
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
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDocumentPermissionRepository _permissionRepo;
        private readonly ILogger<DocumentService> _logger;


        public DocumentService(IDocumentRepository repo, IConfiguration configuration, HttpClient httpClient, IEmailService emailService, IProjectMemberRepository projectMemberRepository, INotificationService notificationService, IHttpContextAccessor httpContextAccessor,
            IDocumentPermissionRepository permissionRepo, ILogger<DocumentService> logger)
        {
            _repo = repo;
            _httpClient = httpClient;
            _geminiApiKey = configuration["GeminiApi:ApiKey"];
            _geminiEndpoint = configuration["GeminiApi:Endpoint"];
            _emailService = emailService;
            _projectMemberRepository = projectMemberRepository;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _permissionRepo = permissionRepo;
            _logger = logger;
        }

        //public async Task<List<DocumentResponseDTO>> GetDocumentsByProject(int projectId)
        //{
        //    var docs = await _repo.GetByProjectAsync(projectId);
        //    return docs.Select(ToResponse).ToList();
        //}

        public async Task<List<DocumentResponseDTO>> GetDocumentsByProject(int projectId, int currentUserId)
        {
            var docs = await _repo.GetByProjectAsync(projectId);

            var visibleDocs = docs.Where(doc =>
                doc.Visibility == "MAIN" ||
                (doc.Visibility == "PRIVATE" && doc.CreatedBy == currentUserId) ||
                (doc.Visibility == "SHAREABLE" && doc.DocumentPermission.Any(p => p.AccountId == currentUserId))
            );

            return visibleDocs.Select(ToResponse).ToList();
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

            var validVisibilities = new[] { "MAIN", "PRIVATE", "SHAREABLE" };

            if (!validVisibilities.Contains(req.Visibility))
            {
                throw new ArgumentException("Invalid visibility type. Must be MAIN, PRIVATE, or SHAREABLE.");
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
                Visibility = req.Visibility,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            try
            {
                await _repo.AddAsync(doc);
                await _repo.SaveChangesAsync();

                var mentionedUserIds = Regex.Matches(req.Content, "data-id=[\"'](\\d+)[\"']")
         .Select(m => int.Parse(m.Groups[1].Value))
         .Distinct()
         .ToList();
                Console.WriteLine("Mentioned IDs found: " + string.Join(",", mentionedUserIds));
                Console.WriteLine("Document Title: " + doc.Title);
                Console.WriteLine("Content: " + req.Content);

                await _notificationService.SendMentionNotification(mentionedUserIds, doc.Id, doc.Title, userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("EF Save Error: " + ex.InnerException?.Message ?? ex.Message);
                throw new Exception("Không thể lưu Document: " + (ex.InnerException?.Message ?? ex.Message));
            }

            return ToResponse(doc);
        }




        public async Task<DocumentResponseDTO> UpdateDocument(int id, UpdateDocumentRequest req, int userId)
        {
            var doc = await _repo.GetByIdAsync(id);
            if (doc == null) throw new Exception("Document not found");

            doc.Title = req.Title ?? doc.Title;
            doc.Content = req.Content ?? doc.Content;
            doc.FileUrl = req.FileUrl ?? doc.FileUrl;
            doc.UpdatedBy = userId; 
            doc.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(req.Visibility))
                doc.Visibility = req.Visibility;


            await _repo.UpdateAsync(doc);
            await _repo.SaveChangesAsync();

            var mentionedUserIds = Regex.Matches(doc.Content ?? "", "data-id=[\"'](\\d+)[\"']")
                .Select(m => int.Parse(m.Groups[1].Value))
                .Distinct()
                .ToList();

            await _notificationService.SendMentionNotification(mentionedUserIds, doc.Id, doc.Title, userId);

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
                UpdatedAt = doc.UpdatedAt,
                Visibility = doc.Visibility 

            };
        }
        //        public async Task<ShareDocumentResponseDTO> ShareDocumentByEmail(int documentId, ShareDocumentRequestDTO req)
        //        {
        //            var document = await _repo.GetByIdAsync(documentId);
        //            if (document == null || !document.IsActive)
        //                throw new Exception("Document not found");

        //            var failed = new List<string>();
        //            var link = $"https://yourdomain.com/documents/{document.Id}";

        //            foreach (var email in req.Emails)
        //            {
        //                try
        //                {
        //                    await _emailService.SendShareDocumentEmail(
        //    email,
        //    document.Title,
        //    req.Message,
        //    $"http://localhost:5173/project/projects/form/SHAREABLE/{document.Id}?projectKey=PROJC"
        //);

        //                }
        //                catch
        //                {
        //                    failed.Add(email);
        //                }
        //            }

        //            return new ShareDocumentResponseDTO
        //            {
        //                Success = failed.Count == 0,
        //                FailedEmails = failed
        //            };
        //        }

        public async Task<ShareDocumentResponseDTO> ShareDocumentByEmail(int documentId, ShareDocumentRequestDTO req)
        {
            var document = await _repo.GetByIdAsync(documentId);
            if (document == null || !document.IsActive)
                throw new Exception("Document not found");

            var failedToSend = new List<string>();
            var failedToFind = new List<string>();

            var lowerInputEmails = req.Emails?
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim().ToLower())
                .Distinct()
                .ToList() ?? new List<string>();

            if (lowerInputEmails.Count == 0)
            {
                return new ShareDocumentResponseDTO
                {
                    Success = false,
                    FailedEmails = new List<string>()
                };
            }
            var permissionType = req.PermissionType?.ToUpper() ?? "VIEW";
            var mode = permissionType == "EDIT" ? "edit" : "view";

            // 🔗 Tạo link chia sẻ
            var projectKey = string.IsNullOrWhiteSpace(req.ProjectKey) ? "DEFAULTKEY" : req.ProjectKey;
            var link = $"http://localhost:5173/project/projects/form/view/{document.Id}?projectKey={projectKey}&mode={mode}";

            // ✅ Lấy danh sách tài khoản theo email
            var accountMap = await _repo.GetAccountMapByEmailsAsync(lowerInputEmails); // Dictionary<email, accountId>
            var matchedEmails = accountMap.Keys;
            failedToFind = lowerInputEmails.Except(matchedEmails).ToList();

            if (accountMap.Count == 0)
            {
                return new ShareDocumentResponseDTO
                {
                    Success = false,
                    FailedEmails = failedToFind
                };
            }

            // 🗑 Xoá các quyền trùng loại PermissionType nếu đã tồn tại
            var existingPermissions = await _permissionRepo.GetByDocumentIdAsync(documentId);
            var toRemove = existingPermissions
                .Where(p => accountMap.Values.Contains(p.AccountId) && p.PermissionType == req.PermissionType)
                .ToList();

            _permissionRepo.RemoveRange(toRemove);

            // ➕ Thêm quyền mới
            var newPermissions = accountMap.Values.Select(accountId => new DocumentPermission
            {
                DocumentId = documentId,
                AccountId = accountId,
                PermissionType = req.PermissionType
            });

            await _permissionRepo.AddRangeAsync(newPermissions);
            await _permissionRepo.SaveChangesAsync();

            // 📧 Gửi email
            foreach (var email in matchedEmails)
            {
                try
                {
                    await _emailService.SendShareDocumentEmail(
                        email,
                        document.Title,
                        req.Message,
                        link
                    );
                }
                catch (Exception ex)
                {
                    failedToSend.Add(email);
                    _logger.LogError(ex, $"Failed to send email to {email}");
                }
            }

            // ✅ Trả kết quả
            return new ShareDocumentResponseDTO
            {
                Success = failedToFind.Count == 0 && failedToSend.Count == 0,
                FailedEmails = failedToFind.Concat(failedToSend).Distinct().ToList()
            };
        }

        //public async Task<ShareDocumentResponseDTO> ShareDocumentByEmail(int documentId, ShareDocumentRequestDTO req)
        //{
        //    var document = await _repo.GetByIdAsync(documentId);
        //    if (document == null || !document.IsActive)
        //        throw new Exception("Document not found");

        //    var failedToSend = new List<string>();
        //    var failedToFind = new List<string>();

        //    var lowerInputEmails = req.Emails?
        //        .Where(e => !string.IsNullOrWhiteSpace(e))
        //        .Select(e => e.Trim().ToLower())
        //        .Distinct()
        //        .ToList() ?? new List<string>();

        //    if (lowerInputEmails.Count == 0)
        //    {
        //        return new ShareDocumentResponseDTO
        //        {
        //            Success = false,
        //            FailedEmails = new List<string>()
        //        };
        //    }

        //    var projectKey = string.IsNullOrWhiteSpace(req.ProjectKey) ? "DEFAULTKEY" : req.ProjectKey;

        //    // ✅ Lấy danh sách tài khoản nội bộ
        //    var accountMap = await _repo.GetAccountMapByEmailsAsync(lowerInputEmails); // Dictionary<email, accountId>
        //    var matchedEmails = accountMap.Keys;
        //    failedToFind = lowerInputEmails.Except(matchedEmails).ToList();

        //    // ➕ Thêm quyền mới (chỉ cho nội bộ)
        //    if (accountMap.Count > 0)
        //    {
        //        var existingPermissions = await _permissionRepo.GetByDocumentIdAsync(documentId);
        //        var toRemove = existingPermissions
        //            .Where(p => accountMap.Values.Contains(p.AccountId) && p.PermissionType == req.PermissionType)
        //            .ToList();

        //        _permissionRepo.RemoveRange(toRemove);

        //        var newPermissions = accountMap.Values.Select(accountId => new DocumentPermission
        //        {
        //            DocumentId = documentId,
        //            AccountId = accountId,
        //            PermissionType = req.PermissionType
        //        });

        //        await _permissionRepo.AddRangeAsync(newPermissions);
        //        await _permissionRepo.SaveChangesAsync();
        //    }

        //    // 📧 Gửi email cho tất cả
        //    foreach (var email in lowerInputEmails)
        //    {
        //        try
        //        {
        //            string link;
        //            if (accountMap.TryGetValue(email, out var accountId))
        //            {
        //                // 👉 Nội bộ
        //                var routeType = req.PermissionType?.ToLower() == "view" ? "view" : "SHAREABLE";
        //                var modeParam = req.PermissionType?.ToLower() == "view" ? "&mode=view" : "";
        //                link = $"http://localhost:5173/project/projects/form/{routeType}/{document.Id}?projectKey={projectKey}{modeParam}";
        //            }
        //            else
        //            {

        //                link = $"http://localhost:5173/shared-doc/{document.Visibility}/{document.Id}?projectKey={projectKey}&mode=view";

        //            }

        //            await _emailService.SendShareDocumentEmail(
        //                email,
        //                document.Title,
        //                req.Message,
        //                link
        //            );
        //        }
        //        catch
        //        {
        //            failedToSend.Add(email);
        //        }
        //    }

        //    return new ShareDocumentResponseDTO
        //    {
        //        Success = failedToFind.Count == 0 && failedToSend.Count == 0,
        //        FailedEmails = failedToFind.Concat(failedToSend).Distinct().ToList()
        //    };
        //}





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


        public List<int> ExtractMentionedAccountIds(string content)
        {
            var mentionedIds = new List<int>();
            var matches = Regex.Matches(content, @"@(\d+)");
            foreach (Match match in matches)
            {
                if (int.TryParse(match.Groups[1].Value, out int id))
                {
                    mentionedIds.Add(id);
                }
            }
            return mentionedIds.Distinct().ToList();
        }

        public async Task<GenerateDocumentResponse> GenerateFromExistingDocument(int documentId)
        {
            var doc = await _repo.GetByIdAsync(documentId);
            if (doc == null)
                throw new Exception("Document not found");

            var projectId = doc.ProjectId;
            Console.WriteLine(projectId);
            var token = GetAccessToken();
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Access token is missing");

            // Gọi API lấy tasks có gắn token
            var taskRequest = new HttpRequestMessage(HttpMethod.Get,
                $"https://localhost:7128/api/task/by-project-id/{projectId}/detailed");
            taskRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var taskResponse = await _httpClient.SendAsync(taskRequest);
            if (!taskResponse.IsSuccessStatusCode)
                throw new Exception($"Failed to fetch tasks: {taskResponse.StatusCode}");

            var taskData = await taskResponse.Content.ReadFromJsonAsync<TaskApiResponse>();
            var tasks = taskData?.Data ?? new List<TaskDto>();
            if (!tasks.Any())
                throw new Exception("No tasks found");

            // Gọi API lấy metrics có gắn token
            var metricRequest = new HttpRequestMessage(HttpMethod.Get,
                $"https://localhost:7128/api/projectmetric/by-project-id?projectId={projectId}");
            metricRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var metricResponse = await _httpClient.SendAsync(metricRequest);
            if (!metricResponse.IsSuccessStatusCode)
                throw new Exception($"Failed to fetch metrics: {metricResponse.StatusCode}");

            var metricData = await metricResponse.Content.ReadFromJsonAsync<ProjectMetricApiResponse>();
            var metrics = metricData?.Data;
            if (metrics == null)
                throw new Exception("No project metrics found");

            // Tạo prompt từ tasks + metrics
            var prompt = BuildFullTaskPrompt(tasks, metrics, projectId);
            var content = await GenerateContentWithGemini(prompt);

            if (string.IsNullOrWhiteSpace(content))
                throw new Exception("AI did not generate content");

            return new GenerateDocumentResponse
            {
                Content = content
            };
        }




        private string BuildFullTaskPrompt(List<TaskDto> tasks, ProjectMetricResponseDTO metrics, int projectId)
        {
            var sb = new StringBuilder();

            // 🧠 System prompt với role definition rõ ràng
            sb.AppendLine("You are an expert project analyst and technical documentation specialist with extensive experience in project management and data visualization.");
            sb.AppendLine();

            sb.AppendLine("# Task: Generate Project Summary Document");
            sb.AppendLine("Create a comprehensive, professional **Project Summary Document** in **pure HTML format** that will be displayed in a WYSIWYG editor (Tiptap). The document must be well-structured, visually appealing, and provide actionable insights.");
            sb.AppendLine();

            // 📋 Cấu trúc document yêu cầu
            sb.AppendLine("## Required Document Structure:");
            sb.AppendLine("1. **Executive Summary** - Key highlights and overall project health");
            sb.AppendLine("2. **Project Metrics Dashboard** - Financial and performance indicators with visual formatting");
            sb.AppendLine("3. **Task Analysis Overview** - Summary statistics and distribution");
            sb.AppendLine("4. **Detailed Task Inventory** - Comprehensive task breakdown in table format");
            sb.AppendLine("5. **Team Performance Analysis** - Assignee workload and performance metrics");
            sb.AppendLine("6. **Risk Assessment & Insights** - Analysis of delays, budget issues, and bottlenecks");
            sb.AppendLine("7. **Recommendations & Action Items** - Specific, actionable next steps");
            sb.AppendLine();

            // 🎨 HTML formatting requirements
            sb.AppendLine("## HTML Output Requirements:");
            sb.AppendLine("- **Pure HTML only** - No markdown, code blocks, or wrapper tags");
            sb.AppendLine("- **Semantic HTML structure** - Use appropriate tags: `<h1>`, `<h2>`, `<table>`, `<ul>`, `<ol>`, `<p>`, `<strong>`, `<em>`");
            sb.AppendLine("- **Professional styling** - Use inline styles or CSS classes for visual hierarchy");
            sb.AppendLine("- **Data visualization** - Present metrics in tables with appropriate formatting");
            sb.AppendLine("- **Status indicators** - Use color coding or icons for task status (🟢 ✅ ⚠️ 🔴)");
            sb.AppendLine("- **Exclude tags** - Do NOT include `<html>`, `<head>`, `<body>`, `<script>`, or `<style>` tags");
            sb.AppendLine();

            // 📊 Project Metrics Section
            sb.AppendLine("## 📊 Project Performance Metrics");
            sb.AppendLine($"**Project ID**: {projectId} | **Analysis Date**: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine();

            sb.AppendLine("### Financial Performance:");
            sb.AppendLine($"- **Planned Value (PV)**: {FormatCurrency(metrics.PlannedValue)}");
            sb.AppendLine($"- **Earned Value (EV)**: {FormatCurrency(metrics.EarnedValue)}");
            sb.AppendLine($"- **Actual Cost (AC)**: {FormatCurrency(metrics.ActualCost)}");
            sb.AppendLine($"- **Budget Variance**: {FormatCurrency(metrics.BudgetOverrun)} {GetVarianceIndicator(metrics.BudgetOverrun)}");
            sb.AppendLine($"- **Projected Total Cost**: {FormatCurrency(metrics.ProjectedTotalCost)}");
            sb.AppendLine();

            sb.AppendLine("### Performance Indices:");
            sb.AppendLine($"- **Schedule Performance Index (SPI)**: {FormatDouble(metrics.SPI, "0.00")} {GetSPIStatus(metrics.SPI)}");
            sb.AppendLine($"- **Cost Performance Index (CPI)**: {FormatDouble(metrics.CPI, "0.00")} {GetCPIStatus(metrics.CPI)}");
            sb.AppendLine();

            sb.AppendLine("### Timeline Analysis:");
            sb.AppendLine($"- **Schedule Variance**: {FormatDelayDays(metrics.DelayDays)}");
            sb.AppendLine($"- **Projected Completion**: {FormatDate(metrics.ProjectedFinishDate)}");
            sb.AppendLine();

            sb.AppendLine("### Audit Trail:");
            sb.AppendLine($"- **Analysis Performed By**: {metrics.CalculatedBy ?? "System"}");
            sb.AppendLine($"- **Approval Status**: {(metrics.IsApproved == true ? "✅ Approved" : "⏳ Pending Approval")}");
            sb.AppendLine($"- **Created**: {FormatDateTime(metrics.CreatedAt)}");
            sb.AppendLine($"- **Last Updated**: {FormatDateTime(metrics.UpdatedAt)}");
            sb.AppendLine();

            // 📋 Task Portfolio Analysis
            sb.AppendLine("## 📋 Task Portfolio Analysis");
            sb.AppendLine($"**Total Tasks**: {tasks.Count}");

            // Task statistics
            var completedTasks = tasks.Count(t => t.PercentComplete >= 100);
            var inProgressTasks = tasks.Count(t => t.Status?.Contains("PROGRESS") == true || t.Status?.Contains("IN_PROGRESS") == true);
            var overdueTasks = tasks.Count(t => t.PlannedEndDate.HasValue && t.PlannedEndDate < DateTime.Now && t.ActualEndDate == null);
            var totalPlannedHours = tasks.Sum(t => t.PlannedHours ?? 0);
            var totalActualHours = tasks.Sum(t => t.ActualHours ?? 0);
            var totalPlannedCost = tasks.Sum(t => t.PlannedCost ?? 0);
            var totalActualCost = tasks.Sum(t => t.ActualCost ?? 0);

            sb.AppendLine($"- **Completed Tasks**: {completedTasks} ({(tasks.Count > 0 ? (completedTasks * 100.0 / tasks.Count) : 0):F1}%)");
            sb.AppendLine($"- **In Progress**: {inProgressTasks} ({(tasks.Count > 0 ? (inProgressTasks * 100.0 / tasks.Count) : 0):F1}%)");
            sb.AppendLine($"- **Overdue Tasks**: {overdueTasks} {(overdueTasks > 0 ? "⚠️" : "✅")}");
            sb.AppendLine($"- **Total Effort**: {totalPlannedHours:F1}h planned | {totalActualHours:F1}h actual");
            sb.AppendLine($"- **Total Cost**: {totalPlannedCost:C0} planned | {totalActualCost:C0} actual");
            sb.AppendLine();

            // 👥 Team Analysis
            sb.AppendLine("### 👥 Team Workload Distribution:");
            var assigneeWorkload = tasks
                .Where(t => t.TaskAssignments != null && t.TaskAssignments.Any())
                .SelectMany(t => t.TaskAssignments.Select(ta => new {
                    Name = ta.AccountFullname,
                    TaskId = t.Id,
                    TaskTitle = t.Title,
                    Status = ta.Status,
                    PlannedHours = t.PlannedHours ?? 0,
                    ActualHours = t.ActualHours ?? 0,
                    HourlyRate = ta.HourlyRate,
                    AssignedAt = ta.AssignedAt,
                    CompletedAt = ta.CompletedAt,
                    IsCompleted = ta.CompletedAt.HasValue
                }))
                .GroupBy(x => x.Name)
                .Select(g => new {
                    Name = g.Key,
                    TaskCount = g.Count(),
                    TotalPlannedHours = g.Sum(x => x.PlannedHours),
                    TotalActualHours = g.Sum(x => x.ActualHours),
                    CompletedTasks = g.Count(x => x.IsCompleted),
                    Tasks = g.ToList()
                })
                .OrderByDescending(x => x.TotalActualHours)
                .ToList();

            foreach (var assignee in assigneeWorkload)
            {
                var efficiency = assignee.TotalPlannedHours > 0 ? (assignee.TotalActualHours / assignee.TotalPlannedHours) : 0;
                var completionRate = assignee.TaskCount > 0 ? (assignee.CompletedTasks * 100.0 / assignee.TaskCount) : 0;

                sb.AppendLine($"- **{assignee.Name}**: {assignee.TaskCount} tasks | {assignee.TotalActualHours:F1}h actual | {completionRate:F1}% completion rate");
            }
            sb.AppendLine();

            // 📋 Detailed Task Information
            sb.AppendLine("## 📋 Detailed Task Information");
            foreach (var task in tasks.OrderBy(t => t.Id))
            {
                sb.AppendLine($"### 📋 Task #{task.Id}: {task.Title}");
                sb.AppendLine();

                // Basic Information
                sb.AppendLine("**📝 Basic Information:**");
                sb.AppendLine($"- **Project**: {task.ProjectName ?? "N/A"} (ID: {task.ProjectId})");
                sb.AppendLine($"- **Task ID**: {task.Id}");
                sb.AppendLine($"- **Title**: {task.Title ?? "No title"}");
                sb.AppendLine($"- **Description**: {task.Description ?? "No description provided"}");
                sb.AppendLine($"- **Type**: {GetTaskTypeWithIcon(task.Type)} | **Priority**: {GetPriorityWithIcon(task.Priority)}");
                sb.AppendLine($"- **Epic Reference**: {task.EpicId ?? "None"}");
                sb.AppendLine();

                // Reporter Information
                sb.AppendLine("**👤 Reporter Information:**");
                sb.AppendLine($"- **Reporter**: {task.ReporterFullname ?? "Unknown"} (ID: {task.ReporterId})");
                if (!string.IsNullOrEmpty(task.ReporterPicture))
                {
                    sb.AppendLine($"- **Profile Picture**: Available");
                }
                sb.AppendLine();

                // Sprint Assignment
                sb.AppendLine("**🏃 Sprint Assignment:**");
                sb.AppendLine($"- **Sprint**: {task.SprintName ?? "Unassigned"} {(task.SprintId.HasValue ? $"(#{task.SprintId})" : "")}");
                sb.AppendLine();

                // Progress Tracking
                sb.AppendLine("**📊 Progress Tracking:**");
                sb.AppendLine($"- **Current Status**: {GetStatusWithIcon(task.Status)}");
                sb.AppendLine($"- **Completion Percentage**: {task.PercentComplete?.ToString("0.##") ?? "0"}%");
                sb.AppendLine($"- **Quality Assessment**: {task.Evaluate ?? "Pending evaluation"}");
                sb.AppendLine();

                // Timeline Information
                sb.AppendLine("**⏱️ Timeline Information:**");
                sb.AppendLine($"- **Planned Period**: {FormatDateRange(task.PlannedStartDate, task.PlannedEndDate)}");
                sb.AppendLine($"- **Actual Period**: {FormatDateRange(task.ActualStartDate, task.ActualEndDate)}");
                sb.AppendLine($"- **Schedule Status**: {GetScheduleStatus(task.PlannedEndDate, task.ActualEndDate)}");
                sb.AppendLine($"- **Created**: {FormatDateTime(task.CreatedAt)}");
                sb.AppendLine($"- **Last Updated**: {FormatDateTime(task.UpdatedAt)}");
                sb.AppendLine();

                // Resource Planning
                sb.AppendLine("**💰 Resource Planning:**");
                sb.AppendLine($"- **Time Budget**: {FormatHours(task.PlannedHours)} planned | {FormatHours(task.ActualHours)} actual | {FormatHours(task.RemainingHours)} remaining");
                sb.AppendLine($"- **Task Cost**: {FormatCurrency(task.PlannedCost)} planned | {FormatCurrency(task.ActualCost)} actual");
                sb.AppendLine($"- **Resource Cost**: {FormatCurrency(task.PlannedResourceCost)} planned | {FormatCurrency(task.ActualResourceCost)} actual");

                if (task.PlannedHours.HasValue && task.ActualHours.HasValue && task.PlannedHours > 0)
                {
                    var efficiency = (task.ActualHours.Value / task.PlannedHours.Value) * 100;
                    var efficiencyStatus = efficiency <= 100 ? "✅ Efficient" : efficiency <= 120 ? "⚠️ Slightly Over" : "🔴 Over Budget";
                    sb.AppendLine($"- **Time Efficiency**: {efficiency:F1}% {efficiencyStatus}");
                }
                sb.AppendLine();

                // Team Assignment Details
                sb.AppendLine("**👥 Team Assignment:**");
                if (task.TaskAssignments != null && task.TaskAssignments.Any())
                {
                    foreach (var assignment in task.TaskAssignments)
                    {
                        sb.AppendLine($"- **Assignee**: {assignment.AccountFullname}");
                        sb.AppendLine($"  - **Status**: {GetStatusWithIcon(assignment.Status)}");
                        sb.AppendLine($"  - **Assigned Date**: {FormatDateTime(assignment.AssignedAt)}");
                        if (assignment.CompletedAt.HasValue && assignment.AssignedAt.HasValue)
                        {
                            sb.AppendLine($"  - **Completed Date**: {FormatDateTime(assignment.CompletedAt.Value)}");
                            var workDuration = (assignment.CompletedAt.Value - assignment.AssignedAt.Value).Days;
                            sb.AppendLine($"  - **Work Duration**: {workDuration} days");
                        }
                        else if (assignment.CompletedAt.HasValue)
                        {
                            sb.AppendLine($"  - **Completed Date**: {FormatDateTime(assignment.CompletedAt.Value)}");
                        }
                        if (assignment.HourlyRate.HasValue)
                        {
                            sb.AppendLine($"  - **Hourly Rate**: {assignment.HourlyRate:C}/hour");
                            if (task.ActualHours.HasValue)
                            {
                                var estimatedCost = assignment.HourlyRate.Value * task.ActualHours.Value;
                                sb.AppendLine($"  - **Estimated Labor Cost**: {estimatedCost:C}");
                            }
                        }
                        if (!string.IsNullOrEmpty(assignment.AccountPicture))
                        {
                            sb.AppendLine($"  - **Profile Picture**: Available");
                        }
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("- **Assignment Status**: ⚠️ Unassigned");
                    sb.AppendLine();
                }

                sb.AppendLine("---");
                sb.AppendLine();
            }

            // Final instructions for AI
            sb.AppendLine("## 📝 Analysis Instructions");
            sb.AppendLine("Please analyze the above data and create a professional project summary document that:");
            sb.AppendLine("- Identifies critical project risks, bottlenecks, and resource constraints");
            sb.AppendLine("- Highlights successful areas, efficient team members, and on-track deliverables");
            sb.AppendLine("- Analyzes team workload distribution and identifies potential burnout risks");
            sb.AppendLine("- Provides specific, actionable recommendations for project improvement");
            sb.AppendLine("- Uses appropriate visual formatting, status indicators, and professional presentation");
            sb.AppendLine("- Maintains professional tone suitable for stakeholder and executive review");
            sb.AppendLine("- Include data-driven insights and quantitative analysis where possible");

            return sb.ToString();
        }

        // Helper Methods
        private string FormatCurrency(decimal? value)
        {
            return value?.ToString("C0") ?? "Not specified";
        }

        private string FormatCurrency(double? value)
        {
            return value?.ToString("C0") ?? "Not specified";
        }

        private string FormatDecimal(decimal? value, string format)
        {
            return value?.ToString(format) ?? "N/A";
        }

        private string FormatDouble(double? value, string format)
        {
            return value?.ToString(format) ?? "N/A";
        }

        private string FormatDate(DateTime? date)
        {
            return date?.ToString("yyyy-MM-dd") ?? "Not determined";
        }

        private string FormatDateTime(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm");
        }

        private string FormatDateTime(DateTime? date)
        {
            return date?.ToString("yyyy-MM-dd HH:mm") ?? "Not specified";
        }

        private string FormatHours(decimal? hours)
        {
            return hours?.ToString("0.##") + "h" ?? "Not specified";
        }

        private string FormatHours(double? hours)
        {
            return hours?.ToString("0.##") + "h" ?? "Not specified";
        }

        private string FormatDelayDays(int? days)
        {
            if (!days.HasValue) return "On schedule ✅";
            return days.Value > 0 ? $"{days} days behind schedule ⚠️" :
                   days.Value < 0 ? $"{Math.Abs(days.Value)} days ahead of schedule ✅" :
                   "On schedule ✅";
        }

        private string GetVarianceIndicator(decimal? variance)
        {
            if (!variance.HasValue) return "";
            return variance.Value > 0 ? "⚠️ Over budget" :
                   variance.Value < 0 ? "✅ Under budget" :
                   "✅ On budget";
        }

        private string GetVarianceIndicator(double? variance)
        {
            if (!variance.HasValue) return "";
            return variance.Value > 0 ? "⚠️ Over budget" :
                   variance.Value < 0 ? "✅ Under budget" :
                   "✅ On budget";
        }

        private string GetSPIStatus(decimal? spi)
        {
            if (!spi.HasValue) return "";
            return spi.Value >= 1.0m ? "✅ On/Ahead Schedule" :
                   spi.Value >= 0.9m ? "⚠️ Slightly Behind" :
                   "🔴 Significantly Behind";
        }

        private string GetSPIStatus(double? spi)
        {
            if (!spi.HasValue) return "";
            return spi.Value >= 1.0 ? "✅ On/Ahead Schedule" :
                   spi.Value >= 0.9 ? "⚠️ Slightly Behind" :
                   "🔴 Significantly Behind";
        }

        private string GetCPIStatus(decimal? cpi)
        {
            if (!cpi.HasValue) return "";
            return cpi.Value >= 1.0m ? "✅ On/Under Budget" :
                   cpi.Value >= 0.9m ? "⚠️ Slightly Over Budget" :
                   "🔴 Significantly Over Budget";
        }

        private string GetCPIStatus(double? cpi)
        {
            if (!cpi.HasValue) return "";
            return cpi.Value >= 1.0 ? "✅ On/Under Budget" :
                   cpi.Value >= 0.9 ? "⚠️ Slightly Over Budget" :
                   "🔴 Significantly Over Budget";
        }

        private string GetStatusWithIcon(string status)
        {
            if (string.IsNullOrEmpty(status)) return "❓ Unknown";
            var upperStatus = status.ToUpper();
            return upperStatus switch
            {
                "COMPLETED" or "DONE" => $"✅ {status}",
                "IN_PROGRESS" or "ACTIVE" => $"🔄 {status}",
                "BLOCKED" or "STOPPED" => $"🔴 {status}",
                "PENDING" or "WAITING" => $"⏳ {status}",
                "TODO" or "NEW" => $"📋 {status}",
                _ => $"📋 {status}"
            };
        }

        private string GetTaskTypeWithIcon(string type)
        {
            if (string.IsNullOrEmpty(type)) return "📋 Standard";
            return type.ToUpper() switch
            {
                "STORY" => $"📖 {type}",
                "BUG" => $"🐛 {type}",
                "TASK" => $"📋 {type}",
                "EPIC" => $"🎯 {type}",
                "FEATURE" => $"⭐ {type}",
                _ => $"📋 {type}"
            };
        }

        private string GetPriorityWithIcon(string priority)
        {
            if (string.IsNullOrEmpty(priority)) return "📊 Normal";
            return priority.ToUpper() switch
            {
                "HIGH" or "URGENT" => $"🔴 {priority}",
                "MEDIUM" => $"🟡 {priority}",
                "LOW" => $"🟢 {priority}",
                "CRITICAL" => $"⚡ {priority}",
                _ => $"📊 {priority}"
            };
        }

        private string FormatDateRange(DateTime? start, DateTime? end)
        {
            if (!start.HasValue && !end.HasValue) return "Not scheduled";
            if (!start.HasValue) return $"End: {FormatDate(end)}";
            if (!end.HasValue) return $"Start: {FormatDate(start)}";
            return $"{FormatDate(start)} → {FormatDate(end)}";
        }

        private string GetScheduleStatus(DateTime? plannedEnd, DateTime? actualEnd)
        {
            if (!plannedEnd.HasValue) return "⚪ No planned end date";
            if (!actualEnd.HasValue)
            {
                return DateTime.Now > plannedEnd.Value ? "🔴 Overdue" : "🔄 In progress";
            }
            return actualEnd.Value <= plannedEnd.Value ? "✅ Completed on time" : "⚠️ Completed late";
        }


        private string? GetAccessToken()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            return authHeader?.Replace("Bearer ", "");
        }

        //public async Task ShareDocumentViaEmail(ShareDocumentViaEmailRequest req)
        //{
        //    var document = await _repo.GetByIdAsync(req.DocumentId);
        //    if (document == null) throw new Exception("Document not found");

        //    var users = await _projectMemberRepository.GetAccountsByIdsAsync(req.UserIds);
        //    foreach (var user in users)
        //    {
        //        if (string.IsNullOrWhiteSpace(user.Email)) continue;

        //        var subject = $"📄 Bạn nhận được tài liệu: {document.Title}";
        //        var body = req.CustomMessage ?? $"Tài liệu \"{document.Title}\" đã được chia sẻ với bạn.";

        //        // Đính kèm link file trong nội dung
        //        body += $"\n\n📎 File: {req.FileUrl}";

        //        await _emailService.SendDocumentShareEmailMeeting(user.Email, document.Title, req.CustomMessage ?? "", req.FileUrl);

        //    }
        //}

        public async Task ShareDocumentViaEmailWithFile(ShareDocumentViaEmailRequest req)
        {
            var users = await _projectMemberRepository.GetAccountsByIdsAsync(req.UserIds);
            if (users == null || users.Count == 0)
                throw new Exception("No valid users found");

            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await req.File.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            var fileName = req.File.FileName;
            var subject = $"📄 Bạn nhận được tài liệu mới";
            var body = req.CustomMessage ?? $"Bạn nhận được một tài liệu mới từ hệ thống IntelliPM.";

            foreach (var user in users)
            {
                if (string.IsNullOrWhiteSpace(user.Email)) continue;

                await _emailService.SendDocumentShareEmailMeeting(
                    user.Email,
                    subject,
                    body,
                    fileBytes,
                    fileName
                );
            }
        }

        public async Task<string> GetUserPermissionLevel(int documentId, int userId)
        {
            var permission = await _permissionRepo.GetPermissionTypeAsync(documentId, userId);
            return permission?.ToLower() ?? "none";
        }














    }
}