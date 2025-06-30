using AutoMapper;
using IntelliPM.Data.DTOs.Label.Response;
using IntelliPM.Data.DTOs.WorkItemLabel.Request;
using IntelliPM.Data.DTOs.WorkItemLabel.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.LabelRepos;
using IntelliPM.Repositories.WorkItemLabelRepos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.WorkItemLabelServices
{
    public class WorkItemLabelService : IWorkItemLabelService
    {
        private readonly IMapper _mapper;
        private readonly IWorkItemLabelRepository _repo;
        private readonly ILabelRepository _labelRepo;
        private readonly ILogger<WorkItemLabelService> _logger;

        public WorkItemLabelService(IMapper mapper, IWorkItemLabelRepository repo, ILabelRepository labelRepo, ILogger<WorkItemLabelService> logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _labelRepo = labelRepo ?? throw new ArgumentNullException(nameof(labelRepo));
        }

        public async Task<WorkItemLabelResponseDTO> CreateWorkItemLabel(WorkItemLabelRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            var label = await _labelRepo.GetByIdAsync(request.LabelId);
            if (label == null)
                throw new KeyNotFoundException($"Label with ID {request.LabelId} not found.");

            var entity = _mapper.Map<WorkItemLabel>(request);
            entity.IsDeleted = request.IsDeleted;

            try
            {
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create work item label due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create work item label: {ex.Message}", ex);
            }
            return _mapper.Map<WorkItemLabelResponseDTO>(entity);
        }

        public async Task DeleteWorkItemLabel(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Work item label with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete work item label: {ex.Message}", ex);
            }
        }

        public async Task<List<WorkItemLabelResponseDTO>> GetAllWorkItemLabelAsync(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1) throw new ArgumentException("Invalid page or page size");
            var entities = await _repo.GetAllWorkItemLabelAsync(); // Chờ kết quả
            var pagedEntities = entities
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return _mapper.Map<List<WorkItemLabelResponseDTO>>(pagedEntities);
        }

        public async Task<WorkItemLabelResponseDTO> GetWorkItemLabelById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Work item label with ID {id} not found.");

            return _mapper.Map<WorkItemLabelResponseDTO>(entity);
        }

        public async Task<WorkItemLabelResponseDTO> UpdateWorkItemLabel(int id, WorkItemLabelRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Work item label with ID {id} not found.");

            var label = await _labelRepo.GetByIdAsync(request.LabelId);
            if (label == null)
                throw new KeyNotFoundException($"Label with ID {request.LabelId} not found.");

            _mapper.Map(request, entity);
            entity.IsDeleted = request.IsDeleted;

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update work item label: {ex.Message}", ex);
            }

            return _mapper.Map<WorkItemLabelResponseDTO>(entity);
        }

        public async Task<List<WorkItemLabelResponseDTO>> GetByEpicIdAsync(string? epicId)
        {
            var entities = await _repo.GetByEpicIdAsync(epicId);
            return _mapper.Map<List<WorkItemLabelResponseDTO>>(entities);
        }

        public async Task<List<WorkItemLabelResponseDTO>> GetBySubtaskIdAsync(string? subtaskId)
        {
            var entities = await _repo.GetBySubtaskIdAsync(subtaskId);
            return _mapper.Map<List<WorkItemLabelResponseDTO>>(entities);
        }

        public async Task<List<WorkItemLabelResponseDTO>> GetByTaskIdAsync(string? taskId)
        {
            var entities = await _repo.GetByTaskIdAsync(taskId);
            return _mapper.Map<List<WorkItemLabelResponseDTO>>(entities);
        }

        public async Task<LabelResponseDTO> GetLabelById(int labelId)
        {
            var label = await _labelRepo.GetByIdAsync(labelId);
            if (label == null)
                throw new KeyNotFoundException($"Label with ID {labelId} not found.");

            return _mapper.Map<LabelResponseDTO>(label);
        }
    }
}
