using AutoMapper;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Request;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.TaskServices;
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
        private readonly ISubtaskRepository _repo;
        private readonly ILogger<SubtaskService> _logger;

        public SubtaskService(IMapper mapper, ISubtaskRepository repo, ILogger<SubtaskService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
        }

        public async Task<SubtaskResponseDTO> CreateSubtask(SubtaskRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Title))
                throw new ArgumentException("Subtask title is required.", nameof(request.Title));

            var entity = _mapper.Map<Subtask>(request);
            entity.Status = "TO-DO";

            try
            {
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create subtask due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create subtask: {ex.Message}", ex);
            }

            return _mapper.Map<SubtaskResponseDTO>(entity);
        }

        public async Task DeleteSubtask(string id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete Subtask: {ex.Message}", ex);
            }
        }

        public async Task<List<SubtaskResponseDTO>> GetAllSubtaskList()
        {
            var entities = await _repo.GetAllSubtask();
            return _mapper.Map<List<SubtaskResponseDTO>>(entities);
        }

        public async Task<SubtaskResponseDTO> GetSubtaskById(string id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            return _mapper.Map<SubtaskResponseDTO>(entity);
        }

        public async Task<SubtaskResponseDTO> UpdateSubtask(string id, SubtaskRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask with ID {id} not found.");

            _mapper.Map(request, entity);

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update Subtask: {ex.Message}", ex);
            }

            return _mapper.Map<SubtaskResponseDTO>(entity);
        }
    }
}
