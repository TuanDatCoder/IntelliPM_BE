using AutoMapper;
using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.ProjectMember.Request;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Services.Helper.CustomExceptions;
using IntelliPM.Services.Helper.DecodeTokenHandler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.ProjectMemberServices
{
    public class ProjectMemberService : IProjectMemberService
    {
        private readonly IMapper _mapper;
        private readonly IProjectMemberRepository _repo;
        private readonly ILogger<ProjectMemberService> _logger;
        private readonly IDecodeTokenHandler _decodeToken;
        private readonly IAccountRepository _accountRepository;


        public ProjectMemberService(IMapper mapper, IProjectMemberRepository repo, ILogger<ProjectMemberService> logger, IDecodeTokenHandler decodeToken)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
            _decodeToken = decodeToken;
        }
   
    
        public async Task<List<ProjectMemberResponseDTO>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            return _mapper.Map<List<ProjectMemberResponseDTO>>(entities);
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


        public async Task<List<ProjectByAccountResponseDTO>> GetProjectsByAccountId(int accountId)
        {
            var members = await _repo.GetAllAsync();
            var accountProjects = members
                .Where(pm => pm.AccountId == accountId)
                .Select(pm => new ProjectByAccountResponseDTO
                {
                    ProjectId = pm.ProjectId,
                    ProjectName = pm.Project.Name,
                    ProjectStatus = pm.Project.Status,
                    JoinedAt = pm.JoinedAt,
                    InvitedAt = pm.InvitedAt,
                    Status = pm.Status
                })
                .ToList();

            if (!accountProjects.Any())
                throw new KeyNotFoundException($"No projects found for Account ID {accountId}.");

            return accountProjects;
        }

        public async Task<List<ProjectByAccountResponseDTO>> GetProjectsByAccount(string token)
        {
            var decode = _decodeToken.decode(token);
            var currentAccount = await _accountRepository.GetAccountByUsername(decode.username);

            if (currentAccount == null)
            {
                throw new ApiException(HttpStatusCode.NotFound, "User not found");
            }

            var members = await _repo.GetAllAsync();
            var accountProjects = members
                .Where(pm => pm.AccountId == currentAccount.Id)
                .Select(pm => new ProjectByAccountResponseDTO
                {
                    ProjectId = pm.ProjectId,
                    ProjectName = pm.Project.Name,
                    ProjectStatus = pm.Project.Status,
                    JoinedAt = pm.JoinedAt,
                    InvitedAt = pm.InvitedAt,
                    Status = pm.Status
                })
                .ToList();

            if (!accountProjects.Any())
            {
                throw new KeyNotFoundException($"No projects found for Account ID {currentAccount.Id}.");
            }

            return accountProjects;
        }


        public async Task<List<AccountByProjectResponseDTO>> GetAccountsByProjectId(int projectId)
        {
            var members = await _repo.GetAllProjectMembers(projectId);
            var projectAccounts = members
                .Select(pm => new AccountByProjectResponseDTO
                {
                    AccountId = pm.AccountId,
                    AccountName = pm.Account.Username, 
                    JoinedAt = pm.JoinedAt,
                    InvitedAt = pm.InvitedAt,
                    Status = pm.Status
                })
                .ToList();

            if (!projectAccounts.Any())
                throw new KeyNotFoundException($"No accounts found for Project ID {projectId}.");

            return projectAccounts;
        }

        public async Task<List<ProjectMemberResponseDTO>> CreateListProjectMember(List<ProjectMemberRequestDTO> requests)
        {
            if (requests == null || !requests.Any())
                throw new ArgumentException("List of project members cannot be null or empty.");

            var responses = new List<ProjectMemberResponseDTO>();
            foreach (var request in requests)
            {
                var response = await AddProjectMember(request);
                responses.Add(response);
            }
            return responses;
        }

    }
}
