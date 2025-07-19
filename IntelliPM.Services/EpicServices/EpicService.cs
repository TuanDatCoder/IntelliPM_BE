using AutoMapper;
using IntelliPM.Data.DTOs.Epic.Request;
using IntelliPM.Data.DTOs.Epic.Response;
using IntelliPM.Data.DTOs.EpicComment.Response;
using IntelliPM.Data.DTOs.Label.Response;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.DTOs.TaskAssignment.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.EpicRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskAssignmentRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.EpicCommentServices;
using IntelliPM.Services.Helper.DecodeTokenHandler;
using IntelliPM.Services.Utilities;
using IntelliPM.Services.WorkItemLabelServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IntelliPM.Services.EpicServices
{
    public class EpicService : IEpicService
    {
        private readonly IMapper _mapper;
        private readonly IEpicRepository _epicRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly ILogger<EpicService> _logger;
        private readonly ISubtaskRepository _subtaskRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly IEpicCommentService _epicCommentService;
        private readonly IWorkItemLabelService _workItemLabelService;
        private readonly ITaskAssignmentRepository _taskAssignmentRepo;
        private readonly IDecodeTokenHandler _decodeToken;


        public EpicService(IMapper mapper, IEpicRepository epicRepo, IProjectRepository projectRepo, ITaskRepository taskRepo, ILogger<EpicService> logger, ISubtaskRepository subtaskRepo, IAccountRepository accountRepo, IEpicCommentService epicCommentService, IWorkItemLabelService workItemLabelService, ITaskAssignmentRepository taskAssignmentRepo,IDecodeTokenHandler decodeToken)
        {
            _mapper = mapper;
            _epicRepo = epicRepo;
            _logger = logger;
            _projectRepo = projectRepo;
            _taskRepo = taskRepo;
            _subtaskRepo = subtaskRepo;
            _accountRepo = accountRepo;
            _epicCommentService = epicCommentService;
            _workItemLabelService = workItemLabelService;
            _taskAssignmentRepo = taskAssignmentRepo;
            _decodeToken = decodeToken;
        }

        public async Task<List<EpicResponseDTO>> GetAllEpics()
        {
            var entities = await _epicRepo.GetAllEpics();
            return _mapper.Map<List<EpicResponseDTO>>(entities);
        }

        public async Task<EpicResponseDTO> GetEpicById(string id)
        {
            var entity = await _epicRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Epic with ID {id} not found.");

            return _mapper.Map<EpicResponseDTO>(entity);
        }

        public async Task<List<EpicResponseDTO>> GetEpicByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be null or empty.");

            var entities = await _epicRepo.GetByNameAsync(name);
            if (!entities.Any())
                throw new KeyNotFoundException($"No epics found with name containing '{name}'.");

            return _mapper.Map<List<EpicResponseDTO>>(entities);
        }

        public async Task<EpicDetailedResponseDTO> GetEpicByIdDetailed(string id)
        {
            var entity = await _epicRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Epic with ID {id} not found.");

            var dto = _mapper.Map<EpicDetailedResponseDTO>(entity);
            await EnrichEpicDetailedResponse(dto);
            return dto;
        }

        public async Task<List<EpicDetailedResponseDTO>> GetEpicsByProjectId(int projectId)
        {
            if (projectId <= 0)
                throw new ArgumentException("Project ID must be greater than 0.");

            var entities = await _epicRepo.GetByProjectKeyAsync(await _projectRepo.GetProjectKeyAsync(projectId));

            var dtos = _mapper.Map<List<EpicDetailedResponseDTO>>(entities);
            foreach (var dto in dtos)
            {
                await EnrichEpicDetailedResponse(dto);
            }

            return dtos;
        }

        public async Task<EpicResponseDTO> CreateEpic(EpicRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Name))
                throw new ArgumentException("Epic name is required.", nameof(request.Name));

            if (request.ProjectId <= 0)
                throw new ArgumentException("Project ID is required and must be greater than 0.", nameof(request.ProjectId));

            var project = await _projectRepo.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {request.ProjectId} not found.");

            var projectKey = await _projectRepo.GetProjectKeyAsync(request.ProjectId);
            if (string.IsNullOrEmpty(projectKey))
                throw new InvalidOperationException($"Invalid project key for Project ID {request.ProjectId}.");

            var entity = _mapper.Map<Epic>(request);
            entity.Id = await IdGenerator.GenerateNextId(projectKey, _epicRepo, _taskRepo, _projectRepo, _subtaskRepo);

            try
            {
                await _epicRepo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create epic due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create epic: {ex.Message}", ex);
            }

            return _mapper.Map<EpicResponseDTO>(entity);
        }

        public async Task<EpicResponseDTO> UpdateEpic(string id, EpicRequestDTO request)
        {
            var entity = await _epicRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Epic with ID {id} not found.");

            _mapper.Map(request, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _epicRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update epic: {ex.Message}", ex);
            }

            return _mapper.Map<EpicResponseDTO>(entity);
        }

        public async Task DeleteEpic(string id)
        {
            var entity = await _epicRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Epic with ID {id} not found.");

            try
            {
                await _epicRepo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete epic: {ex.Message}", ex);
            }
        }

        public async Task<EpicResponseDTO> ChangeEpicStatus(string id, string status)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("Status cannot be null or empty.");

            var entity = await _epicRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Epic with ID {id} not found.");

            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _epicRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change epic status: {ex.Message}", ex);
            }

            return _mapper.Map<EpicResponseDTO>(entity);
        }

        private async Task EnrichEpicDetailedResponse(EpicDetailedResponseDTO epicDetailedDTO)
        {
         
            if (epicDetailedDTO.ReporterId.HasValue)
            {
                var reporter = await _accountRepo.GetAccountById(epicDetailedDTO.ReporterId.Value);
                if (reporter != null)
                {
                    epicDetailedDTO.ReporterFullname = reporter.FullName;
                    epicDetailedDTO.ReporterPicture = reporter.Picture;
                }
            }
            if (epicDetailedDTO.AssignedBy.HasValue)
            {
                var assignedBy = await _accountRepo.GetAccountById(epicDetailedDTO.AssignedBy.Value);
                if (assignedBy != null)
                {
                    epicDetailedDTO.AssignedByFullname = assignedBy.FullName;
                    epicDetailedDTO.AssignedByPicture = assignedBy.Picture;
                }
            }
            var allComments = await _epicCommentService.GetAllEpicComment();
            var epicComments = allComments.Where(c => c.EpicId == epicDetailedDTO.Id).ToList();
            epicDetailedDTO.CommentCount = epicComments.Count;
            epicDetailedDTO.Comments = _mapper.Map<List<EpicCommentResponseDTO>>(epicComments);

            var labels = await _workItemLabelService.GetByEpicIdAsync(epicDetailedDTO.Id);
            var labelDtos = new List<LabelResponseDTO>();
            foreach (var l in labels)
            {
                var label = await _workItemLabelService.GetLabelById(l.LabelId);
                labelDtos.Add(_mapper.Map<LabelResponseDTO>(label));
            }
            epicDetailedDTO.Labels = labelDtos;

            epicDetailedDTO.Type = "EPIC";
        }

        public async Task<string> CreateEpicWithTaskAndAssignment(int projectId, string token, EpicWithTaskRequestDTO request)
        {

            var decode = _decodeToken.Decode(token);
            if (decode == null || string.IsNullOrEmpty(decode.username))
                throw new UnauthorizedAccessException("Invalid token data.");

            var currentAccount = await _accountRepo.GetAccountByUsername(decode.username);
            if (currentAccount == null)
                throw new KeyNotFoundException("User not found.");

            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Title))
                throw new ArgumentException("Epic Title is required.", nameof(request.Title));

            if (request.StartDate > request.EndDate)
                throw new ArgumentException("Epic EndDate must be after StartDate.", nameof(request.EndDate));

            if (projectId <= 0)
                throw new ArgumentException("Project ID must be greater than 0.", nameof(projectId));

            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");

            var projectKey = await _projectRepo.GetProjectKeyAsync(projectId);
            if (string.IsNullOrEmpty(projectKey))
                throw new InvalidOperationException($"Invalid project key for Project ID {projectId}.");

            foreach (var task in request.Tasks)
            {
                var accountIds = task.AssignedMembers.Select(m => m.AccountId).ToList();
            }

            var epicEntity = _mapper.Map<Epic>(request);
            epicEntity.ProjectId = projectId;
            epicEntity.ReporterId = currentAccount.Id;
            epicEntity.Name = request.Title;
            epicEntity.Status = "TO_DO";
            epicEntity.Id = await IdGenerator.GenerateNextId(projectKey, _epicRepo, _taskRepo, _projectRepo, _subtaskRepo);

            try
            {
                await _epicRepo.Add(epicEntity);

                foreach (var task in request.Tasks)
                {
                    var taskEntity = _mapper.Map<Tasks>(task);
                    taskEntity.Id = await IdGenerator.GenerateNextId(projectKey, _epicRepo, _taskRepo, _projectRepo, _subtaskRepo);
                    taskEntity.ReporterId = currentAccount.Id;
                    taskEntity.ProjectId = projectId;
                    taskEntity.EpicId = epicEntity.Id;
                    taskEntity.Type = "TASK";
                    taskEntity.PlannedStartDate = task.StartDate;
                    taskEntity.PlannedEndDate = task.EndDate;
                    taskEntity.Status = "TO_DO";

                    await _taskRepo.Add(taskEntity);

                    foreach (var member in task.AssignedMembers)
                    {
                        var account = await _accountRepo.GetAccountById(member.AccountId);
                        if (account == null)
                            throw new KeyNotFoundException($"Member with AccountId {member.AccountId} not found for task ID {taskEntity.Id}.");

                        var taskAssignmentEntity = new TaskAssignment
                        {
                            TaskId = taskEntity.Id,
                            AccountId = member.AccountId,
                            Status = "ASSIGNED"
                        };

                        await _taskAssignmentRepo.Add(taskAssignmentEntity);
                    }
                }

          
                return $"Created epic with ID: {epicEntity.Id}";
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create epic or tasks: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create epic or tasks: {ex.Message}", ex);
            }
        }

        public async Task<EpicTasksStatsResponseDTO> GetTasksByEpicIdWithStatsAsync(string epicId)
        {
            if (string.IsNullOrEmpty(epicId))
                throw new ArgumentException("Epic ID cannot be null or empty.");

            var epic = await _epicRepo.GetByIdAsync(epicId);
            if (epic == null)
                throw new KeyNotFoundException($"Epic with ID {epicId} not found.");

            var taskEntities = await _taskRepo.GetByEpicIdAsync(epicId);
            if (taskEntities == null || !taskEntities.Any())
                throw new KeyNotFoundException($"No tasks found for Epic ID {epicId}.");

            var taskDtos = _mapper.Map<List<TaskBacklogResponseDTO>>(taskEntities);

            var taskIds = taskDtos.Select(t => t.Id).ToList();
            var allAssignments = await _taskAssignmentRepo.GetByProjectIdAsync(epic.ProjectId);

            var assignmentsByTaskId = allAssignments
                .GroupBy(a => a.TaskId)
                .ToDictionary(g => g.Key, g => g.Select(a => _mapper.Map<TaskAssignmentResponseDTO>(a)).ToList());

            foreach (var task in taskDtos)
            {
                if (assignmentsByTaskId.TryGetValue(task.Id, out var taskAssignments))
                {
                    task.TaskAssignments = taskAssignments;
                }
                else
                {
                    task.TaskAssignments = new List<TaskAssignmentResponseDTO>();
                }
            }


            var stats = new EpicTasksStatsResponseDTO
            {
                Tasks = taskDtos,
                TotalTasks = taskDtos.Count,
                TotalToDoTasks = taskDtos.Count(t => t.Status == "TO_DO"),
                TotalInProgressTasks = taskDtos.Count(t => t.Status == "IN_PROGRESS"),
                TotalDoneTasks = taskDtos.Count(t => t.Status == "DONE")
            };

            return stats;
        }
        public async Task<List<EpicWithStatsResponseDTO>> GetEpicsWithTasksByProjectKeyAsync(string projectKey)
        {
            _logger.LogInformation("Starting GetEpicsWithTasksByProjectKeyAsync with projectKey: {ProjectKey}", projectKey);

            if (string.IsNullOrEmpty(projectKey))
            {
                _logger.LogError("Project key is null or empty.");
                throw new ArgumentException("Project key cannot be null or empty.");
            }

            var project = await _projectRepo.GetProjectByKeyAsync(projectKey);
            if (project == null)
            {
                _logger.LogError("Project not found for projectKey: {ProjectKey}", projectKey);
                throw new KeyNotFoundException($"Project with key {projectKey} not found.");
            }

            var epicEntities = await _epicRepo.GetByProjectKeyAsync(projectKey);
            if (epicEntities == null || !epicEntities.Any())
            {
                _logger.LogWarning("No epics found for projectKey: {ProjectKey}. Returning empty list.", projectKey);
                return new List<EpicWithStatsResponseDTO>();
            }

            var allTasks = await _taskRepo.GetByProjectIdAsync(project.Id);
            var validTasks = allTasks.Where(t => t.EpicId != null).ToList();
            var tasksByEpicId = validTasks
                .GroupBy(t => t.EpicId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<EpicWithStatsResponseDTO>();
            foreach (var epic in epicEntities)
            {
                _logger.LogInformation("Processing epic: {EpicId}", epic.Id);
                var taskEntities = tasksByEpicId.TryGetValue(epic.Id, out var tasks) ? tasks : new List<Tasks>();
                var taskDtos = _mapper.Map<List<TaskBacklogResponseDTO>>(taskEntities);

                var epicDto = _mapper.Map<EpicWithStatsResponseDTO>(epic);

                if (epic.ReporterId.HasValue)
                {
                    var reporter = await _accountRepo.GetAccountById(epic.ReporterId.Value);
                    if (reporter != null)
                    {
                        epicDto.ReporterFullname = reporter.FullName;
                        epicDto.ReporterPicture = reporter.Picture;
                    }
                }


                if (epic.AssignedBy.HasValue)
                {
                    var assignedBy = await _accountRepo.GetAccountById(epic.AssignedBy.Value);
                    if (assignedBy != null)
                    {
                        epicDto.AssignedByFullname = assignedBy.FullName;
                        epicDto.AssignedByPicture = assignedBy.Picture;
                    }
                }


                epicDto.TotalTasks = taskDtos.Count;
                epicDto.TotalToDoTasks = taskDtos.Count(t => t.Status == "TO_DO");
                epicDto.TotalInProgressTasks = taskDtos.Count(t => t.Status == "IN_PROGRESS");
                epicDto.TotalDoneTasks = taskDtos.Count(t => t.Status == "DONE");

                result.Add(epicDto);
            }

            _logger.LogInformation("Completed processing for projectKey: {ProjectKey}", projectKey);
            return result;
        }



    }
}