using AutoMapper;
using IntelliPM.Data.DTOs.Document.Request;
using IntelliPM.Data.DTOs.Document.Response;
using IntelliPM.Data.DTOs.ProjectMetric.Response;
using IntelliPM.Data.DTOs.ShareDocument.Request;
using IntelliPM.Data.DTOs.ShareDocument.Response;
using IntelliPM.Data.DTOs.ShareDocumentViaEmail;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Data.Enum.Document;
using IntelliPM.Repositories.DocumentPermissionRepos;
using IntelliPM.Repositories.DocumentRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Services.CloudinaryStorageServices;
using IntelliPM.Services.EmailServices;
using IntelliPM.Services.External.ProjectMetricApi;
using IntelliPM.Services.External.TaskApi;
using IntelliPM.Services.NotificationServices;
using IntelliPM.Services.ProjectMetricServices;
using IntelliPM.Services.ShareServices;
using IntelliPM.Services.TaskServices;
using IntelliPM.Shared.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace IntelliPM.Services.DocumentServices

{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _IDocumentRepository;
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey;
        private readonly string _geminiEndpoint;
        private readonly IEmailService _emailService;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDocumentPermissionRepository _permissionRepo;
        private readonly ILogger<DocumentService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<DocumentHub> _hubContext;
        private readonly IMapper _mapper;
        private readonly IShareTokenService _shareTokenService;
        private readonly IProjectMetricService _projectMetricService;
        private readonly ITaskService _taskService;
        private readonly ICloudinaryStorageService _cloudinaryStorageService;

        public DocumentService(IDocumentRepository IDocumentRepository, IConfiguration configuration, HttpClient httpClient, IEmailService emailService, IProjectMemberRepository projectMemberRepository, INotificationService notificationService, IHttpContextAccessor httpContextAccessor,
            IDocumentPermissionRepository permissionRepo, ILogger<DocumentService> logger, IHubContext<DocumentHub> hubContext, IMapper mapper, IShareTokenService shareTokenService, IProjectMetricService projectMetricService, ITaskService taskService, ICloudinaryStorageService  cloudinaryStorageService)
        {
            _IDocumentRepository = IDocumentRepository;
            _httpClient = httpClient;
            _geminiApiKey = configuration["GeminiApi:ApiKey"];
            _geminiEndpoint = configuration["GeminiApi:Endpoint"];
            _emailService = emailService;
            _projectMemberRepository = projectMemberRepository;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _permissionRepo = permissionRepo;
            _logger = logger;
            _configuration = configuration;
            _hubContext = hubContext;
            _mapper = mapper;
            _shareTokenService = shareTokenService;
            _projectMetricService = projectMetricService;
            _taskService = taskService;
            _cloudinaryStorageService = cloudinaryStorageService;
        }

        //public async Task<List<DocumentResponseDTO>> GetDocumentsByProject(int projectId)
        //{
        //    var docs = await _repo.GetByProjectAsync(projectId);
        //    return docs.Select(ToResponse).ToList();
        //}

        public async Task<List<DocumentResponseDTO>> GetDocumentsByProject(int projectId, int currentUserId)
        {
            var docs = await _IDocumentRepository.GetByProjectAsync(projectId);

            var visibleDocs = docs.Where(doc =>
                doc.Visibility == "MAIN" ||
                (doc.Visibility == "PRIVATE" && doc.CreatedBy == currentUserId)
            //(doc.Visibility == "SHAREABLE" && doc.DocumentPermission.Any(p => p.AccountId == currentUserId))
            );

            return visibleDocs.Select(ToResponse).ToList();
        }



        public async Task<List<DocumentResponseDTO>> GetAllDocuments()
        {
            var docs = await _IDocumentRepository.GetAllAsync();
            return docs.Select(d => new DocumentResponseDTO
            {
                Id = d.Id,
                ProjectId = d.ProjectId,
                TaskId = d.TaskId,
                Title = d.Title,
                //Type = d.Type,

                Content = d.Content,
                CreatedBy = d.CreatedBy,
                UpdatedBy = d.UpdatedBy,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            }).ToList();
        }

        public async Task<DocumentResponseDTO> GetByIdAsync(int id)
        {
            var entity = await _IDocumentRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException($"Document {id} not found.");
            return _mapper.Map<DocumentResponseDTO>(entity);
        }


        public async Task<DocumentResponseDTO> GetDocumentById(int id)
        {
            var doc = await _IDocumentRepository.GetByIdAsync(id);

            if (doc == null)
                throw new KeyNotFoundException($"Document {id} not found");

            return ToResponse(doc);
        }


        //public async Task<DocumentResponseDTO> CreateDocumentRequest(DocumentRequestDTO req, int userId)
        //{
        //    int count =
        //  (!string.IsNullOrWhiteSpace(req.EpicId) ? 1 : 0) +
        //  (!string.IsNullOrWhiteSpace(req.TaskId) ? 1 : 0) +
        //  (!string.IsNullOrWhiteSpace(req.SubTaskId) ? 1 : 0);

        //    if (count > 1)
        //    {
        //        throw new Exception("Document phải liên kết với duy nhất một trong: Epic, Task hoặc Subtask.");
        //    }


        //    var doc = new Document
        //    {
        //        ProjectId = req.ProjectId,
        //        EpicId = req.EpicId,
        //        TaskId = req.TaskId,
        //        SubtaskId = req.SubTaskId,
        //        Title = req.Title,
        //        //Type = req.Type,

        //        Content = req.Content,

        //        CreatedBy = userId,
        //        CreatedAt = DateTime.UtcNow,
        //        UpdatedAt = DateTime.UtcNow,
        
        //    };

        //    try
        //    {
        //        await _IDocumentRepository.AddAsync(doc);
        //        await _IDocumentRepository.SaveChangesAsync();
        //        var teamLeaders = await _projectMemberRepository.GetTeamLeaderByProjectId(doc.ProjectId);
        //        await _emailService.SendEmailTeamLeader(teamLeaders.Select(tl => tl.Account.Email).ToList(), "hello con ga");


        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("EF Save Error: " + ex.InnerException?.Message ?? ex.Message);
        //        throw new Exception("Không thể lưu Document: " + (ex.InnerException?.Message ?? ex.Message));
        //    }

        //    return ToResponse(doc);
        //}

        public async Task<DocumentResponseDTO> CreateDocument(DocumentRequestDTO req, int userId)
        {


            DocumentVisibilityEnum visibility;

            if (!Enum.TryParse<DocumentVisibilityEnum>(req.Visibility, true, out visibility))
            {
                visibility = DocumentVisibilityEnum.MAIN; 
            }



            int linkCount =
                (!string.IsNullOrWhiteSpace(req.EpicId) ? 1 : 0) +
                (!string.IsNullOrWhiteSpace(req.TaskId) ? 1 : 0) +
                (!string.IsNullOrWhiteSpace(req.SubTaskId) ? 1 : 0);

            if (linkCount > 1)
                throw new ArgumentException("Document chỉ được liên kết tối đa một trong: Epic, Task hoặc Subtask.");

            var now = DateTime.UtcNow;

            var doc = new Document
            {
                ProjectId = req.ProjectId,
                EpicId = req.EpicId,
                TaskId = req.TaskId,
                SubtaskId = req.SubTaskId,
                Title = req.Title.Trim(),
                Content = req.Content,
                CreatedBy = userId,
                UpdatedBy = userId,
                Visibility = visibility.ToString(),
                CreatedAt = now,
                UpdatedAt = now,
           
            };

            try
            {
                await _IDocumentRepository.AddAsync(doc);
                await _IDocumentRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var rootMsg = ex.InnerException?.Message ?? ex.Message;
                throw new Exception("Không thể lưu Document: " + rootMsg);
            }

            try
            {
                var content = req.Content ?? string.Empty;
                var mentionedUserIds = Regex.Matches(content, "data-id=[\"'](\\d+)[\"']", RegexOptions.IgnoreCase)
                    .Select(m => int.Parse(m.Groups[1].Value))
                    .Distinct()
                    .ToList();

                if (mentionedUserIds.Count > 0)
                {
                    await _notificationService.SendMentionNotification(
                        mentionedUserIds, doc.Id, doc.Title, userId);
                }
            }
            catch { }

            return ToResponse(doc);
        }






        public async Task<DocumentResponseDTO> UpdateDocument(int id, UpdateDocumentRequest req, int userId)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));

            var doc = await _IDocumentRepository.GetByIdAsync(id)
                      ?? throw new KeyNotFoundException("Document not found");

            if (!string.IsNullOrWhiteSpace(req.Title))
                doc.Title = req.Title.Trim();

            if (req.Content != null)
                doc.Content = req.Content;

            if (!string.IsNullOrWhiteSpace(req.Visibility.ToString()))
                doc.Visibility = req.Visibility.ToString().Trim().ToUpperInvariant();

            doc.UpdatedBy = userId;
            doc.UpdatedAt = DateTime.UtcNow;

            await _IDocumentRepository.UpdateAsync(doc);
            await _IDocumentRepository.SaveChangesAsync();

            await _hubContext.Clients
                .Group($"document-{id}")
                .SendAsync("DocumentUpdated", new
                {
                    documentId = id,
                    updatedAt = doc.UpdatedAt,
                    updatedBy = userId
                });

            try
            {
                var mentionedUserIds = Regex.Matches(doc.Content ?? string.Empty, "data-id=[\"'](\\d+)[\"']", RegexOptions.IgnoreCase)
                    .Select(m => int.Parse(m.Groups[1].Value))
                    .Distinct()
                    .ToList();

                if (mentionedUserIds.Count > 0)
                    await _notificationService.SendMentionNotification(mentionedUserIds, doc.Id, doc.Title, userId);
            }
            catch
            {
            }

            return ToResponse(doc);
        }



        public async Task<bool> DeleteDocument(int id, int deletedBy)
        {
            var doc = await _IDocumentRepository.GetByIdAsync(id);

            if (doc == null )
                throw new KeyNotFoundException($"Document {id} not found or already deleted");

     
            doc.UpdatedBy = deletedBy;
            doc.UpdatedAt = DateTime.UtcNow;

            await _IDocumentRepository.DeleteAsync(doc);
            await _IDocumentRepository.SaveChangesAsync();

            return true;
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
            var doc = await _IDocumentRepository.GetByIdAsync(documentId);
            if (doc == null || string.IsNullOrWhiteSpace(doc.Content))
                throw new Exception("Document not found or empty content.");

            var prompt = $@"
You are an AI assistant. Below is an HTML document content:

{doc.Content}

Please read and summarize this document, keeping the key points, project structure, and objectives. 
Respond in plain text (not HTML).
";

            var summary = await GenerateContentWithGemini(prompt);
            return summary ?? "Unable to summarize the content.";
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
                //Type = doc.Type,

                Content = doc.Content,
                CreatedBy = doc.CreatedBy,
                UpdatedBy = doc.UpdatedBy,
                CreatedAt = doc.CreatedAt,
                UpdatedAt = doc.UpdatedAt,
                Visibility = doc.Visibility

            };
        }

        private string GetBackendBaseUrl()
        {
            var fromConfig = _configuration["Environment:BE_URL"];
            if (!string.IsNullOrWhiteSpace(fromConfig))
                return fromConfig.TrimEnd('/');

            var req = _httpContextAccessor.HttpContext?.Request;
            if (req != null)
                return $"{req.Scheme}://{req.Host.Value}".TrimEnd('/');

            // Fallback dev
            return "https://localhost:7128";
        }


        public async Task<ShareDocumentResponseDTO> ShareDocumentByEmail(int documentId, ShareDocumentRequestDTO req)
        {
            // 1) Tìm document
            var document = await _IDocumentRepository.GetByIdAsync(documentId);
            if (document == null)
                throw new KeyNotFoundException($"Document {documentId} not found");

            // 2) Validate emails (giữ nguyên)
            var lowerInputEmails = (req.Emails ?? Enumerable.Empty<string>())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim().ToLowerInvariant())
                .Where(IsValidEmail)
                .Distinct()
                .ToList();

            if (lowerInputEmails.Count == 0)
                throw new ArgumentException("No valid emails provided.");

            // 3) Chuẩn hoá permission (giữ nguyên)
            DocumentShareEnum permissionType;
            if (!Enum.TryParse<DocumentShareEnum>(req.PermissionType, true, out permissionType))
            {
                permissionType = DocumentShareEnum.VIEW; 
            }



            // 4) Lấy account map từ emails (giữ nguyên)
            var accountMap = await _IDocumentRepository.GetAccountMapByEmailsAsync(lowerInputEmails);

            // 5) Upsert quyền cho các email có accountId (giữ nguyên)
            if (accountMap.Count > 0)
            {
                var existingPermissions = await _permissionRepo.GetByDocumentIdAsync(documentId);
                var targetAccountIds = accountMap.Values.ToHashSet();
                var toRemove = existingPermissions
                    .Where(p => targetAccountIds.Contains(p.AccountId) &&
                                string.Equals(p.PermissionType, permissionType.ToString(), StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (toRemove.Count > 0)
                    _permissionRepo.RemoveRange(toRemove);

                var newPermissions = accountMap.Values.Select(accountId => new DocumentPermission
                {
                    DocumentId = documentId,
                    AccountId = accountId,
                    PermissionType = permissionType.ToString()
                });

                await _permissionRepo.AddRangeAsync(newPermissions);
                await _permissionRepo.SaveChangesAsync();
            }

            // 6) Gửi email VỚI LINK CHỨA TOKEN CÁ NHÂN HÓA
            //var baseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var beBaseUrl = GetBackendBaseUrl();
            var knownEmails = accountMap.Keys;
            var unknownEmails = lowerInputEmails.Except(knownEmails).ToList();
            var failedToSend = new List<string>(unknownEmails);

            foreach (var email in knownEmails)
            {
                try
                {
                    // Lấy AccountId tương ứng với email
                    var accountId = accountMap[email];
                     
                    // TẠO TOKEN RIÊNG cho người dùng này
                    var token = _shareTokenService.GenerateShareToken(documentId, accountId, permissionType.ToString());


                    // TẠO LINK CHIA SẺ MỚI chứa token
                    var link = $"{beBaseUrl}/api/documents/share/verify?token={token}";

                    await _emailService.SendShareDocumentEmail(
                        email,
                        document.Title,
                        req.Message,
                        link // <-- Sử dụng link mới đã được cá nhân hóa
                    );
                }
                catch (Exception ex)
                {
                    failedToSend.Add(email);
                    _logger.LogError(ex, "Failed to send share document email to {Email}", email);
                }
            }

            // 7) Trả kết quả (giữ nguyên)
            return new ShareDocumentResponseDTO
            {
                Success = failedToSend.Count == 0,
                FailedEmails = failedToSend
            };
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Regex đơn giản: ký tự trước @, domain, và phần mở rộng
            var regex = new System.Text.RegularExpressions.Regex(
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            return regex.IsMatch(email);
        }

        private bool IsPromptValid(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt)) return false;
            if (prompt.Length < 10) return false;
            return Regex.IsMatch(prompt, @"[a-zA-ZÀ-ỹ0-9]");
        }

        private string BuildProjectPlanPrompt(string userPrompt)
        {
            return $@"
You are a professional AI assistant for generating document content.

Please respond to the following request in **complete HTML** format, using tags such as:
- <h3> for headings
- <p> for paragraphs
- <ul><li> for bullet lists
- <table><thead><tbody><tr><th><td> for tables

Return only HTML, without any additional descriptions outside.

Request:
{userPrompt}";
        }

        public async Task<string> GenerateAIContent(int documentId, string prompt)
        {
            if (!IsPromptValid(prompt))
                throw new Exception("Prompt không hợp lệ. Vui lòng mô tả rõ hơn về nội dung bạn muốn tạo.");

            var doc = await _IDocumentRepository.GetByIdAsync(documentId);
            if (doc == null) throw new Exception("Document not found");

            var fullPrompt = BuildProjectPlanPrompt(prompt);
            var content = await GenerateContentWithGemini(fullPrompt);

            //if (string.IsNullOrWhiteSpace(content) || !IsValidProjectPlanHtml(content))
            //    throw new Exception("AI không thể tạo nội dung hợp lệ từ prompt. Hãy nhập mô tả chi tiết hơn về kế hoạch dự án.");

            doc.Content = content;
            doc.UpdatedAt = DateTime.UtcNow;

            await _IDocumentRepository.UpdateAsync(doc);
            await _IDocumentRepository.SaveChangesAsync();

            return content;
        }

        public async Task<string> GenerateFreeAIContent(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt) || prompt.Length < 5)
                throw new Exception("Invalid prompt. Please enter a more specific request.");

            var htmlPrompt = $@"
You are an AI assistant specialized in generating professional document content.

Please respond to the following request in **complete HTML format**, using tags such as:
- <h3> for headings
- <p> for paragraphs
- <ul><li> for bullet lists
- <table><thead><tbody><tr><th><td> for tables

Return HTML only, do not include any external explanation or markdown.

Request:
{prompt}";

            var response = await GenerateContentWithGemini(htmlPrompt);

            if (string.IsNullOrWhiteSpace(response))
                throw new Exception("AI could not generate a response.");

            return response;
        }



        //public async Task<DocumentResponseDTO?> GetByKey(int projectId, string? epicId, string? taskId, string? subTaskId)
        //{
        //    var doc = await _repo.GetByKeyAsync(projectId, epicId, taskId, subTaskId);
        //    if (doc == null) return null;

        //    return new DocumentResponseDTO
        //    {
        //        Id = doc.Id,
        //        ProjectId = doc.ProjectId,
        //        TaskId = doc.TaskId,
        //        Title = doc.Title,
        //        //Type = doc.Type,

        //        Content = doc.Content,


        //        CreatedBy = doc.CreatedBy,
        //        UpdatedBy = doc.UpdatedBy,
        //        CreatedAt = doc.CreatedAt,
        //        UpdatedAt = doc.UpdatedAt
        //    };
        //}

        public async Task<Dictionary<string, int>> GetUserDocumentMappingAsync(int projectId, int userId)
        {
            return await _IDocumentRepository.GetUserDocumentMappingAsync(projectId, userId);
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

        public async Task<GenerateDocumentResponse> GenerateFromProject(int documentId)
        {
            var doc = await _IDocumentRepository.GetByIdAsync(documentId)
                      ?? throw new Exception("Document not found");

            var projectId = doc.ProjectId;
            var projectKey = await _IDocumentRepository.GetProjectKeyByProjectIdAsync(projectId);


            // GỌI THẲNG service/repo thay vì HTTP
            var metrics = await _projectMetricService.GetProjectHealthAsync(projectKey);
            if (metrics == null)
                throw new Exception("No project metrics found");

            var prompt = BuildSmarterTaskPrompt(metrics);

            var content = await GenerateContentWithGemini(prompt);
            if (string.IsNullOrWhiteSpace(content))
                throw new Exception("AI did not generate content");

            return new GenerateDocumentResponse { Content = content };
        }



        public async Task<GenerateDocumentResponse> GenerateFromTask(int documentId)
        {
            var doc = await _IDocumentRepository.GetByIdAsync(documentId)
                      ?? throw new Exception("Document not found");

            var projectId = doc.ProjectId;

            // ✅ gọi đúng hàm: GetTasksByProjectIdDetailed(int projectId)
            var tasks = await _taskService.GetTasksByProjectIdDetailed(projectId);
            if (tasks == null || tasks.Count == 0)
                throw new Exception("No project tasks found");

            var prompt = BuildTasksTablesPrompt(tasks);
            var content = await GenerateContentWithGemini(prompt);

            if (string.IsNullOrWhiteSpace(content))
                throw new Exception("AI did not generate content");

            return new GenerateDocumentResponse { Content = content };
        }


        private string BuildSmarterTaskPrompt(ProjectHealthDTO metrics)
        {
            // Bước 1: Định nghĩa rõ ràng các cặp Nhãn và Giá trị.
            // Dễ dàng thêm, bớt hoặc thay đổi thứ tự tại một nơi duy nhất!
            var dataPairs = new List<KeyValuePair<string, object>>
    {
        new("Project Status", metrics.ProjectStatus),
        new("Time Status", metrics.TimeStatus),
        new("Tasks To Be Completed", metrics.TasksToBeCompleted),
        new("Overdue Tasks", metrics.OverdueTasks),
        new("Progress (%)", metrics.ProgressPercent),
        new("Cost Status", metrics.CostStatus)
    };

            // Bước 2: Chuyển đổi dữ liệu thành một định dạng văn bản đơn giản, dễ hiểu cho AI.
            var dataLines = string.Join("\n", dataPairs.Select(kvp => $"{kvp.Key}: {kvp.Value ?? "N/A"}"));

            // Bước 3: Sử dụng một prompt ngắn gọn, tập trung vào nhiệm vụ chính.
            return $@"
You are an expert data formatter. Your sole purpose is to convert key-value data into a clean, semantic HTML `<table>`.

RULES:
- The final output must be ONLY the `<table>` element and its contents.
- Do not include `<html>`, `<body>`, CSS, markdown, or any explanations.
- For each line in the data, create one `<tr>`.
- The key (before the colon) goes into a `<th>`.
- The value (after the colon) goes into a `<td>`.

--- DATA ---
{dataLines}

--- HTML OUTPUT ---
";
        }



        //        private string BuildFullTaskPrompt(NewProjectMetricResponseDTO metrics, int projectId)
        //        {
        //            var json = JsonSerializer.Serialize(metrics, new JsonSerializerOptions
        //            {
        //                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //                WriteIndented = false
        //            });

        //            return $@"
        //You are an AI assistant. ONLY RETURN PURE HTML (no CSS, no markdown, no explanation) as a VERTICAL table (<table>), in which:
        //- Each row (<tr>) contains a pair of data.
        //- The first column is the field name (<th>), and the second column is the corresponding value (<td>).
        //- Do not add style or class.
        //- If a value is null, leave it empty.
        //- Values are taken from the JSON (camelCase), except Project ID which is taken from the external parameter: {projectId}.

        //JSON:
        //{json}

        //EXPECTED STRUCTURE:
        //<table>
        //  <tbody>
        //    <tr><th>Project ID</th><td>{projectId}</td></tr>
        //    <tr><th>Planned Value (PV)</th><td>{{metrics.plannedValue}}</td></tr>
        //    <tr><th>Earned Value (EV)</th><td>{{metrics.earnedValue}}</td></tr>
        //    <tr><th>Actual Cost (AC)</th><td>{{metrics.actualCost}}</td></tr>
        //    <tr><th>Budget At Completion (BAC)</th><td>{{metrics.budgetAtCompletion}}</td></tr>
        //    <tr><th>Cost Variance (CV)</th><td>{{metrics.costVariance}}</td></tr>
        //    <tr><th>Schedule Variance (SV)</th><td>{{metrics.scheduleVariance}}</td></tr>
        //    <tr><th>Cost Performance Index (CPI)</th><td>{{metrics.costPerformanceIndex}}</td></tr>
        //    <tr><th>Schedule Performance Index (SPI)</th><td>{{metrics.schedulePerformanceIndex}}</td></tr>
        //    <tr><th>Estimate At Completion (EAC)</th><td>{{metrics.estimateAtCompletion}}</td></tr>
        //    <tr><th>Estimate To Complete (ETC)</th><td>{{metrics.estimateToComplete}}</td></tr>
        //    <tr><th>Variance At Completion (VAC)</th><td>{{metrics.varianceAtCompletion}}</td></tr>
        //    <tr><th>Duration at Completion (days)</th><td>{{metrics.durationAtCompletion}}</td></tr>
        //    <tr><th>Estimate Duration at Completion (days)</th><td>{{metrics.estimateDurationAtCompletion}}</td></tr>
        //    <tr><th>Confidence Score</th><td>{{metrics.confidenceScore}}</td></tr>
        //    <tr><th>Created At (UTC)</th><td>{{metrics.createdAt}}</td></tr>
        //    <tr><th>Updated At (UTC)</th><td>{{metrics.updatedAt}}</td></tr>
        //  </tbody>
        //</table>";
        //        }




        private string BuildTasksTablesPrompt(List<TaskDetailedResponseDTO> tasks)
        {
            // Serialize to camelCase JSON so AI can read correct keys
            var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            return $@"
You are an AI assistant. ONLY RETURN PURE HTML (no CSS, no markdown, no explanations).

Requirements:
- The output must be MULTIPLE <table> elements, each task in the JSON must be printed as exactly 1 table.
- Each table should follow a vertical structure (Name/Information) as shown below.
- Do not add style or class.
- Do not reformat values, print them exactly as in JSON (if null, leave empty).
- STRICTLY DO NOT display the fields: taskAssignments, commentCount, comments, labels.
- Only use the following tags: <table>, <thead>, <tbody>, <tr>, <th>, <td>.
- Do not add any text outside of the <table> elements.

JSON data (array of tasks):
{json}

For each task in the array, output exactly 1 table following the **fixed template** below, mapping label → JSON key accordingly:

<table>
  <thead>
    <tr>
      <th>Name</th>
      <th>Information</th>
    </tr>
  </thead>
  <tbody>
    <tr><td>ID</td><td>{{task.id}}</td></tr>
    <tr><td>Project Name</td><td>{{task.projectName}}</td></tr>
    <tr><td>Type</td><td>{{task.type}}</td></tr>
    <tr><td>Title</td><td>{{task.title}}</td></tr>
    <tr><td>Description</td><td>{{task.description}}</td></tr>

    <tr><td>Planned Start Date</td><td>{{task.plannedStartDate}}</td></tr>
    <tr><td>Planned End Date</td><td>{{task.plannedEndDate}}</td></tr>
    <tr><td>Actual Start Date</td><td>{{task.actualStartDate}}</td></tr>
    <tr><td>Actual End Date</td><td>{{task.actualEndDate}}</td></tr>

    <tr><td>Created At</td><td>{{task.createdAt}}</td></tr>
    <tr><td>Updated At</td><td>{{task.updatedAt}}</td></tr>
    <tr><td>Status</td><td>{{task.status}}</td></tr>
    <tr><td>Priority</td><td>{{task.priority}}</td></tr>

    <tr><td>Reporter Fullname</td><td>{{task.reporterFullname}}</td></tr>
    <tr><td>Reporter Picture</td><td>{{task.reporterPicture}}</td></tr>

    <tr><td>Percent Complete</td><td>{{task.percentComplete}}</td></tr>
    <tr><td>Planned Hours</td><td>{{task.plannedHours}}</td></tr>
    <tr><td>Actual Hours</td><td>{{task.actualHours}}</td></tr>
    <tr><td>Remaining Hours</td><td>{{task.remainingHours}}</td></tr>
    <tr><td>Sprint Name</td><td>{{task.sprintName}}</td></tr>
    <tr><td>Evaluate</td><td>{{task.evaluate}}</td></tr>
  </tbody>
</table>";
        }

        private string? GetAccessToken()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            return authHeader?.Replace("Bearer ", "");
        }


        public async Task ShareDocumentViaEmailWithFile(ShareDocumentViaEmailRequest req)
        {
            //if (req.File == null || req.File.Length == 0)
            //{
            //    throw new BadHttpRequestException("File is required.");
            //}

            // Mở stream từ IFormFile và gọi service của bạn
            string fileUrl;
            using (var stream = req.File.OpenReadStream())
            {
                fileUrl = await _cloudinaryStorageService.UploadFileAsync(stream, req.File.FileName);
            }

            if (string.IsNullOrEmpty(fileUrl))
            {
                throw new Exception("An error occurred while uploading the file.");
            }

            var users = await _projectMemberRepository.GetAccountsByIdsAsync(req.UserIds);
            if (users == null || users.Count == 0)
                throw new Exception("No valid users found");

            var fileName = req.File.FileName;
            var subject = $"📄 You've received a new document: {fileName}";

            var fileSizeInKB = req.File.Length / 1024;
            var fileSizeDisplay = fileSizeInKB > 1024
                ? $"{(double)fileSizeInKB / 1024:F1} MB"
                : $"{fileSizeInKB} KB";

            var iconUrl = "https://img.icons8.com/pastel-glyph/64/000000/document--v1.png";

            var attachmentHtml = $@"
        <a href=""{fileUrl}"" target=""_blank"" style=""display: block; text-decoration: none; background-color: #f0f4f8; border: 1px solid #d1d9e6; border-radius: 8px; padding: 12px; margin: 16px 0; max-width: 450px;"">
          <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
            <tr>
              <td width=""40"" style=""vertical-align: middle;"">
                <img src=""{iconUrl}"" alt=""file icon"" width=""32"" height=""32"" style=""display: block;"">
              </td>
              <td style=""vertical-align: middle; padding-left: 12px; font-family: Arial, sans-serif; font-size: 14px;"">
                <strong style=""color: #0d1b2a; display: block; margin-bottom: 2px;"">{fileName}</strong>
                <span style=""color: #5f6c7b; font-size: 12px;"">{fileSizeDisplay}</span>
              </td>
            </tr>
          </table>
        </a>";

            var customMessage = req.CustomMessage ?? "You have received a new document from the IntelliPM system. Please see the link below to download:";
            var body = $"<p style='font-family: Arial, sans-serif;'>{customMessage}</p>{attachmentHtml}";

            foreach (var user in users)
            {
                if (string.IsNullOrWhiteSpace(user.Email)) continue;

                await _emailService.SendHtmlEmailAsync(
                    user.Email,
                    subject,
                    body
                );
            }
        }



        public async Task<string> GetUserPermissionLevel(int documentId, int userId)
        {
            var permission = await _permissionRepo.GetPermissionTypeAsync(documentId, userId);
            return permission?.ToLower() ?? "none";
        }

        public async Task<DocumentResponseDTO> ChangeVisibilityAsync(int documentId, ChangeVisibilityRequest request, int currentUserId)
        {
            var doc = await _IDocumentRepository.GetByIdAsync(documentId)
             ?? throw new KeyNotFoundException($"Document {documentId} not found.");

            // 2) Chỉ creator mới được đổi visibility
            if (doc.CreatedBy != currentUserId)
                throw new UnauthorizedAccessException("Only the document creator can change its visibility.");

            var updated = await _IDocumentRepository.UpdateVisibilityAsync(documentId, request.Visibility, currentUserId);
            if (!updated)
                throw new KeyNotFoundException($"Document {documentId} not found.");

            var latest = await _IDocumentRepository.GetByIdAsync(documentId)
                         ?? throw new KeyNotFoundException($"Document {documentId} not found after update.");

            //var
            var dto = new DocumentResponseDTO
            {
                Id = latest.Id,
                ProjectId = latest.ProjectId,
                TaskId = latest.TaskId,
                Title = latest.Title,
                Content = latest.Content,
                CreatedBy = latest.CreatedBy,
                UpdatedBy = latest.UpdatedBy,
                CreatedAt = latest.CreatedAt,
                UpdatedAt = latest.UpdatedAt,
                Visibility = latest.Visibility,
            };

            return dto;
        }

        public async Task<List<DocumentResponseDTO>> GetDocumentsSharedToUser(int userId)
        {
            var sharedDocs = await _permissionRepo.GetDocumentsSharedToUserAsync(userId);
            return sharedDocs.Select(ToResponse).ToList();
        }

        public async Task<List<DocumentResponseDTO>> GetDocumentsSharedToUserInProject(int userId, int projectId)
        {
            var docs = await _permissionRepo.GetDocumentsSharedToUserInProjectAsync(userId, projectId);
            return docs.Select(ToResponse).ToList();
        }

    }
}