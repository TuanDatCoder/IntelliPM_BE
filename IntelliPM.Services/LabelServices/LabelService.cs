using AutoMapper;
using IntelliPM.Data.DTOs.Label.Request;
using IntelliPM.Data.DTOs.Label.Response;
using IntelliPM.Data.DTOs.WorkItemLabel.Request;
using IntelliPM.Data.DTOs.WorkItemLabel.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.LabelRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.WorkItemLabelRepos;
using IntelliPM.Services.WorkItemLabelServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Services.LabelServices
{
    public class LabelService : ILabelService
    {
        private readonly IMapper _mapper;
        private readonly ILabelRepository _repo;
        private readonly IProjectRepository _projectRepo;
        private readonly ILogger<LabelService> _logger;
        private readonly IWorkItemLabelService _workItemLabelService;
        private readonly IWorkItemLabelRepository _workItemLabelRepository;

        public LabelService(IMapper mapper, ILabelRepository repo, IProjectRepository projectRepo, ILogger<LabelService> logger, IWorkItemLabelService workItemLabelService, IWorkItemLabelRepository workItemLabelRepository)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
            _projectRepo = projectRepo;
            _workItemLabelService = workItemLabelService;
            _workItemLabelRepository = workItemLabelRepository;
        }

        public async Task<LabelResponseDTO> CreateLabel(LabelRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Name))
                throw new ArgumentException("Label name is required.", nameof(request.Name));

            var project = await _projectRepo.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {request.ProjectId} not found.");

            var entity = _mapper.Map<Label>(request);
            entity.Status = request.Status ?? "ACTIVE"; // Đặt giá trị mặc định nếu không có

            try
            {
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create label due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create label: {ex.Message}", ex);
            }
            return _mapper.Map<LabelResponseDTO>(entity);
        }

        public async Task<WorkItemLabelResponseDTO> CreateLabelAndAssignAsync(CreateLabelAndAssignDTO dto)
        {
            int count = 0;
            if (!string.IsNullOrWhiteSpace(dto.TaskId)) count++;
            if (!string.IsNullOrWhiteSpace(dto.EpicId)) count++;
            if (!string.IsNullOrWhiteSpace(dto.SubtaskId)) count++;

            if (count != 1)
                throw new ArgumentException("Exactly one of TaskId, EpicId, or SubtaskId must be provided.");

            var existingLabel = await _repo.GetByProjectIdAndNameAsync(dto.ProjectId, dto.Name);

            LabelResponseDTO labelToUse;

            if (existingLabel != null)
            {
                // Sử dụng label đã tồn tại
                labelToUse = _mapper.Map<LabelResponseDTO>(existingLabel);
            }
            else
            {
                // Tạo mới nếu chưa có
                var labelRequest = new LabelRequestDTO
                {
                    ProjectId = dto.ProjectId,
                    Name = dto.Name,
                    Status = "ACTIVE"
                };

                labelToUse = await CreateLabel(labelRequest);
            }

            bool isAlreadyAssigned = await _workItemLabelRepository.IsLabelAlreadyAssignedAsync(labelToUse.Id, dto.TaskId, dto.EpicId, dto.SubtaskId);

            if (isAlreadyAssigned)
                throw new InvalidOperationException("This label has already been assigned to this work item.");


            // 3. Tạo WorkItemLabel gắn vào task/epic/subtask
            var workItemLabelRequest = new WorkItemLabelRequestDTO
            {
                LabelId = labelToUse.Id,
                TaskId = dto.TaskId,
                EpicId = dto.EpicId,
                SubtaskId = dto.SubtaskId,
                IsDeleted = false,
            };

            var result = await _workItemLabelService.CreateWorkItemLabel(workItemLabelRequest); // Gọi lại hàm CreateWorkItemLabel đã có
            return result;
        }

        public async Task DeleteLabel(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Label with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete label: {ex.Message}", ex);
            }
        }

        public async Task<List<LabelResponseDTO>> GetAllLabelAsync(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1) throw new ArgumentException("Invalid page or page size");
            var entities = (await _repo.GetAllLabelAsync())
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return _mapper.Map<List<LabelResponseDTO>>(entities);
        }

        public async Task<LabelResponseDTO> GetLabelById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Label with ID {id} not found.");

            return _mapper.Map<LabelResponseDTO>(entity);
        }

        public async Task<List<LabelResponseDTO>> GetLabelByProject(int projectId)
        {
            var entity = await _repo.GetByProjectAsync(projectId);
            if (entity == null)
                throw new KeyNotFoundException($"Label with Project ID {projectId} not found.");

            return _mapper.Map<List<LabelResponseDTO>>(entity);
        }

        public async Task<LabelResponseDTO> UpdateLabel(int id, LabelRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Label with ID {id} not found.");

            var project = await _projectRepo.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {request.ProjectId} not found.");

            _mapper.Map(request, entity);
            entity.Status = request.Status ?? entity.Status; // Giữ giá trị cũ nếu không có

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update label: {ex.Message}", ex);
            }

            return _mapper.Map<LabelResponseDTO>(entity);
        }
    }
}
