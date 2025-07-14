using AutoMapper;
using Google.Cloud.Storage.V1;
using IntelliPM.Data.DTOs.Label.Response;
using IntelliPM.Data.DTOs.Subtask.Request;
using IntelliPM.Data.DTOs.Subtask.Response;
using IntelliPM.Data.DTOs.SubtaskComment.Response;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Request;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.EpicRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.GeminiServices;
using IntelliPM.Services.SubtaskCommentServices;
using IntelliPM.Services.Utilities;
using IntelliPM.Services.WorkItemLabelServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.SubtaskServices
{
    public class SubtaskService : ISubtaskService
    {
        private readonly IMapper _mapper;
        private readonly ISubtaskRepository _subtaskRepo;
        private readonly ILogger<SubtaskService> _logger;
        private readonly ITaskRepository _taskRepo;
        private readonly IGeminiService _geminiService;
        private readonly IEpicRepository _epicRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IAccountRepository _accountRepo; 
        private readonly ISubtaskCommentService _subtaskCommentService;
        private readonly IWorkItemLabelService _workItemLabelService;

        public SubtaskService(IMapper mapper, ISubtaskRepository subtaskRepo, ILogger<SubtaskService> logger, ITaskRepository taskRepo, IGeminiService geminiService, IEpicRepository epicRepo, IProjectRepository projectRepo, IAccountRepository accountRepo, ISubtaskCommentService subtaskCommentService, IWorkItemLabelService workItemLabelService)
        {
            _mapper = mapper;
            _subtaskRepo = subtaskRepo;
            _logger = logger;
            _taskRepo = taskRepo;
            _geminiService = geminiService;
            _epicRepo = epicRepo;
            _projectRepo = projectRepo;
            _accountRepo = accountRepo;
            _subtaskCommentService = subtaskCommentService;
            _workItemLabelService = workItemLabelService;
        }

        public async Task<List<Subtask>> GenerateSubtaskPreviewAsync(string taskId)
        {
            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null)
                throw new KeyNotFoundException($"Task with ID {taskId} not found.");

            var checklistTitles = await _geminiService.GenerateSubtaskAsync(task.Title);

            var checklists = checklistTitles.Select(title => new Subtask
            {
                TaskId = taskId,
                Title = title,
                Status = "TO-DO",
                ManualInput = false,
                GenerationAiInput = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            return checklists;
        }

        


        public async Task<SubtaskResponseDTO> CreateSubtask(SubtaskRequest1DTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Title))
                throw new ArgumentException("Subtask title is required.", nameof(request.Title));

            var task = await _taskRepo.GetByIdAsync(request.TaskId);
            if (task == null)
                throw new KeyNotFoundException($"Task with ID {request.TaskId} not found.");

            var project = await _projectRepo.GetByIdAsync(task.ProjectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {task.ProjectId} not found.");

            var projectKey = project.ProjectKey;

            var entity = _mapper.Map<Subtask>(request);
            entity.Id = await IdGenerator.GenerateNextId(projectKey, _epicRepo, _taskRepo, _projectRepo, _subtaskRepo);
            entity.Status = "TO-DO";
            entity.ManualInput = true;
            entity.GenerationAiInput = false;
            entity.Priority = "MEDIUM";

            try
            {
                await _subtaskRepo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create subtask due to DB error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create subtask: {ex.Message}", ex);
            }

            return _mapper.Map<SubtaskResponseDTO>(entity);
        }

        public async Task<List<SubtaskResponseDTO>> SaveGeneratedSubtasks(List<SubtaskRequest2DTO> previews)
        {
            if (previews == null || previews.Count == 0)
                throw new ArgumentException("No preview subtasks provided");

            var result = new List<SubtaskResponseDTO>();

            foreach (var request in previews)
            {
                var created = await Create2Subtask(request);
                result.Add(created);
            }

            return result;
        }

        public async Task<SubtaskResponseDTO> Create2Subtask(SubtaskRequest2DTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Title))
                throw new ArgumentException("Subtask title is required.", nameof(request.Title));

            // Lấy thông tin Task để kiểm tra tồn tại
            var task = await _taskRepo.GetByIdAsync(request.TaskId);
            if (task == null)
                throw new KeyNotFoundException($"Task with ID {request.TaskId} not found.");

            // Lấy Project để lấy ProjectKey cho ID Generator
            var project = await _projectRepo.GetByIdAsync(task.ProjectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {task.ProjectId} not found.");

            var projectKey = project.ProjectKey;

            // Map từ DTO sang Entity
            var entity = _mapper.Map<Subtask>(request);

            // Tạo ID dạng FLOWER1-6,...
            entity.Id = await IdGenerator.GenerateNextId(projectKey, _epicRepo, _taskRepo, _projectRepo, _subtaskRepo);

            // Set mặc định
            entity.Status = "TO_DO";
            entity.Priority = "MEDIUM";
            entity.AssignedBy = null;
            entity.ManualInput = true;
            entity.GenerationAiInput = false;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            // Lưu vào DB
            try
            {
                await _subtaskRepo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create subtask due to DB error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create subtask: {ex.Message}", ex);
            }

            // Trả về DTO
            return _mapper.Map<SubtaskResponseDTO>(entity);
        }


        public async Task DeleteSubtask(string id)
        {
            var entity = await _subtaskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            try
            {
                await _subtaskRepo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete Subtask: {ex.Message}", ex);
            }
        }

        public async Task<List<SubtaskResponseDTO>> GetAllSubtaskList()
        {
            var entities = await _subtaskRepo.GetAllSubtask();
            return _mapper.Map<List<SubtaskResponseDTO>>(entities);
        }

        public async Task<SubtaskResponseDTO> GetSubtaskById(string id)
        {
            var entity = await _subtaskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            return _mapper.Map<SubtaskResponseDTO>(entity);
        }

        public async Task<List<SubtaskResponseDTO>> GetSubtaskByTaskIdAsync(string taskId)
        {
            { 
                var entities = await _subtaskRepo.GetSubtaskByTaskIdAsync(taskId);

                if (entities == null || !entities.Any())
                    throw new KeyNotFoundException($"No subtasks found for Task ID {taskId}.");

                return _mapper.Map<List<SubtaskResponseDTO>>(entities);
            }
        }

        public async Task<SubtaskResponseDTO> UpdateSubtask(string id, SubtaskRequestDTO request)
        {
            var entity = await _subtaskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            _mapper.Map(request, entity);

            if(request.AssignedBy == 0) 
            entity.AssignedBy = null;

            try
            {
                await _subtaskRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update Subtask: {ex.Message}", ex);
            }

            return _mapper.Map<SubtaskResponseDTO>(entity);
        }

        public async Task<SubtaskResponseDTO> ChangeSubtaskStatus(string id, string status)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("Status cannot be null or empty.");

            var entity = await _subtaskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _subtaskRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change subtask status: {ex.Message}", ex);
            }

            return _mapper.Map<SubtaskResponseDTO>(entity);
        }


        public async Task<SubtaskDetailedResponseDTO> GetSubtaskByIdDetailed(string id)
        {
            var entity = await _subtaskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            var dto = _mapper.Map<SubtaskDetailedResponseDTO>(entity);
            await EnrichSubtaskDetailedResponse(dto);
            return dto;
        }
        public async Task<List<SubtaskDetailedResponseDTO>> GetSubtaskByTaskIdDetailed(string taskId)
        {
            var entities = await _subtaskRepo.GetSubtaskByTaskIdAsync(taskId);
            var dtos = _mapper.Map<List<SubtaskDetailedResponseDTO>>(entities); 
            foreach (var dto in dtos)
            {
                await EnrichSubtaskDetailedResponse(dto);
            }
            return dtos;
        }

        public async Task<List<SubtaskDetailedResponseDTO>> GetSubtasksByProjectIdDetailed(int projectId)
        {
        
            var tasks = await _taskRepo.GetByProjectIdAsync(projectId);
            if (!tasks.Any())
            {
                return new List<SubtaskDetailedResponseDTO>();
            }

            var taskIds = tasks.Select(t => t.Id).ToList();
            var allSubtasks = new List<SubtaskDetailedResponseDTO>();

       
            foreach (var taskId in taskIds)
            {
                var subtasks = await GetSubtaskByTaskIdDetailed(taskId);
                allSubtasks.AddRange(subtasks);
            }

            return allSubtasks.OrderBy(s => s.CreatedAt).ToList();
        }

        private async Task EnrichSubtaskDetailedResponse(SubtaskDetailedResponseDTO dto)
        {
            if (dto.ReporterId.HasValue)
            {
                var reporter = await _accountRepo.GetAccountById(dto.ReporterId.Value);
                if (reporter != null)
                {
                    dto.ReporterFullname = reporter.FullName;
                    dto.ReporterPicture = reporter.Picture;
                }
            }

        
            if (dto.AssignedBy.HasValue)
            {
                var assignedBy = await _accountRepo.GetAccountById(dto.AssignedBy.Value);
                if (assignedBy != null)
                {
                    dto.AssignedByFullname = assignedBy.FullName;
                    dto.AssignedByPicture = assignedBy.Picture;
                }
            }

            var allComments = await _subtaskCommentService.GetAllSubtaskComment();
            var subtaskComments = allComments.Where(c => c.SubtaskId == dto.Id).ToList();
            dto.CommentCount = subtaskComments.Count;
            dto.Comments = _mapper.Map<List<SubtaskCommentResponseDTO>>(subtaskComments);


            var labels = await _workItemLabelService.GetBySubtaskIdAsync(dto.Id);
            var labelDtos = new List<LabelResponseDTO>();
            foreach (var l in labels)
            {
                var label = await _workItemLabelService.GetLabelById(l.LabelId);
                labelDtos.Add(_mapper.Map<LabelResponseDTO>(label));
            }
            dto.Labels = labelDtos;
        }
    }
}


