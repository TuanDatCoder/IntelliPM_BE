using AutoMapper;
using IntelliPM.Data.DTOs.Project.Response;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.DTOs.ProjectPosition.Request;
using IntelliPM.Data.DTOs.ProjectPosition.Response;
using IntelliPM.Data.DTOs.Requirement.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.ProjectPositionRepos;
using IntelliPM.Services.ProjectMemberServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ProjectPositionServices
{
    public class ProjectPositionService : IProjectPositionService
    {
        private readonly IMapper _mapper;
        private readonly IProjectPositionRepository _repo;
        private readonly IProjectMemberService _projectMemberService; 
        private readonly ILogger<ProjectPositionService> _logger;

        public ProjectPositionService(IMapper mapper, IProjectPositionRepository repo, IProjectMemberService projectMemberService, ILogger<ProjectPositionService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _projectMemberService = projectMemberService;
            _logger = logger;
        }

        public async Task<List<ProjectPositionResponseDTO>> GetAllProjectPositions(int projectMemberId)
        {
            var entities = await _repo.GetAllProjectPositions(projectMemberId);
            return _mapper.Map<List<ProjectPositionResponseDTO>>(entities);
        }

        public async Task<ProjectPositionResponseDTO> GetProjectPositionById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project position with ID {id} not found.");

            return _mapper.Map<ProjectPositionResponseDTO>(entity);
        }

        public async Task<ProjectPositionResponseDTO> AddProjectPosition(ProjectPositionRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Position))
                throw new ArgumentException("Position is required.", nameof(request.Position));

            var entity = _mapper.Map<ProjectPosition>(request);
            // Không gán AssignedAt vì DB tự động gán

            try
            {
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to add project position due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add project position: {ex.Message}", ex);
            }

            return _mapper.Map<ProjectPositionResponseDTO>(entity);
        }

        public async Task<ProjectPositionResponseDTO> UpdateProjectPosition(int id, ProjectPositionRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project position with ID {id} not found.");

            _mapper.Map(request, entity);
            // Không gán AssignedAt vì DB tự động gán khi tạo, không cần cập nhật

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update project position: {ex.Message}", ex);
            }

            return _mapper.Map<ProjectPositionResponseDTO>(entity);
        }

        public async Task DeleteProjectPosition(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project position with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete project position: {ex.Message}", ex);
            }
        }

        public async Task<List<ProjectPositionResponseDTO>> CreateListProjectPosition(List<ProjectPositionRequestDTO> requests)
        {
            if (requests == null || !requests.Any())
                throw new ArgumentException("List of project positions cannot be null or empty.");

            var responses = new List<ProjectPositionResponseDTO>();
            foreach (var request in requests)
            {
                var response = await AddProjectPosition(request);
                responses.Add(response);
            }
            return responses;
        }

     
        public async Task<List<ProjectPosition>> GetAllByProjectId(int projectId)
        {
            var members = await _projectMemberService.GetAllByProjectId(projectId); 
            var positions = new List<ProjectPosition>();
            foreach (var member in members)
            {
                var memberPositions = await _repo.GetAllProjectPositions(member.Id);
                positions.AddRange(memberPositions);
            }
            if (!positions.Any())
                throw new KeyNotFoundException($"No project positions found for Project ID {projectId}.");
            return positions;
        }

       
    }

}

