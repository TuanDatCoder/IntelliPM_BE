using AutoMapper;
using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.ProjectMember.Request;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.DTOs.ProjectPosition.Response;
using IntelliPM.Data.DTOs.Task.Request;
using IntelliPM.Data.DTOs.Task.Response;
using IntelliPM.Data.DTOs.TaskCheckList.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.ProjectPositionRepos;
using IntelliPM.Services.Helper.CustomExceptions;
using IntelliPM.Services.Helper.DecodeTokenHandler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;
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
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly IProjectPositionRepository _projectPositionRepo;
        private readonly ILogger<ProjectMemberService> _logger;
        private readonly IDecodeTokenHandler _decodeToken;
        private readonly IAccountRepository _accountRepo;


        public ProjectMemberService(IMapper mapper, IProjectMemberRepository projectMemberRepo, IProjectPositionRepository projectPositionRepo, ILogger<ProjectMemberService> logger, IDecodeTokenHandler decodeToken, IAccountRepository accountRepo)
        {
            _mapper = mapper;
            _projectMemberRepo = projectMemberRepo;
            _projectPositionRepo = projectPositionRepo;
            _logger = logger;
            _decodeToken = decodeToken;
            _accountRepo = accountRepo;
        }


        public async Task<List<ProjectMemberResponseDTO>> GetAllAsync()
        {
            var entities = await _projectMemberRepo.GetAllAsync();
            return _mapper.Map<List<ProjectMemberResponseDTO>>(entities);
        }

        public async Task<List<ProjectMemberResponseDTO>> GetAllProjectMembers(int projectId)
        {
            var entities = await _projectMemberRepo.GetAllProjectMembers(projectId);
            return _mapper.Map<List<ProjectMemberResponseDTO>>(entities);
        }

        public async Task<ProjectMemberResponseDTO> GetProjectMemberById(int id)
        {
            var entity = await _projectMemberRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project member with ID {id} not found.");

            return _mapper.Map<ProjectMemberResponseDTO>(entity);
        }



        public async Task<ProjectMemberResponseDTO> AddProjectMember(ProjectMemberRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");


            var existingMember = await _projectMemberRepo.GetByAccountAndProjectAsync(request.AccountId, request.ProjectId);
            if (existingMember != null)
                throw new InvalidOperationException($"Account ID {request.AccountId} is already a member of Project ID {request.ProjectId}.");

            var entity = _mapper.Map<ProjectMember>(request);

            try
            {
                await _projectMemberRepo.Add(entity);
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
            var entity = await _projectMemberRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Project member with ID {id} not found.");

            try
            {
                await _projectMemberRepo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete project member: {ex.Message}", ex);
            }
        }


        public async Task<List<ProjectByAccountResponseDTO>> GetProjectsByAccountId(int accountId)
        {
            var members = await _projectMemberRepo.GetAllAsync();
            var accountProjects = members
                .Where(pm => pm.AccountId == accountId)
                .Select(pm => new ProjectByAccountResponseDTO
                {
                    ProjectId = pm.ProjectId,
                    ProjectName = pm.Project.Name,
                    ProjectKey = pm.Project.ProjectKey,
                    ProjectStatus = pm.Project.Status,
                    IconUrl = pm.Project.IconUrl,
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
            var decode = _decodeToken.Decode(token);
            var currentAccount = await _accountRepo.GetAccountByUsername(decode.username);

            if (currentAccount == null)
            {
                throw new ApiException(HttpStatusCode.NotFound, "User not found");
            }

            var members = await _projectMemberRepo.GetAllAsync();
            var accountProjects = members
                .Where(pm => pm.AccountId == currentAccount.Id)
                .Select(pm => new ProjectByAccountResponseDTO
                {
                    ProjectId = pm.ProjectId,
                    ProjectName = pm.Project.Name,
                    ProjectKey = pm.Project.ProjectKey,
                    ProjectStatus = pm.Project.Status,
                    IconUrl = pm.Project.IconUrl,
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
            var members = await _projectMemberRepo.GetAllProjectMembers(projectId);
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


        public async Task<List<ProjectMember>> GetAllByProjectId(int projectId)
        {
            var entities = await _projectMemberRepo.GetAllProjectMembers(projectId);
            throw new KeyNotFoundException($"No project members found for Project ID {projectId}.");
            return entities;
        }

        public async Task<List<ProjectMemberResponseDTO>> GetProjectMemberByProjectId(int projectId)
        {
            var entities = await _projectMemberRepo.GetProjectMemberbyProjectId(projectId);

            if (entities == null || !entities.Any())
                throw new KeyNotFoundException($"No members found for projectId {projectId}.");

            return _mapper.Map<List<ProjectMemberResponseDTO>>(entities);
        }



        public async Task<List<ProjectMemberWithPositionsResponseDTO>> CreateBulkWithPositions(int projectId, string token, List<ProjectMemberWithPositionRequestDTO> requests)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token is required.", nameof(token));

            var decode = _decodeToken.Decode(token);
            if (decode == null || string.IsNullOrEmpty(decode.username))
                throw new ApiException(HttpStatusCode.Unauthorized, "Invalid token data.");

            var currentAccount = await _accountRepo.GetAccountByUsername(decode.username);
            if (currentAccount == null)
                throw new ApiException(HttpStatusCode.NotFound, "User not found.");

            if (requests == null || !requests.Any())
                throw new ArgumentException("Request list cannot be null or empty.");

        
            var checkCurrent = await _projectMemberRepo.GetByAccountAndProjectAsync(currentAccount.Id, projectId);
            if (checkCurrent == null)
            {
                var creatorRequest = new ProjectMemberWithPositionRequestDTO
                {
                    AccountId = currentAccount.Id,
                    Positions = new List<string> { currentAccount.Role ?? "TEAM_LEADER" }
                };

                var alreadyIncluded = requests.Any(r => r.AccountId == currentAccount.Id);
                if (!alreadyIncluded)
                    requests.Insert(0, creatorRequest);
                if (requests.Count < 5)
                    throw new ArgumentException("A team must have at least 5 members including the creator.");


                var allPositions = requests.SelectMany(r => r.Positions).ToList();
                if (!allPositions.Any(p => p == "PROJECT_MANAGER"))
                    throw new ArgumentException("A team must have at least one PROJECT_MANAGER.");
                if (!allPositions.Any(p => p == "TEAM_LEADER"))
                    throw new ArgumentException("A team must have at least one TEAM_LEADER.");
                if (!allPositions.Any(p => p == "CLIENT"))
                    throw new ArgumentException("A team must have at least one CLIENT.");

            }



            var memberResponses = new List<ProjectMemberWithPositionsResponseDTO>();

            foreach (var request in requests)
            {
                var existingMember = await _projectMemberRepo.GetByAccountAndProjectAsync(request.AccountId, projectId);
                if (existingMember != null)
                    throw new InvalidOperationException($"Account ID {request.AccountId} is already a member of Project ID {projectId}.");

                var memberEntity = new ProjectMember
                {
                    AccountId = request.AccountId,
                    ProjectId = projectId,
                    JoinedAt = request.AccountId == currentAccount.Id ? DateTime.UtcNow : null,
                    InvitedAt = DateTime.UtcNow,
                    Status = "CREATED"
                };
                await _projectMemberRepo.Add(memberEntity);

                var positionEntities = new List<ProjectPosition>();
                foreach (var position in request.Positions)
                {
                    if (string.IsNullOrEmpty(position))
                        throw new ArgumentException("Position cannot be null or empty.", nameof(position));

                    var positionEntity = new ProjectPosition
                    {
                        ProjectMemberId = memberEntity.Id,
                        Position = position,
                        AssignedAt = DateTime.UtcNow
                    };
                    await _projectPositionRepo.Add(positionEntity);
                    positionEntities.Add(positionEntity);
                }

                var updatedMember = await _projectMemberRepo.GetByIdAsync(memberEntity.Id);
                var response = _mapper.Map<ProjectMemberWithPositionsResponseDTO>(updatedMember);
                response.ProjectPositions = _mapper.Map<List<ProjectPositionResponseDTO>>(positionEntities);
                memberResponses.Add(response);
            }

            return memberResponses;
        }

        public async Task<List<ProjectMemberWithPositionsResponseDTO>> GetProjectMemberWithPositionsByProjectId(int projectId)
        {
          
            var members = await _projectMemberRepo.GetAllProjectMembers(projectId);
            if (members == null || !members.Any())
                throw new KeyNotFoundException($"No project members found for Project ID {projectId}.");

            var responses = new List<ProjectMemberWithPositionsResponseDTO>();

            foreach (var member in members)
            {
                var positions = await _projectPositionRepo.GetAllProjectPositions(member.Id);

                var response = _mapper.Map<ProjectMemberWithPositionsResponseDTO>(member);
                response.ProjectPositions = _mapper.Map<List<ProjectPositionResponseDTO>>(positions);

                var account = await _accountRepo.GetAccountById(member.AccountId);
                if (account != null)
                {
                    response.FullName = account.FullName;
                    response.Username = account.Username;
                    response.Picture = account.Picture;
                    response.Email = account.Email; 
                }

                responses.Add(response);
            }

            return responses;
        }

    }
}

    

