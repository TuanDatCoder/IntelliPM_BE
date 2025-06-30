using AutoMapper;
using IntelliPM.Data.DTOs.Epic.Request;
using IntelliPM.Data.DTOs.Epic.Response;
using IntelliPM.Data.DTOs.EpicComment.Response;
using IntelliPM.Data.DTOs.Label.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.EpicRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.EpicCommentServices;
using IntelliPM.Services.Utilities;
using IntelliPM.Services.WorkItemLabelServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public EpicService(IMapper mapper, IEpicRepository epicRepo, IProjectRepository projectRepo, ITaskRepository taskRepo, ILogger<EpicService> logger, ISubtaskRepository subtaskRepo, IAccountRepository accountRepo, IEpicCommentService epicCommentService, IWorkItemLabelService workItemLabelService)
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
            if (!entities.Any())
                throw new KeyNotFoundException($"No epics found for Project ID {projectId}.");

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
            // Lấy thông tin Reporter
            if (epicDetailedDTO.ReporterId.HasValue)
            {
                var reporter = await _accountRepo.GetAccountById(epicDetailedDTO.ReporterId.Value);
                if (reporter != null)
                {
                    epicDetailedDTO.ReporterFullname = reporter.FullName;
                    epicDetailedDTO.ReporterPicture = reporter.Picture;
                }
            }

            // Lấy thông tin AssignedBy
            if (epicDetailedDTO.AssignedById.HasValue)
            {
                var assignedBy = await _accountRepo.GetAccountById(epicDetailedDTO.AssignedById.Value);
                if (assignedBy != null)
                {
                    epicDetailedDTO.AssignedByFullname = assignedBy.FullName;
                    epicDetailedDTO.AssignedByPicture = assignedBy.Picture;
                }
            }

            // Lấy comment và số lượng
            var allComments = await _epicCommentService.GetAllEpicComment();
            var epicComments = allComments.Where(c => c.EpicId == epicDetailedDTO.Id).ToList();
            epicDetailedDTO.CommentCount = epicComments.Count;
            epicDetailedDTO.Comments = _mapper.Map<List<EpicCommentResponseDTO>>(epicComments);

            // Lấy các label
            var labels = await _workItemLabelService.GetByEpicIdAsync(epicDetailedDTO.Id);
            epicDetailedDTO.Labels = (await Task.WhenAll(labels.Select(async l =>
            {
                var label = await _workItemLabelService.GetLabelById(l.LabelId); 
                return _mapper.Map<LabelResponseDTO>(label);
            }))).ToList();

            epicDetailedDTO.Type = "EPIC";
        }
    }
}