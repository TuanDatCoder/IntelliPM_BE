using AutoMapper;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.DTOs.TaskComment.Request;
using IntelliPM.Data.DTOs.TaskComment.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.TaskCheckListRepos;
using IntelliPM.Repositories.TaskCommentRepos;
using IntelliPM.Services.TaskCheckListServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.TaskCommentServices
{
    public class TaskCommentService : ITaskCommentService
    {
        private readonly IMapper _mapper;
        private readonly ITaskCommentRepository _repo;
        private readonly ILogger<TaskCommentService> _logger;

        public TaskCommentService(IMapper mapper, ITaskCommentRepository repo, ILogger<TaskCommentService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
        }

        public async Task<TaskCommentResponseDTO> CreateTaskComment(TaskCommentRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Content))
                throw new ArgumentException("Task comment content is required.", nameof(request.Content));

            var entity = _mapper.Map<TaskComment>(request);

            try
            {
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create task comment due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create task comment: {ex.Message}", ex);
            }

            return _mapper.Map<TaskCommentResponseDTO>(entity);
        }

        public async Task DeleteTaskComment(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task task comment with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete task comment: {ex.Message}", ex);
            }
        }

        public async Task<List<TaskCommentResponseDTO>> GetAllTaskComment()
        {
            var entities = await _repo.GetAllTaskComment();
            return _mapper.Map<List<TaskCommentResponseDTO>>(entities);
        }

        public async Task<TaskCommentResponseDTO> GetTaskCommentById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task comment with ID {id} not found.");

            return _mapper.Map<TaskCommentResponseDTO>(entity);
        }

        public async Task<TaskCommentResponseDTO> UpdateTaskComment(int id, TaskCommentRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task comment with ID {id} not found.");

            _mapper.Map(request, entity);

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update task comment: {ex.Message}", ex);
            }

            return _mapper.Map<TaskCommentResponseDTO>(entity);
        }
    }
}
