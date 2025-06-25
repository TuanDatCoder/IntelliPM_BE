using AutoMapper;
using IntelliPM.Data.DTOs.SubtaskComment.Request;
using IntelliPM.Data.DTOs.SubtaskComment.Response;
using IntelliPM.Data.DTOs.TaskComment.Request;
using IntelliPM.Data.DTOs.TaskComment.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.SubtaskCommentRepos;
using IntelliPM.Repositories.TaskCommentRepos;
using IntelliPM.Services.TaskCommentServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.SubtaskCommentServices
{
    public class SubtaskCommentService : ISubtaskCommentService
    {
        private readonly IMapper _mapper;
        private readonly ISubtaskCommentRepository _repo;
        private readonly ILogger<SubtaskCommentService> _logger;

        public SubtaskCommentService(IMapper mapper, ISubtaskCommentRepository repo, ILogger<SubtaskCommentService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
        }

        public async Task<SubtaskCommentResponseDTO> CreateSubtaskComment(SubtaskCommentRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Content))
                throw new ArgumentException("Subtask comment content is required.", nameof(request.Content));

            var entity = _mapper.Map<SubtaskComment>(request);

            try
            {
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to create subtask comment due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create subtask comment: {ex.Message}", ex);
            }

            return _mapper.Map<SubtaskCommentResponseDTO>(entity);
        }

        public async Task DeleteSubtaskComment(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Task subtask comment with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete subtask comment: {ex.Message}", ex);
            }
        }
        public async Task<List<SubtaskCommentResponseDTO>> GetAllSubtaskComment()
        {
            var entities = await _repo.GetAllSubtaskComment();
            return _mapper.Map<List<SubtaskCommentResponseDTO>>(entities);
        }

        public async Task<SubtaskCommentResponseDTO> GetSubtaskCommentById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask comment with ID {id} not found.");

            return _mapper.Map<SubtaskCommentResponseDTO>(entity);
        }

        public async Task<SubtaskCommentResponseDTO> UpdateSubtaskComment(int id, SubtaskCommentRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Subtask comment with ID {id} not found.");

            _mapper.Map(request, entity);

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update subtask comment: {ex.Message}", ex);
            }

            return _mapper.Map<SubtaskCommentResponseDTO>(entity);
        }
    }
}
