using AutoMapper;
using IntelliPM.Data.DTOs.TaskAssignment.Request;
using IntelliPM.Data.DTOs.TaskAssignment.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.TaskAssignmentRepos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.TaskAssignmentServices
{
    public class TaskAssignmentService : ITaskAssignmentService
    {
        private readonly IMapper _mapper;
        private readonly ITaskAssignmentRepository _repo;
        private readonly ILogger<TaskAssignmentService> _logger;

        public TaskAssignmentService(IMapper mapper, ITaskAssignmentRepository repo, ILogger<TaskAssignmentService> logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<TaskAssignmentResponseDTO>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            return _mapper.Map<List<TaskAssignmentResponseDTO>>(entities);
        }

        public async Task<List<TaskAssignmentResponseDTO>> GetByTaskIdAsync(string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
                throw new ArgumentException("Task ID cannot be null or empty.");

            var entities = await _repo.GetByTaskIdAsync(taskId);
            if (!entities.Any())
                throw new KeyNotFoundException($"No task assignments found for Task ID {taskId}.");

            return _mapper.Map<List<TaskAssignmentResponseDTO>>(entities);
        }

        public async Task<List<TaskAssignmentResponseDTO>> GetByAccountIdAsync(int accountId)
        {
            if (accountId <= 0)
                throw new ArgumentException("Account ID must be greater than 0.");

            var entities = await _repo.GetByAccountIdAsync(accountId);
            if (!entities.Any())
                throw new KeyNotFoundException($"No task assignments found for Account ID {accountId}.");

            return _mapper.Map<List<TaskAssignmentResponseDTO>>(entities);
        }

        public async Task<TaskAssignmentResponseDTO> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task assignment with ID {id} not found.");

            return _mapper.Map<TaskAssignmentResponseDTO>(entity);
        }

        public async Task<TaskAssignmentResponseDTO> CreateTaskAssignment(TaskAssignmentRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.TaskId))
                throw new ArgumentException("Task ID is required.", nameof(request.TaskId));

            var entity = _mapper.Map<TaskAssignment>(request);
            entity.AssignedAt = DateTime.UtcNow;

            try
            {
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create task assignment due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create task assignment: {ex.Message}", ex);
            }

            return _mapper.Map<TaskAssignmentResponseDTO>(entity);
        }


        public async Task<TaskAssignmentResponseDTO> CreateTaskAssignmentQuick(string taskId, TaskAssignmentQuickRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            var entity = _mapper.Map<TaskAssignment>(request);
            entity.TaskId = taskId;
            entity.AssignedAt = DateTime.UtcNow;
            entity.Status = "ASSIGNED";
            try
            {
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create task assignment due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create task assignment: {ex.Message}", ex);
            }

            return _mapper.Map<TaskAssignmentResponseDTO>(entity);
        }


        public async Task<TaskAssignmentResponseDTO> UpdateTaskAssignment(int id, TaskAssignmentRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task assignment with ID {id} not found.");

            _mapper.Map(request, entity);
            entity.AssignedAt = entity.AssignedAt ?? DateTime.UtcNow; // Giữ giá trị cũ hoặc gán mới nếu null

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update task assignment: {ex.Message}", ex);
            }

            return _mapper.Map<TaskAssignmentResponseDTO>(entity);
        }

        public async Task DeleteTaskAssignment(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task assignment with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete task assignment: {ex.Message}", ex);
            }
        }

        public async Task<TaskAssignmentResponseDTO> ChangeStatus(int id, string status)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("Status cannot be null or empty.");

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task assignment with ID {id} not found.");

            entity.Status = status;
            if (status.ToLower() == "completed")
                entity.CompletedAt = DateTime.UtcNow;
            else
                entity.CompletedAt = null;

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change task assignment status: {ex.Message}", ex);
            }

            return _mapper.Map<TaskAssignmentResponseDTO>(entity);
        }

        public async Task<List<TaskAssignmentResponseDTO>> CreateListTaskAssignment(List<TaskAssignmentRequestDTO> requests)
        {
            if (requests == null || !requests.Any())
                throw new ArgumentException("List of task assignments cannot be null or empty.");

            var responses = new List<TaskAssignmentResponseDTO>();
            foreach (var request in requests)
            {
                var response = await CreateTaskAssignment(request);
                responses.Add(response);
            }
            return responses;
        }
    }
}
