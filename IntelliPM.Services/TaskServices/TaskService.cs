using AutoMapper;
using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.TaskRepos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.TaskServices
{
    public class TaskService : ITaskService
    {
        private readonly IMapper _mapper;
        private readonly ITaskRepository _repo;
        private readonly ILogger<TaskService> _logger;

        public TaskService(IMapper mapper, ITaskRepository repo, ILogger<TaskService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<TaskResponseDTO>> GetAllTasks()
        {
            var entities = await _repo.GetAllTasks();
            return _mapper.Map<List<TaskResponseDTO>>(entities);
        }

        public async Task<TaskResponseDTO> GetTaskById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task<List<TaskResponseDTO>> GetTaskByTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentException("Title cannot be null or empty.");

            var entities = await _repo.GetByTitleAsync(title);
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

            var entity = _mapper.Map<Tasks>(request);
            // Không gán CreatedAt và UpdatedAt vì DB tự động gán

            try
            {
                await _repo.Add(entity);
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

        public async Task<TaskResponseDTO> UpdateTask(int id, TaskRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            _mapper.Map(request, entity);
            // Không gán UpdatedAt vì DB tự động cập nhật

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update task: {ex.Message}", ex);
            }

            return _mapper.Map<TaskResponseDTO>(entity);
        }

        public async Task DeleteTask(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete task: {ex.Message}", ex);
            }
        }

        public async Task<TaskResponseDTO> ChangeTaskStatus(int id, string status)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("Status cannot be null or empty.");

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            entity.Status = status;
            // Không gán UpdatedAt vì DB tự động cập nhật

            try
            {
                await _repo.Update(entity);
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

            var entities = await _repo.GetByProjectIdAsync(projectId);

            if (entities == null || !entities.Any())
                throw new KeyNotFoundException($"No tasks found for Project ID {projectId}.");

            return _mapper.Map<List<TaskResponseDTO>>(entities);
        }

    }
}
