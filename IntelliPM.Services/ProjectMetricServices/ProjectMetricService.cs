using AutoMapper;
using IntelliPM.Data.DTOs.ProjectMetric.Request;
using IntelliPM.Data.DTOs.ProjectMetric.Response;
using IntelliPM.Data.Entities;
using EntityTask = IntelliPM.Data.Entities.Tasks;
using IntelliPM.Repositories.ProjectMetricRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.TaskRepos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace IntelliPM.Services.ProjectMetricServices
{
    public class ProjectMetricService : IProjectMetricService
    {
        private readonly IMapper _mapper;
        private readonly IProjectMetricRepository _repo;
        private readonly ITaskRepository _taskRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly ILogger<ProjectMetricService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public ProjectMetricService(IMapper mapper, IProjectMetricRepository repo, IProjectRepository projectRepo, ITaskRepository taskRepo, ILogger<ProjectMetricService> logger, HttpClient httpClient, IConfiguration config)
        {
            _mapper = mapper;
            _repo = repo;
            _projectRepo = projectRepo;
            _taskRepo = taskRepo;
            _logger = logger;
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<ProjectMetricResponseDTO> CalculateAndSaveMetricsAsync(int projectId, string calculatedBy)
        {
            var tasks = await _taskRepo.GetByProjectIdAsync(projectId);
            var today = DateTime.UtcNow;

            // Planned Value (PV): Tổng PlannedCost của các task kết thúc đến thời điểm hiện tại
            var plannedTasks = tasks.Where(t => t.PlannedEndDate.HasValue && t.PlannedEndDate.Value <= today);
            decimal plannedValue = plannedTasks.Sum(t => t.PlannedCost ?? 0);

            // Earned Value (EV): Tổng (PlannedCost * % hoàn thành)
            decimal earnedValue = tasks.Sum(t => (t.PlannedCost ?? 0) * (decimal)((t.PercentComplete ?? 0) / 100));

            // Actual Cost (AC): Tổng ActualCost
            decimal actualCost = tasks.Sum(t => t.ActualCost ?? 0);

            // SPI = EV / PV
            double? spi = plannedValue == 0 ? null : (double?)Math.Round((double)earnedValue / (double)plannedValue, 2);

            // CPI = EV / AC
            double? cpi = actualCost == 0 ? null : (double?)Math.Round((double)earnedValue / (double)actualCost, 2);

            // Delay Days = số ngày trễ giữa ngày kết thúc dự kiến và ngày thực tế kết thúc trễ nhất
            var latestPlannedEnd = tasks.Max(t => t.PlannedEndDate);
            var latestActualEnd = tasks.Max(t => t.ActualEndDate);
            int? delayDays = (latestActualEnd.HasValue && latestPlannedEnd.HasValue)
                ? (int?)(latestActualEnd.Value - latestPlannedEnd.Value).TotalDays
                : null;

            // BudgetOverrun = AC - PV
            decimal? budgetOverrun = actualCost - plannedValue;

            // ProjectedFinishDate = lấy ngày kết thúc thực tế trễ nhất hoặc ngày dự kiến trễ nhất nếu chưa hoàn thành
            DateTime? projectedFinish = tasks.Any(t => t.ActualEndDate.HasValue)
                ? tasks.Max(t => t.ActualEndDate)
                : tasks.Max(t => t.PlannedEndDate);

            // Tổng chi phí ước lượng của toàn bộ dự án
            decimal totalCost = tasks.Sum(t => t.PlannedCost ?? 0);

            var metric = new ProjectMetric
            {
                ProjectId = projectId,
                CalculatedBy = calculatedBy,
                IsApproved = false,
                PlannedValue = plannedValue,
                EarnedValue = earnedValue,
                ActualCost = actualCost,
                Spi = (decimal?)spi,
                Cpi = (decimal?)cpi,
                DelayDays = delayDays,
                BudgetOverrun = budgetOverrun,
                ProjectedFinishDate = projectedFinish,
                ProjectedTotalCost = totalCost,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.Add(metric);

            return _mapper.Map<ProjectMetricResponseDTO>(metric);
        }

        public async Task<ProjectMetricRequestDTO> CalculateMetricsByAIAsync(int projectId)
        {
            var tasks = await _taskRepo.GetByProjectIdAsync(projectId);
            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project == null || tasks == null || !tasks.Any())
                throw new Exception("Không đủ dữ liệu để tính toán");

            // Chuẩn bị prompt
            var prompt = BuildPrompt(project, tasks);

            // Gửi prompt tới OpenAI (gọi GPT)
            var resultJson = await CallOpenAIAsync(prompt);

            // Parse kết quả
            var result = JsonConvert.DeserializeObject<ProjectMetricRequestDTO>(resultJson);
            result.ProjectId = projectId;
            result.CalculatedBy = "AI";

            return result;
        }

        private string BuildPrompt(Project project, List<EntityTask> tasks)
        {
            var taskList = JsonConvert.SerializeObject(tasks.Select(t => new {
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

            return $@"
Dưới đây là thông tin của một dự án phần mềm, bao gồm danh sách các task. Hãy tính các chỉ số quản lý dự án:

Trả về đúng JSON:
{{
  ""plannedValue"": 0,
  ""earnedValue"": 0,
  ""actualCost"": 0,
  ""spi"": 0,
  ""cpi"": 0,
  ""budgetOverrun"": 0,
  ""projectedFinishDate"": ""{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}"",
  ""projectTotalCost"": 0
}}

Dự án:
- Tên: {project.Name}
- Ngân sách: {project.Budget}
- Bắt đầu: {project.StartDate}
- Kết thúc: {project.EndDate}

Tasks:
{taskList}
";
        }

        //private async Task<string> CallOpenAIAsync(string prompt)
        //{
        //    var requestBody = new
        //    {
        //        model = "gpt-4",
        //        messages = new[]
        //        {
        //        new { role = "system", content = "Bạn là một chuyên gia quản lý dự án." },
        //        new { role = "user", content = prompt }
        //    }
        //    };

        //    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        //    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "YOUR_API_KEY");
        //    request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        //    var response = await _httpClient.SendAsync(request);
        //    response.EnsureSuccessStatusCode();

        //    var content = await response.Content.ReadAsStringAsync();
        //    var completion = JsonConvert.DeserializeObject<OpenAIResponse>(content);

        //    return completion.Choices[0].Message.Content.Trim();
        //}

        private async Task<string> CallOpenAIAsync(string prompt)
        {
            //var apiKey = _config["OpenAI:ApiKey"]; 

            //_httpClient.DefaultRequestHeaders.Clear();
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
            new { role = "user", content = prompt }
        },
                temperature = 0.2
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenAI call failed: {response.StatusCode} - {error}");
            }

            var result = await response.Content.ReadAsStringAsync();

            // Extract content from GPT response
            var json = JObject.Parse(result);
            var reply = json["choices"]?[0]?["message"]?["content"]?.ToString();
            if (reply == null)
                throw new Exception("Empty AI response");

            return reply;
        }

        public async Task<List<ProjectMetricResponseDTO>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            return _mapper.Map<List<ProjectMetricResponseDTO>>(entities);
        }

        public async Task<ProjectMetricResponseDTO> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project metric with ID {id} not found.");

            return _mapper.Map<ProjectMetricResponseDTO>(entity);
        }

        public async Task<List<ProjectMetricResponseDTO>> GetByProjectIdAsync(int projectId)
        {
            var entities = await _repo.GetByProjectIdAsync(projectId);
            return _mapper.Map<List<ProjectMetricResponseDTO>>(entities);
        }

        public async Task<ProjectHealthDTO> GetProjectHealthAsync(int projectId)
        {
            var tasks = await _taskRepo.GetByProjectIdAsync(projectId);
            var allMetrics = await _repo.GetAllAsync();
            var latestMetric = allMetrics
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            double plannedDuration = tasks
                .Where(t => t.PlannedStartDate.HasValue && t.PlannedEndDate.HasValue)
                .Sum(t => (t.PlannedEndDate.Value - t.PlannedStartDate.Value).TotalDays);

            double actualDuration = tasks
                .Where(t => t.ActualStartDate.HasValue && t.ActualEndDate.HasValue)
                .Sum(t => (t.ActualEndDate.Value - t.ActualStartDate.Value).TotalDays);

            string timeStatus = "On track";
            if (plannedDuration > 0)
            {
                var deviation = ((actualDuration - plannedDuration) / plannedDuration) * 100;
                timeStatus = deviation >= 0 ? $"{Math.Round(deviation, 2)}% behind" : $"{Math.Abs(Math.Round(deviation, 2))}% ahead";
            }

            int tasksToBeCompleted = tasks.Count(t => t.Status != "CM");
            int overdueTasks = tasks.Count(t => t.PlannedEndDate < DateTime.UtcNow && t.Status != "CM");
            double progress = tasks.Any()
                ? tasks.Average(t => (double)(t.PercentComplete ?? 0))
                : 0;

            decimal costStatus = 0;
            var costDto = new ProjectMetricResponseDTO();
            if (latestMetric != null)
            {
                if (latestMetric.ActualCost.HasValue && latestMetric.ActualCost != 0)
                    costStatus = Math.Round(latestMetric.EarnedValue.GetValueOrDefault() / latestMetric.ActualCost.Value, 2);
                costDto = _mapper.Map<ProjectMetricResponseDTO>(latestMetric);
            }

            return new ProjectHealthDTO
            {
                TimeStatus = timeStatus,
                TasksToBeCompleted = tasksToBeCompleted,
                OverdueTasks = overdueTasks,
                ProgressPercent = Math.Round(progress, 2),
                CostStatus = costStatus,
                Cost = costDto
            };
        }
    }
}
