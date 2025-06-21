using AutoMapper;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Request;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.DTOs.TaskFile.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.TaskCheckListRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.TaskServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.TaskCheckListServices
{
    public class TaskCheckListService : ITaskCheckListService
    {
        private readonly IMapper _mapper;
        private readonly ITaskCheckListRepository _repo;
        private readonly ILogger<TaskCheckListService> _logger;

        public TaskCheckListService(IMapper mapper, ITaskCheckListRepository repo, ILogger<TaskCheckListService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
        }

        public async Task<TaskCheckListResponseDTO> CreateTaskCheckList(string taskId, TaskCheckListRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Title))
                throw new ArgumentException("Task checklist title is required.", nameof(request.Title));

            var entity = _mapper.Map<TaskCheckList>(request);

            // Gán TaskId từ route
            entity.TaskId = taskId;
            entity.Status = "TO-DO";
            entity.ManualInput = true;
            entity.GenerationAiInput = false;

            try
            {
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create task checklist due to DB error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create task checklist: {ex.Message}", ex);
            }

            return _mapper.Map<TaskCheckListResponseDTO>(entity);
        }


        public async Task DeleteTaskCheckList(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task check list with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete task check list: {ex.Message}", ex);
            }
        }

        //public Task<List<TaskCheckList>> GenerateChecklistFromTaskAsync(string taskId)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<List<TaskCheckListResponseDTO>> GetAllTaskCheckList()
        {
            var entities = await _repo.GetAllTaskCheckList();
            return _mapper.Map<List<TaskCheckListResponseDTO>>(entities);
        }

        public async Task<TaskCheckListResponseDTO> GetTaskCheckListById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task check list with ID {id} not found.");

            return _mapper.Map<TaskCheckListResponseDTO>(entity);
        }

        public async Task<List<TaskCheckListResponseDTO>> GetTaskCheckListByTaskIdAsync(string taskId)
        {
            var checkList = await _repo.GetTaskCheckListByTaskIdAsync(taskId);
            return _mapper.Map<List<TaskCheckListResponseDTO>>(checkList);
        }

        public async Task<TaskCheckListResponseDTO> UpdateTaskCheckList(int id, TaskCheckListRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task check list with ID {id} not found.");

            _mapper.Map(request, entity);

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update task check list: {ex.Message}", ex);
            }

            return _mapper.Map<TaskCheckListResponseDTO>(entity);
        }
    }
}
