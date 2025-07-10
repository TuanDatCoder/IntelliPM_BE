using AutoMapper;
using IntelliPM.Data.Contexts;
using IntelliPM.Data.DTOs.Label.Response;
using IntelliPM.Data.DTOs.ProjectMember.Request;
using IntelliPM.Data.DTOs.Task;
using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.DTOs.TaskAssignment.Response;
using IntelliPM.Data.DTOs.TaskComment.Response;
using IntelliPM.Data.DTOs.TaskDependency.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.EpicRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskAssignmentRepos;
using IntelliPM.Repositories.TaskDependencyRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.TaskCommentServices; // Giả sử bạn có service này
using IntelliPM.Services.Utilities;
using IntelliPM.Services.WorkItemLabelServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.TaskServices
{
    public class TaskService : ITaskService
    {
        private readonly IMapper _mapper;
        private readonly ITaskRepository _taskRepo;
        private readonly IEpicRepository _epicRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly ISubtaskRepository _subtaskRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly ITaskCommentService _taskCommentService; 
        private readonly IWorkItemLabelService _workItemLabelService;
        private readonly ITaskAssignmentRepository _taskAssignmentRepo;
        private readonly ITaskDependencyRepository _taskDependencyRepo;

        public TaskService(IMapper mapper, ITaskRepository taskRepo, IEpicRepository epicRepo, IProjectRepository projectRepo, ISubtaskRepository subtaskRepo, IAccountRepository accountRepo, ITaskCommentService taskCommentService, IWorkItemLabelService workItemLabelService, ITaskAssignmentRepository taskAssignmentRepository, ITaskDependencyRepository taskDependencyRepo)
        {
            _mapper = mapper;
            _taskRepo = taskRepo;
            _epicRepo = epicRepo;
            _projectRepo = projectRepo;
            _subtaskRepo = subtaskRepo;
            _accountRepo = accountRepo;
            _taskCommentService = taskCommentService;
            _workItemLabelService = workItemLabelService;
            _taskAssignmentRepo = taskAssignmentRepository;
            _taskDependencyRepo = taskDependencyRepo;
        }

        public async Task<List<TaskResponseDTO>> GetAllTasks()
        {
            var entities = await _taskRepo.GetAllTasks();
            return _mapper.Map<List<TaskResponseDTO>>(entities);
        }

        public async Task<TaskResponseDTO> GetTaskById(string id)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            var dto = _mapper.Map<TaskResponseDTO>(entity);
            var dependencies = await _taskDependencyRepo.GetByTaskIdAsync(id);
            dto.Dependencies = dependencies.Select(d => new TaskDependencyResponseDTO
            {
                Id = d.Id,
                TaskId = d.TaskId,
                LinkedFrom = d.LinkedFrom,
                LinkedTo = d.LinkedTo,
                Type = d.Type
            }).ToList();

            return dto;
        }

        public async Task<List<TaskResponseDTO>> GetTaskByTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentException("Title cannot be null or empty.");

            var entities = await _taskRepo.GetByTitleAsync(title);
            if (!entities.Any())
                throw new KeyNotFoundException($"No tasks found with title containing '{title}'.");

            return _mapper.Map<List<TaskResponseDTO>>(entities);
        }

        public async Task<TaskResponseDTO> CreateTask(TaskRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Title))
                throw new ArgumentException("Task title is required.", nameof(request.Title));

            if (request.ProjectId <= 0)
                throw new ArgumentException("Project ID is required and must be greater than 0.", nameof(request.ProjectId));

            var project = await _projectRepo.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {request.ProjectId} not found.");

            var projectKey = await _projectRepo.GetProjectKeyAsync(request.ProjectId);
            if (string.IsNullOrEmpty(projectKey))
                throw new InvalidOperationException($"Invalid project key for Project ID {request.ProjectId}.");

            var entity = _mapper.Map<Tasks>(request);
            entity.Id = await IdGenerator.GenerateNextId(projectKey, _epicRepo, _taskRepo, _projectRepo, _subtaskRepo);

            try
            {
                await _taskRepo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create task due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create task: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<TaskResponseDTO> UpdateTask(string id, TaskRequestDTO request)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            _mapper.Map(request, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update task: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task DeleteTask(string id)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            try
            {
                await _taskRepo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete task: {ex.Message}", ex);
            }
        }

        public async Task<TaskResponseDTO> ChangeTaskStatus(string id, string status)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("Status cannot be null or empty.");

            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task status: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<List<TaskResponseDTO>> GetTasksByProjectIdAsync(int projectId)
        {
            if (projectId <= 0)
                throw new ArgumentException("Project ID must be greater than 0.");

            var entities = await _taskRepo.GetByProjectIdAsync(projectId);

            if (entities == null || !entities.Any())
                throw new KeyNotFoundException($"No tasks found for Project ID {projectId}.");

            return _mapper.Map<List<TaskResponseDTO>>(entities);
        }

        public async Task<List<TaskResponseDTO>> GetTasksByEpicIdAsync(string epicId)
        {

            var entities = await _taskRepo.GetByEpicIdAsync(epicId);

            if (entities == null || !entities.Any())
                throw new KeyNotFoundException($"No tasks found for epic ID {epicId}.");

            return _mapper.Map<List<TaskResponseDTO>>(entities);
        }

        public async Task<TaskResponseDTO> ChangeTaskType(string id, string type)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("Type cannot be null or empty.");

            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            entity.Type = type;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task type: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<TaskDetailedResponseDTO> GetTaskByIdDetailed(string id)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            var dto = _mapper.Map<TaskDetailedResponseDTO>(entity);
            await EnrichTaskDetailedResponse(dto);
            return dto;
        }

        public async Task<List<TaskDetailedResponseDTO>> GetTasksByProjectIdDetailed(int projectId)
        {
            if (projectId <= 0)
                throw new ArgumentException("Project ID must be greater than 0.");

            var entities = await _taskRepo.GetByProjectIdAsync(projectId);

            var dtos = _mapper.Map<List<TaskDetailedResponseDTO>>(entities);
            foreach (var dto in dtos)
            {
                await EnrichTaskDetailedResponse(dto);
            }

            return dtos;
        }

        private async Task EnrichTaskDetailedResponse(TaskDetailedResponseDTO dto)
        {
            // Lấy thông tin Reporter
            var reporter = await _accountRepo.GetAccountById(dto.ReporterId);
            if (reporter != null)
            {
                dto.ReporterFullname = reporter.FullName;
                dto.ReporterPicture = reporter.Picture;
            }

            // Lấy danh sách TaskAssignment và enrich thông tin Account
            var assignments = await _taskAssignmentRepo.GetByTaskIdAsync(dto.Id); 
            dto.TaskAssignments = (await Task.WhenAll(assignments.Select(async a =>
            {
                var assignmentDto = _mapper.Map<TaskAssignmentResponseDTO>(a);
                var account = await _accountRepo.GetAccountById(a.AccountId);
                if (account != null)
                {
                    assignmentDto.AccountFullname = account.FullName;
                    assignmentDto.AccountPicture = account.Picture;
                }
                return assignmentDto;
            }))).ToList();

            // Lấy comment và số lượng
            var allComments = await _taskCommentService.GetAllTaskComment();
            var taskComments = allComments.Where(c => c.TaskId == dto.Id).ToList();
            dto.CommentCount = taskComments.Count;
            dto.Comments = _mapper.Map<List<TaskCommentResponseDTO>>(taskComments);

            // Lấy các label
            var labels = await _workItemLabelService.GetByTaskIdAsync(dto.Id);
            dto.Labels = (await Task.WhenAll(labels.Select(async l =>
            {
                var label = await _workItemLabelService.GetLabelById(l.LabelId);
                return _mapper.Map<LabelResponseDTO>(label);
            }))).ToList();

        }

        public async Task<TaskResponseDTO> ChangeTaskTitle(string id, string title)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentException("Title cannot be null or empty.");

            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Title with task ID {id} not found.");

            entity.Title = title;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task title: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<TaskResponseDTO> ChangeTaskPlannedStartDate(string id, DateTime plannedStartDate)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Planned StartDate with task ID {id} not found.");

            entity.PlannedStartDate = plannedStartDate;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task title: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<TaskResponseDTO> ChangeTaskPlannedEndDate(string id, DateTime plannedEndDate)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Planned StartDate with task ID {id} not found.");

            entity.PlannedEndDate = plannedEndDate;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task Planned EndDate: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<TaskResponseDTO> ChangeTaskDescription(string id, string description)
        {
            var entity = await _taskRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Planned StartDate with task ID {id} not found.");

            entity.Description = description;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _taskRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task Description: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }
    }
}