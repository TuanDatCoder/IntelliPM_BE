using AutoMapper;
using IntelliPM.Data.DTOs.ProjectMember.Request;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.ProjectMemberRepos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ProjectMemberServices
{
    public class ProjectMemberService : IProjectMemberService
    {
        private readonly IMapper _mapper;
        private readonly IProjectMemberRepository _repo;
        private readonly ILogger<ProjectMemberService> _logger;

        public ProjectMemberService(IMapper mapper, IProjectMemberRepository repo, ILogger<ProjectMemberService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<ProjectMemberResponseDTO>> GetAllProjectMembers(int projectId)
        {
            var entities = await _repo.GetAllProjectMembers(projectId);
            return _mapper.Map<List<ProjectMemberResponseDTO>>(entities);
        }

        public async Task<ProjectMemberResponseDTO> GetProjectMemberById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project member with ID {id} not found.");

            return _mapper.Map<ProjectMemberResponseDTO>(entity);
        }

        public async Task<ProjectMemberResponseDTO> AddProjectMember(ProjectMemberRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            // Kiểm tra xem cặp account_id và project_id đã tồn tại chưa (ràng buộc UNIQUE)
            var existingMember = await _repo.GetByAccountAndProjectAsync(request.AccountId, request.ProjectId);
            if (existingMember != null)
                throw new InvalidOperationException($"Account ID {request.AccountId} is already a member of Project ID {request.ProjectId}.");

            var entity = _mapper.Map<ProjectMember>(request);
            // Không gán JoinedAt vì DB tự động gán

            try
            {
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Failed to add project member due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add project member: {ex.Message}", ex);
            }

            return _mapper.Map<ProjectMemberResponseDTO>(entity);
        }

        public async Task DeleteProjectMember(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project member with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete project member: {ex.Message}", ex);
            }
        }
    }
}
