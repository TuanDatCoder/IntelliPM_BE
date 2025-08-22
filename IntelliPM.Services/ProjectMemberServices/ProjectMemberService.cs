using AutoMapper;
using IntelliPM.Data.DTOs.ProjectMember.Request;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.DTOs.ProjectPosition.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.ProjectPositionRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskAssignmentRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.Helper.CustomExceptions;
using IntelliPM.Services.Helper.DecodeTokenHandler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace IntelliPM.Services.ProjectMemberServices
{
    public class ProjectMemberService : IProjectMemberService
    {
        private readonly IMapper _mapper;
        private readonly IProjectMemberRepository _projectMemberRepo;
        private readonly IProjectPositionRepository _projectPositionRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly ILogger<ProjectMemberService> _logger;
        private readonly IDecodeTokenHandler _decodeToken;
        private readonly IAccountRepository _accountRepo;
        private readonly ISubtaskRepository _subtaskRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly ITaskAssignmentRepository _taskAssignmentRepo;


        public ProjectMemberService(IMapper mapper, IProjectMemberRepository projectMemberRepo, IProjectPositionRepository projectPositionRepo, IProjectRepository projectRepo, ILogger<ProjectMemberService> logger, IDecodeTokenHandler decodeToken, IAccountRepository accountRepo, ISubtaskRepository subtaskRepo, ITaskRepository taskRepo, ITaskAssignmentRepository taskAssignmentRepo)
        {
            _mapper = mapper;
            _projectMemberRepo = projectMemberRepo;
            _projectPositionRepo = projectPositionRepo;
            _projectRepo = projectRepo;
            _logger = logger;
            _decodeToken = decodeToken;
            _accountRepo = accountRepo;
            _subtaskRepo = subtaskRepo;
            _taskRepo = taskRepo;
            _taskAssignmentRepo = taskAssignmentRepo;
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

        public async Task<ProjectMemberResponseDTO> CreateProjectMember(int projectId, ProjectMemberNoProjectIdRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");
            var existingProject = await _projectRepo.GetByIdAsync(projectId);
            if (existingProject == null)
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");

            var existingMember = await _projectMemberRepo.GetByAccountAndProjectAsync(request.AccountId, projectId);
            if (existingMember != null)
                throw new InvalidOperationException($"Account ID {request.AccountId} is already a member of Project ID {projectId}.");

            var entity = _mapper.Map<ProjectMember>(request);
            entity.ProjectId = projectId;
            entity.Status = "CREATED";

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


            var positions = await _projectPositionRepo.GetAllProjectPositions(id);
            foreach (var pos in positions)
            {
                await _projectPositionRepo.Delete(pos);
            }

            try
            {
                await _projectMemberRepo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete project member: {ex.Message}", ex);
            }
        }






        public async Task<List<ProjectPositionWithProjectInfoDTO>> GetProjectPositionsByAccountId(int accountId)
        {

            var members = await _projectMemberRepo.GetAllAsync();
            var accountMembers = members
                .Where(pm => pm.AccountId == accountId)
                .ToList();

            if (!accountMembers.Any())
                throw new KeyNotFoundException($"No project members found for Account ID {accountId}.");

            var positions = new List<ProjectPositionWithProjectInfoDTO>();

            foreach (var member in accountMembers)
            {
                var memberPositions = await _projectPositionRepo.GetAllProjectPositions(member.Id);
                var mappedPositions = memberPositions.Select(pp => new ProjectPositionWithProjectInfoDTO
                {
                    Id = pp.Id,
                    ProjectMemberId = pp.ProjectMemberId,
                    Position = pp.Position,
                    AssignedAt = pp.AssignedAt,
                    ProjectId = pp.ProjectMember.ProjectId,
                    ProjectCreatedAt = pp.ProjectMember.Project.CreatedAt
                });

                positions.AddRange(mappedPositions);
            }

            if (!positions.Any())
                throw new KeyNotFoundException($"No project positions found for Account ID {accountId}.");

            return positions;
        }





        public async Task<List<ProjectByAccountResponseDTO>> GetProjectsByAccountId(int accountId)
        {
            var currentAccount = await _accountRepo.GetAccountById(accountId);
            if (currentAccount == null)
            {
                throw new ApiException(HttpStatusCode.NotFound, "User not found");
            }

            var members = await _projectMemberRepo.GetAllAsync();
            var accountProjects = new List<ProjectByAccountResponseDTO>();

            var isLimitedRole = currentAccount.Role.Equals("TEAM_MEMBER", StringComparison.OrdinalIgnoreCase)
                             || currentAccount.Role.Equals("CLIENT", StringComparison.OrdinalIgnoreCase);

            if (isLimitedRole)
            {
                accountProjects = members
                    .Where(pm => pm.AccountId == accountId)
                    .Where(pm => !string.Equals(pm.Project.Status, "PLANNING", StringComparison.OrdinalIgnoreCase))
                    .Select(pm => new ProjectByAccountResponseDTO
                    {
                        ProjectId = pm.ProjectId,
                        ProjectName = pm.Project.Name,
                        ProjectKey = pm.Project.ProjectKey,
                        ProjectStatus = pm.Project.Status,
                        ProjectType = pm.Project.ProjectType,
                        ProjectCreateAt = pm.Project.CreatedAt,
                        IconUrl = pm.Project.IconUrl,
                        JoinedAt = pm.JoinedAt,
                        InvitedAt = pm.InvitedAt,
                        Status = pm.Status
                    })
                    .ToList();
            }
            else
            {
                accountProjects = members
                    .Where(pm => pm.AccountId == accountId)
                    .Select(pm => new ProjectByAccountResponseDTO
                    {
                        ProjectId = pm.ProjectId,
                        ProjectName = pm.Project.Name,
                        ProjectKey = pm.Project.ProjectKey,
                        ProjectStatus = pm.Project.Status,
                        ProjectType = pm.Project.ProjectType,
                        ProjectCreateAt = pm.Project.CreatedAt,
                        IconUrl = pm.Project.IconUrl,
                        JoinedAt = pm.JoinedAt,
                        InvitedAt = pm.InvitedAt,
                        Status = pm.Status
                    })
                    .ToList();
            }

            if (!accountProjects.Any())
            {
                throw new KeyNotFoundException($"No projects found for Account ID {accountId}.");
            }

            return accountProjects
                .OrderByDescending(p => p.ProjectCreateAt)
                .ThenByDescending(p => p.ProjectId)
                .ToList();

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
            var accountProjects = new List<ProjectByAccountResponseDTO>();

            var isLimitedRole = currentAccount.Role.Equals("TEAM_MEMBER", StringComparison.OrdinalIgnoreCase)
                             || currentAccount.Role.Equals("CLIENT", StringComparison.OrdinalIgnoreCase);

            if (isLimitedRole)
            {
                accountProjects = members
                    .Where(pm => pm.AccountId == currentAccount.Id)
                    .Where(pm =>
                        !string.Equals(pm.Project.Status, "PLANNING", StringComparison.OrdinalIgnoreCase))
                    .Select(pm => new ProjectByAccountResponseDTO
                    {
                        ProjectId = pm.ProjectId,
                        ProjectName = pm.Project.Name,
                        ProjectKey = pm.Project.ProjectKey,
                        ProjectStatus = pm.Project.Status,
                        ProjectCreateAt = pm.Project.CreatedAt,
                        IconUrl = pm.Project.IconUrl,
                        JoinedAt = pm.JoinedAt,
                        InvitedAt = pm.InvitedAt,
                        Status = pm.Status
                    })
                    .ToList();
            }
            else
            {
                accountProjects = members
                    .Where(pm => pm.AccountId == currentAccount.Id)
                    .Select(pm => new ProjectByAccountResponseDTO
                    {
                        ProjectId = pm.ProjectId,
                        ProjectName = pm.Project.Name,
                        ProjectKey = pm.Project.ProjectKey,
                        ProjectStatus = pm.Project.Status,
                        ProjectCreateAt = pm.Project.CreatedAt,
                        IconUrl = pm.Project.IconUrl,
                        JoinedAt = pm.JoinedAt,
                        InvitedAt = pm.InvitedAt,
                        Status = pm.Status
                    })
                    .ToList();
            }

            if (!accountProjects.Any())
            {
                throw new KeyNotFoundException($"No projects found for Account ID {currentAccount.Id}.");
            }

            return accountProjects
                .OrderByDescending(p => p.ProjectCreateAt)
                .ThenByDescending(p => p.ProjectId)
                .ToList();
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

        public async Task<ProjectMemberResponseDTO> GetProjectMemberByProjectIdAndAccountId(int projectId, int accountId)
        {
            var entity = await _projectMemberRepo.GetByAccountAndProjectAsync(accountId, projectId);

            if (entity == null)
                throw new KeyNotFoundException($"No members found for projectId {projectId}.");

            return _mapper.Map<ProjectMemberResponseDTO>(entity);
        }




        public async Task<TeamsByAccountResponseDTO> GetTeamsByAccountId(int accountId)
        {
            var projectsJoined = await _projectMemberRepo.GetProjectIdsByAccountIdAsync(accountId);

            if (!projectsJoined.Any())
                throw new KeyNotFoundException($"Account ID {accountId} has not joined any team.");

            var teams = new List<TeamWithMembersResponseDTO>();

            foreach (var projectId in projectsJoined)
            {
                var project = await _projectRepo.GetByIdAsync(projectId);
                if (project == null) continue;

                var members = await _projectMemberRepo.GetAllProjectMembers(projectId);
                var memberDtos = _mapper.Map<List<ProjectMemberResponseDTO>>(members);

                teams.Add(new TeamWithMembersResponseDTO
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    ProjectKey = project.ProjectKey,
                    TotalMembers = members.Count,
                    Members = memberDtos
                });
            }

            return new TeamsByAccountResponseDTO
            {
                TotalTeams = teams.Count,
                Teams = teams
            };
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
                if (existingMember == null)
                {
                    var memberEntity = new ProjectMember
                    {
                        AccountId = request.AccountId,
                        ProjectId = projectId,
                        JoinedAt = request.AccountId == currentAccount.Id ? DateTime.UtcNow : null,
                        InvitedAt = DateTime.UtcNow,
                        Status = request.AccountId == currentAccount.Id ? "ACTIVE" : "CREATED"
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
                    response.Role = account.Role;
                }

                responses.Add(response);
            }

            return responses;
        }

        public async Task<ProjectMemberResponseDTO> ChangeProjectMemberStatus(int id, string status)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("Status cannot be null or empty.");

            var entity = await _projectMemberRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"ProjectMember with ID {id} not found.");

            entity.Status = status;
            entity.JoinedAt = DateTime.UtcNow;

            try
            {
                await _projectMemberRepo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change ProjectMember status: {ex.Message}", ex);
            }

            return _mapper.Map<ProjectMemberResponseDTO>(entity);
        }

        public async Task<List<ProjectMemberWithTasksResponseDTO>> GetProjectMembersWithTasksAsync(int projectId)
        {
            return await _projectMemberRepo.GetProjectMembersWithTasksAsync(projectId);
        }

        //public async Task<ProjectMemberWithTasksResponseDTO> ChangeHourlyRate(int id, decimal hourlyRate)
        //{
        //    // Validate input
        //    if (hourlyRate < 0)
        //        throw new ArgumentException("Hourly rate cannot be negative.");

        //    // Get the project member
        //    var projectMember = await _projectMemberRepo.GetByIdAsync(id);
        //    if (projectMember == null)
        //        throw new KeyNotFoundException($"Project member with ID {id} not found.");

        //    // Store the original hourly rate for comparison (if needed)
        //    var originalHourlyRate = projectMember.HourlyRate ?? 0m;

        //    // Update the hourly rate
        //    projectMember.HourlyRate = hourlyRate;
        //    await _projectMemberRepo.Update(projectMember);

        //    // Step 1: Recalculate costs for subtasks assigned by this account
        //    var subtaskUpdates = new List<Subtask>();
        //    var subtasks = await _subtaskRepo.GetByProjectAndAccountAsync(projectMember.ProjectId, projectMember.AccountId);
        //    foreach (var subtask in subtasks)
        //    {
        //        decimal plannedHours = subtask.PlannedHours ?? 0m;
        //        decimal actualHours = subtask.ActualHours ?? 0m;
        //        decimal plannedResourceCost = hourlyRate * plannedHours;
        //        decimal actualResourceCost = hourlyRate * actualHours;

        //        subtask.PlannedResourceCost = plannedResourceCost;
        //        subtask.PlannedCost = plannedResourceCost;
        //        subtask.ActualResourceCost = actualResourceCost;
        //        subtask.ActualCost = actualResourceCost;
        //        subtask.UpdatedAt = DateTime.UtcNow;
        //        subtaskUpdates.Add(subtask);
        //    }

        //    // Batch update subtasks
        //    await _subtaskRepo.UpdateRange(subtaskUpdates);

        //    // Step 2: Update parent tasks based on subtasks
        //    var taskUpdates = new List<Tasks>();
        //    var uniqueTaskIds = subtaskUpdates.Select(s => s.TaskId).Distinct();
        //    foreach (var taskId in uniqueTaskIds)
        //    {
        //        var relatedSubtasks = await _subtaskRepo.GetSubtaskByTaskIdAsync(taskId);
        //        var task = await _taskRepo.GetByIdAsync(taskId);
        //        if (task != null)
        //        {
        //            decimal plannedResourceCost = relatedSubtasks.Sum(s => s.PlannedResourceCost ?? 0m);
        //            decimal actualResourceCost = relatedSubtasks.Sum(s => s.ActualResourceCost ?? 0m);
        //            task.PlannedResourceCost = plannedResourceCost;
        //            task.PlannedCost = plannedResourceCost;
        //            task.ActualResourceCost = actualResourceCost;
        //            task.ActualCost = actualResourceCost;
        //            task.UpdatedAt = DateTime.UtcNow;
        //            taskUpdates.Add(task);
        //        }
        //    }
        //    await _taskRepo.UpdateRange(taskUpdates);

        //    // Step 3: Recalculate costs for tasks with no subtasks assigned to this member
        //    var taskAssignments = await _taskAssignmentRepo.GetByAccountIdAsync(projectMember.AccountId);
        //    var subtasksCheck = await _subtaskRepo.GetByProjectIdAsync(projectMember.ProjectId);
        //    var tasksWithoutSubtasks = taskAssignments
        //        .Select(ta => ta.TaskId)
        //        .Distinct()
        //        .Select(taskId => _taskRepo.GetByIdAsync(taskId).Result)
        //        .Where(t => t != null && !subtasksCheck.Any(s => s.TaskId == t.Id))
        //        .ToList();

        //    foreach (var task in tasksWithoutSubtasks)
        //    {
        //        // Recalculate planned_resource_cost and actual_resource_cost based on all task assignments
        //        var allTaskAssignments = await _taskAssignmentRepo.GetByTaskIdAsync(task.Id);
        //        decimal plannedResourceCost = 0m;
        //        decimal actualResourceCost = 0m;

        //        foreach (var assignment in allTaskAssignments)
        //        {
        //            var member = await _projectMemberRepo.GetByAccountAndProjectAsync(assignment.AccountId, projectMember.ProjectId);
        //            decimal memberHourlyRate = member?.HourlyRate ?? 0m;
        //            plannedResourceCost += (assignment.PlannedHours ?? 0m) * memberHourlyRate;
        //            actualResourceCost += (assignment.ActualHours ?? 0m) * memberHourlyRate;
        //        }

        //        task.PlannedResourceCost = plannedResourceCost;
        //        task.PlannedCost = plannedResourceCost;
        //        task.ActualResourceCost = actualResourceCost;
        //        task.ActualCost = actualResourceCost;
        //        task.UpdatedAt = DateTime.UtcNow;
        //        await _taskRepo.Update(task);
        //    }

        //    // Return response (assuming ProjectMemberWithTasksResponseDTO includes updated data)
        //    var updatedMember = await _projectMemberRepo.GetByIdAsync(id);
        //    return _mapper.Map<ProjectMemberWithTasksResponseDTO>(updatedMember);
        //}

        public async Task<ProjectMemberWithTasksResponseDTO> ChangeHourlyRate(int id, decimal hourlyRate)
        {
            // Validate input
            if (hourlyRate < 0)
                throw new ArgumentException("Hourly rate cannot be negative.");

            // Get the project member
            var projectMember = await _projectMemberRepo.GetByIdAsync(id);
            if (projectMember == null)
                throw new KeyNotFoundException($"Project member with ID {id} not found.");

            // Update the hourly rate
            projectMember.HourlyRate = hourlyRate;
            await _projectMemberRepo.Update(projectMember);

            // Step 1: Recalculate costs for subtasks assigned by this member
            var subtaskUpdates = new List<Subtask>();
            var subtasks = await _subtaskRepo.GetByProjectAndAccountAsync(projectMember.ProjectId, projectMember.AccountId);
            foreach (var subtask in subtasks)
            {
                if (subtask.AssignedBy == null) continue; // Skip unassigned subtasks

                decimal plannedHours = subtask.PlannedHours ?? 0m;
                decimal actualHours = subtask.ActualHours ?? 0m;
                subtask.PlannedResourceCost = plannedHours * hourlyRate;
                subtask.PlannedCost = subtask.PlannedResourceCost; // Only personnel costs
                subtask.ActualResourceCost = actualHours * hourlyRate;
                subtask.ActualCost = subtask.ActualResourceCost;
                subtask.UpdatedAt = DateTime.UtcNow;
                subtaskUpdates.Add(subtask);
            }
            await _subtaskRepo.UpdateRange(subtaskUpdates);

            // Step 2: Get all tasks affected by this member (via subtasks or task assignments)
            var taskIdsFromSubtasks = subtaskUpdates.Select(s => s.TaskId).Distinct().ToList();
            var taskAssignments = await _taskAssignmentRepo.GetByAccountIdAsync(projectMember.AccountId);
            var taskIdsFromAssignments = taskAssignments
                .Where(ta => ta.Task != null && ta.Task.ProjectId == projectMember.ProjectId)
                .Select(ta => ta.TaskId)
                .Distinct()
                .ToList();
            var allAffectedTaskIds = taskIdsFromSubtasks.Union(taskIdsFromAssignments).Distinct().ToList();

            // Step 3: Recalculate costs for affected tasks
            var taskUpdates = new List<Tasks>();
            foreach (var taskId in allAffectedTaskIds)
            {
                var task = await _taskRepo.GetByIdAsync(taskId);
                if (task == null) continue;

                // Get subtasks for this task
                var relatedSubtasks = await _subtaskRepo.GetSubtaskByTaskIdAsync(taskId);

                // Get task assignments for this task
                var relatedAssignments = await _taskAssignmentRepo.GetByTaskIdAsync(taskId);

                // Calculate task costs
                decimal plannedResourceCost = 0m;
                decimal actualResourceCost = 0m;

                // Case 1: Task has subtasks
                if (relatedSubtasks.Any())
                {
                    // Sum costs from subtasks
                    plannedResourceCost = relatedSubtasks.Sum(s => s.PlannedResourceCost ?? 0m);
                    actualResourceCost = relatedSubtasks.Sum(s => s.ActualResourceCost ?? 0m);
                }

                // Case 2: Task has assignments (with or without subtasks)
                foreach (var assignment in relatedAssignments)
                {
                    var member = await _projectMemberRepo.GetByAccountAndProjectAsync(assignment.AccountId, projectMember.ProjectId);
                    decimal memberHourlyRate = member?.HourlyRate ?? 0m;
                    plannedResourceCost += (assignment.PlannedHours ?? 0m) * memberHourlyRate;
                    actualResourceCost += (assignment.ActualHours ?? 0m) * memberHourlyRate;
                }

                // Update task costs
                task.PlannedResourceCost = plannedResourceCost;
                task.PlannedCost = plannedResourceCost;
                task.ActualResourceCost = actualResourceCost;
                task.ActualCost = actualResourceCost;
                task.UpdatedAt = DateTime.UtcNow;
                taskUpdates.Add(task);
            }
            await _taskRepo.UpdateRange(taskUpdates);

            // Step 5: Update project metrics (optional, if dashboard relies on project_metric)
            //var projectMetric = await _projectMetricRepo.GetByProjectIdAndCalculatedByAsync(projectMember.ProjectId, "System");
            //if (projectMetric != null)
            //{
            //    var allTasks = await _taskRepo.GetByProjectIdAsync(projectMember.ProjectId);
            //    projectMetric.ActualCost = allTasks.Sum(t => t.ActualCost ?? 0m);
            //    projectMetric.PlannedValue = allTasks.Sum(t => t.PlannedCost ?? 0m);
            //    // Recalculate EVM metrics (e.g., CPI, SPI) if needed
            //    projectMetric.CostPerformanceIndex = projectMetric.EarnedValue > 0 ? projectMetric.EarnedValue / projectMetric.ActualCost : 0m;
            //    projectMetric.UpdatedAt = DateTime.UtcNow;
            //    await _projectMetricRepo.Update(projectMetric);
            //}

            // Return updated member
            var updatedMember = await _projectMemberRepo.GetByIdAsync(id);
            return _mapper.Map<ProjectMemberWithTasksResponseDTO>(updatedMember);
        }

        //public async Task<ProjectMemberWithTasksResponseDTO> ChangeWorkingHoursPerDay(int id, decimal workingHoursPerDay)
        //{
        //    // Validate input
        //    if (workingHoursPerDay < 0)
        //        throw new ArgumentException("Working hours per day cannot be negative.");

        //    // Get the project member
        //    var projectMember = await _projectMemberRepo.GetByIdAsync(id);
        //    if (projectMember == null)
        //        throw new KeyNotFoundException($"Project member with ID {id} not found.");

        //    // Store the original working hours per day for comparison (if needed)
        //    var originalWorkingHoursPerDay = projectMember.WorkingHoursPerDay ?? 0;

        //    // Update the working hours per day and timestamp
        //    projectMember.WorkingHoursPerDay = workingHoursPerDay;
        //    await _projectMemberRepo.Update(projectMember);

        //    // Get all task assignments for the member's account
        //    var taskAssignments = await _taskAssignmentRepo.GetByAccountIdAsync(projectMember.AccountId);
        //    if (taskAssignments == null || !taskAssignments.Any())
        //        return _mapper.Map<ProjectMemberWithTasksResponseDTO>(projectMember); // No assignments to update

        //    // Get all tasks for the project to map taskIds to plannedHours
        //    var tasks = await _taskRepo.GetByProjectIdAsync(projectMember.ProjectId);
        //    var taskDict = tasks.ToDictionary(t => t.Id, t => t.PlannedHours ?? 0m);

        //    // Process each task assignment for the member
        //    foreach (var assignment in taskAssignments)
        //    {
        //        if (taskDict.TryGetValue(assignment.TaskId, out decimal taskPlannedHours))
        //        {
        //            // Get all assignments for the same task to calculate total working hours per day
        //            var allTaskAssignments = await _taskAssignmentRepo.GetByTaskIdAsync(assignment.TaskId);
        //            var relatedMembers = new List<ProjectMember>();
        //            foreach (var ta in allTaskAssignments)
        //            {
        //                var member = await _projectMemberRepo.GetByAccountAndProjectAsync(ta.AccountId, projectMember.ProjectId);
        //                if (member != null && member.WorkingHoursPerDay.HasValue)
        //                {
        //                    relatedMembers.Add(member);
        //                }
        //            }

        //            decimal totalWorkingHoursPerDay = relatedMembers.Sum(m => m.WorkingHoursPerDay.Value);
        //            if (totalWorkingHoursPerDay > 0)
        //            {
        //                // Calculate the ratio based on the updated workingHoursPerDay
        //                decimal memberRatio = (decimal)projectMember.WorkingHoursPerDay / totalWorkingHoursPerDay;
        //                decimal newPlannedHours = taskPlannedHours * memberRatio;

        //                // Update the task assignment's planned hours
        //                assignment.PlannedHours = newPlannedHours;
        //                await _taskAssignmentRepo.Update(assignment);
        //            }
        //        }
        //    }

        //    // Return response (assuming ProjectMemberWithTasksResponseDTO includes updated data)
        //    var updatedMember = await _projectMemberRepo.GetByIdAsync(id);
        //    return _mapper.Map<ProjectMemberWithTasksResponseDTO>(updatedMember);
        //}

        public async Task<ProjectMemberWithTasksResponseDTO> ChangeWorkingHoursPerDay(int id, decimal workingHoursPerDay)
        {
            // Validate input
            if (workingHoursPerDay < 0)
                throw new ArgumentException("Working hours per day cannot be negative.");

            // Get the project member
            var projectMember = await _projectMemberRepo.GetByIdAsync(id);
            if (projectMember == null)
                throw new KeyNotFoundException($"Project member with ID {id} not found.");

            // Store the original working hours per day for comparison (if needed)
            var originalWorkingHoursPerDay = projectMember.WorkingHoursPerDay ?? 0;

            // Update the working hours per day and timestamp
            projectMember.WorkingHoursPerDay = workingHoursPerDay;
            await _projectMemberRepo.Update(projectMember);

            // Get all task assignments for the member's account within the project
            var taskAssignments = await _taskAssignmentRepo.GetByAccountIdAsync(projectMember.AccountId);
            if (taskAssignments == null || !taskAssignments.Any())
            {
                return _mapper.Map<ProjectMemberWithTasksResponseDTO>(projectMember); // No assignments to update
            }

            // Get unique task IDs from assignments
            var taskIds = taskAssignments.Select(ta => ta.TaskId).Distinct().ToList();

            // Update related tasks
            foreach (var taskId in taskIds)
            {
                var task = await _taskRepo.GetByIdAsync(taskId);
                if (task != null && task.ProjectId == projectMember.ProjectId) // Ensure task is in the same project
                {
                    // Recalculate task planned hours and related properties
                    await RecalculateTaskPlannedHours(taskId, projectMember.AccountId);
                }
            }

            // Return response with updated data
            var updatedMember = await _projectMemberRepo.GetByIdAsync(id);
            return _mapper.Map<ProjectMemberWithTasksResponseDTO>(updatedMember);
        }

        // Helper method to recalculate task planned hours
        private async Task RecalculateTaskPlannedHours(string taskId, int account)
        {
            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null) return;

            var taskAssignments = await _taskAssignmentRepo.GetByTaskIdAsync(taskId);
            var assignedAccountIds = taskAssignments.Select(a => a.AccountId).Distinct().ToList();

            var projectMembers = new List<ProjectMember>();

            foreach (var accountId in assignedAccountIds)
            {
                var member = await _projectMemberRepo.GetByAccountAndProjectAsync(accountId, task.ProjectId);
                if (member != null && member.WorkingHoursPerDay.HasValue)
                {
                    decimal hourlyRate = member.HourlyRate ?? 0m;
                    projectMembers.Add(new ProjectMember
                    {
                        Id = member.Id,
                        AccountId = member.AccountId,
                        ProjectId = member.ProjectId,
                        WorkingHoursPerDay = member.WorkingHoursPerDay,
                        HourlyRate = hourlyRate,
                    });
                }
            }

            decimal totalWorkingHoursPerDay = projectMembers.Sum(m => m.WorkingHoursPerDay.Value);
            decimal? totalCost = 0m;

            if (totalWorkingHoursPerDay > 0)
            {
                foreach (var member in projectMembers)
                {
                    var memberAssignedHours = task.PlannedHours * (member.WorkingHoursPerDay.Value / totalWorkingHoursPerDay);
                    var memberCost = memberAssignedHours * member.HourlyRate.Value;
                    totalCost += memberCost;

                    var taskAssignment = await _taskAssignmentRepo.GetByTaskAndAccountAsync(taskId, member.AccountId);
                    if (taskAssignment != null)
                    {
                        taskAssignment.PlannedHours = memberAssignedHours;
                        await _taskAssignmentRepo.Update(taskAssignment);
                    }
                }

                task.PlannedResourceCost = totalCost;
                task.PlannedCost = totalCost;
            }
            else
            {
                // Warning: No assignments or working hours, costs remain 0
                task.PlannedResourceCost = 0m;
                task.PlannedCost = 0m;
            }

            await _taskRepo.Update(task);
            //await UpdateTaskProgressAsync(task);
        }

        //private async Task UpdateTaskProgressAsync(Tasks task)
        //{
        //    if (task.Subtask?.Any() ?? false)
        //    {
        //        var subtasks = await _subtaskRepo.GetSubtaskByTaskIdAsync(task.Id);
        //        task.PercentComplete = (int)Math.Round(subtasks.Average(st => st.PercentComplete ?? 0));
        //    }
        //    else
        //    {
        //        if (task.Status == "DONE") task.PercentComplete = 100;
        //        else if (task.Status == "TO_DO") task.PercentComplete = 0;
        //        else if (task.Status == "IN_PROGRESS" && task.PlannedHours.HasValue && task.PlannedHours.Value > 0)
        //        {
        //            var rawProgress = ((task.ActualHours ?? 0) / task.PlannedHours.Value) * 100;
        //            task.PercentComplete = Math.Min((int)rawProgress, 99);
        //        }
        //        else task.PercentComplete = 0;
        //    }

        //    task.UpdatedAt = DateTime.UtcNow;
        //    await _taskRepo.Update(task);
        //}
    }
}



