using AutoMapper;
using Google.Cloud.Storage.V1;
using IntelliPM.Data.DTOs.Subtask.Request;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Request;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.EpicRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.GeminiServices;
using IntelliPM.Services.TaskServices;
using IntelliPM.Services.Utilities;
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

        public SubtaskService(IMapper mapper, ISubtaskRepository subtaskRepo, ILogger<SubtaskService> logger, ITaskRepository taskRepo, IGeminiService geminiService, IEpicRepository epicRepo, IProjectRepository projectRepo)
        {
            _mapper = mapper;
            _subtaskRepo = subtaskRepo;
            _logger = logger;
            _taskRepo = taskRepo;
            _geminiService = geminiService;
            _epicRepo = epicRepo;
            _projectRepo = projectRepo;
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
    }
}
