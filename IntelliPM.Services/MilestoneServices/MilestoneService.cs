using AutoMapper;
using IntelliPM.Data.DTOs.Milestone.Request;
using IntelliPM.Data.DTOs.Milestone.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Data.Enum.Account;
using IntelliPM.Data.Enum.ProjectMember;
using IntelliPM.Repositories.DynamicCategoryRepos;
using IntelliPM.Repositories.MilestoneRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SprintRepos;
using IntelliPM.Services.EmailServices;
using IntelliPM.Services.Helper.DecodeTokenHandler;
using IntelliPM.Services.ProjectMemberServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IntelliPM.Services.MilestoneServices
{
    public class MilestoneService : IMilestoneService
    {
        private readonly IMapper _mapper;
        private readonly IMilestoneRepository _repo;
        private readonly ISprintRepository _sprintRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly ILogger<MilestoneService> _logger;
        private readonly IDynamicCategoryRepository _dynamicCategoryRepo;
        private readonly string _frontendUrl;
        private readonly IConfiguration _config;
        private readonly IDecodeTokenHandler _decodeToken;
        private readonly IEmailService _emailService;
        private readonly IProjectMemberService _projectMemberService;
        public MilestoneService(IMapper mapper, IMilestoneRepository repo, IProjectRepository projectRepo, ISprintRepository sprintRepo, ILogger<MilestoneService> logger, IDynamicCategoryRepository dynamicCategoryRepo, IConfiguration config, IDecodeTokenHandler decodeToken, IEmailService emailService, IProjectMemberService projectMemberService)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
            _sprintRepo = sprintRepo;
            _projectRepo = projectRepo;
            _dynamicCategoryRepo = dynamicCategoryRepo;
            _frontendUrl = config["Environment:FE_URL"];
            _decodeToken = decodeToken;
            _emailService = emailService;
            _projectMemberService = projectMemberService;
        }

        public async Task<List<MilestoneResponseDTO>> GetAllMilestones()
        {
            var entities = await _repo.GetAllMilestones();
            return _mapper.Map<List<MilestoneResponseDTO>>(entities);
        }

        public async Task<MilestoneResponseDTO> GetMilestoneById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Milestone with ID {id} not found.");

            return _mapper.Map<MilestoneResponseDTO>(entity);
        }

        public async Task<List<MilestoneResponseDTO>> GetMilestoneByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be null or empty.");

            var entities = await _repo.GetByNameAsync(name);
            if (!entities.Any())
                throw new KeyNotFoundException($"No milestones found with name containing '{name}'.");

            return _mapper.Map<List<MilestoneResponseDTO>>(entities);
        }




        public async Task<MilestoneResponseDTO> CreateMilestone(MilestoneRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Name))
                throw new ArgumentException("Milestone name is required.", nameof(request.Name));

            var project = await _projectRepo.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {request.ProjectId} not found.");

            string milestoneKey = await GenerateMilestoneKeyAsync(request.ProjectId, project.ProjectKey);

            var entity = _mapper.Map<Milestone>(request);
            entity.Key = milestoneKey;

            try
            {
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {

                throw new Exception($"Failed to create milestone due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating milestone with key: {Key}", milestoneKey);
                throw new Exception($"Failed to create milestone: {ex.Message}", ex);
            }

            return _mapper.Map<MilestoneResponseDTO>(entity);
        }



        public async Task<MilestoneResponseDTO> CreateQuickMilestone(MilestoneQuickRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            if (string.IsNullOrEmpty(request.Name))
                throw new ArgumentException("Milestone name is required.", nameof(request.Name));

            var project = await _projectRepo.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {request.ProjectId} not found.");

            string milestoneKey = await GenerateMilestoneKeyAsync(request.ProjectId, project.ProjectKey);

            var entity = _mapper.Map<Milestone>(request);
            entity.Key = milestoneKey;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.Status = "PLANNING";
            try
            {
                _logger.LogInformation("Creating milestone with key: {Key} for project: {ProjectId}", milestoneKey, request.ProjectId);
                await _repo.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating milestone with key: {Key}", milestoneKey);
                throw new Exception($"Failed to create milestone due to database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating milestone with key: {Key}", milestoneKey);
                throw new Exception($"Failed to create milestone: {ex.Message}", ex);
            }

            return _mapper.Map<MilestoneResponseDTO>(entity);
        }



        private async Task<string> GenerateMilestoneKeyAsync(int projectId, string projectKey)
        {
            var entities = await _repo.GetMilestonesByProjectIdAsync(projectId);
            int nextNumber = 1;

            if (entities.Any())
            {
                var latestKey = entities
                    .Select(m => m.Key?.Trim() ?? "")
                    .Where(k => k.StartsWith($"{projectKey}-M"))
                    .OrderByDescending(k => k)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(latestKey))
                {
                    var parts = latestKey.Split('-');
                    string lastPart = parts.LastOrDefault() ?? "";
                    if (lastPart.StartsWith("M") && int.TryParse(lastPart.Substring(1), out int currentNumber))
                    {
                        nextNumber = currentNumber + 1;
                    }
                }
            }

            return $"{projectKey}-M{nextNumber}";
        }







        public async Task<MilestoneResponseDTO> UpdateMilestone(int id, MilestoneRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Milestone with ID {id} not found.");

            _mapper.Map(request, entity);
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update milestone: {ex.Message}", ex);
            }

            return _mapper.Map<MilestoneResponseDTO>(entity);
        }

        public async Task DeleteMilestone(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Milestone with ID {id} not found.");

            try
            {
                await _repo.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete milestone: {ex.Message}", ex);
            }
        }

        public async Task<MilestoneResponseDTO> ChangeMilestoneStatus(int id, string status)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("Status cannot be null or empty.");

            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Milestone with ID {id} not found.");

            entity.Status = status;
            entity.UpdatedAt = DateTime.UtcNow;


            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change milestone status: {ex.Message}", ex);
            }

            return _mapper.Map<MilestoneResponseDTO>(entity);
        }

        //public async Task<MilestoneResponseDTO> ChangeMilestoneStatus(int id, string status)
        //{
        //    if (string.IsNullOrEmpty(status))
        //        throw new ArgumentException("Status cannot be null or empty.");

        //    // Fetch valid milestone statuses from dynamic_category
        //    var milestoneStatuses = await _dynamicCategoryRepo.FindAllAsync(c => c.Group == "milestone_status");
        //    var validStatuses = milestoneStatuses.Select(c => c.Name.ToUpper()).ToList();

        //    // Validate the provided status
        //    if (!validStatuses.Contains(status.ToUpper()))
        //        throw new ArgumentException($"Invalid status '{status}'. Valid statuses are: {string.Join(", ", validStatuses)}.");

        //    var entity = await _repo.GetByIdAsync(id);
        //    if (entity == null)
        //        throw new KeyNotFoundException($"Milestone with ID {id} not found.");

        //    // Determine if the milestone is starting or completing
        //    var isStarting = validStatuses
        //        .Where(s => s != "PLANNING" && s != "CANCELLED" && s != "ON_HOLD")
        //        .Contains(status.ToUpper());
        //    var isCompleting = validStatuses
        //        .Where(s => s == "AWAITING_REVIEW" || s == "APPROVED" || s == "REJECTED")
        //        .Contains(status.ToUpper());

        //    // Fetch dependencies
        //    var dependencies = await _taskDependencyRepo
        //        .FindAllAsync(d => d.LinkedTo == entity.Key && d.ToType == "Milestone");

        //    var warnings = new List<string>();

        //    foreach (var dep in dependencies)
        //    {
        //        object? source = null;
        //        string? sourceStatus = null;

        //        switch (dep.FromType.ToLower())
        //        {
        //            case "task":
        //                var task = await _taskRepo.GetByIdAsync(dep.LinkedFrom);
        //                if (task == null) continue;
        //                source = task;
        //                sourceStatus = task.Status;
        //                break;

        //            case "subtask":
        //                var subtask = await _subtaskRepo.GetByIdAsync(dep.LinkedFrom);
        //                if (subtask == null) continue;
        //                source = subtask;
        //                sourceStatus = subtask.Status;
        //                break;

        //            case "milestone":
        //                var milestone = await _milestoneRepo.GetByKeyAsync(dep.LinkedFrom);
        //                if (milestone == null) continue;
        //                source = milestone;
        //                sourceStatus = milestone.Status;
        //                break;

        //            default:
        //                continue;
        //        }

        //        // Fetch valid statuses for the source type
        //        var sourceValidStatuses = await _categoryRepo.FindAllAsync(c => c.Group == $"{dep.FromType.ToLower()}_status");
        //        var sourceValidStatusNames = sourceValidStatuses.Select(c => c.Name.ToUpper()).ToList();

        //        bool sourceStarted = false;
        //        bool sourceDone = false;

        //        if (dep.FromType.ToLower() == "task" || dep.FromType.ToLower() == "subtask")
        //        {
        //            sourceStarted = sourceStatus != null && sourceValidStatusNames.Contains(sourceStatus.ToUpper()) && sourceStatus.ToUpper() != "TO_DO";
        //            sourceDone = sourceStatus != null && sourceStatus.ToUpper() == "DONE";
        //        }
        //        else if (dep.FromType.ToLower() == "milestone")
        //        {
        //            sourceStarted = sourceStatus != null && sourceValidStatusNames.Contains(sourceStatus.ToUpper()) &&
        //                            sourceStatus.ToUpper() != "PLANNING" && sourceStatus.ToUpper() != "CANCELLED";
        //            sourceDone = sourceStatus != null && (sourceStatus.ToUpper() == "AWAITING_REVIEW" || sourceStatus.ToUpper() == "APPROVED");
        //        }

        //        switch (dep.Type.ToUpper())
        //        {
        //            case "FINISH_START":
        //                if (isStarting && !sourceDone)
        //                {
        //                    warnings.Add($"Milestone '{entity.Key}' depends on {dep.FromType.ToLower()} '{dep.LinkedFrom}' to be completed before starting.");
        //                }
        //                break;

        //            case "START_START":
        //                if (isStarting && !sourceStarted)
        //                {
        //                    warnings.Add($"Milestone '{entity.Key}' depends on {dep.FromType.ToLower()} '{dep.LinkedFrom}' to be started before starting.");
        //                }
        //                break;

        //            case "FINISH_FINISH":
        //                if (isCompleting && !sourceDone)
        //                {
        //                    warnings.Add($"Milestone '{entity.Key}' depends on {dep.FromType.ToLower()} '{dep.LinkedFrom}' to be completed before it can be completed.");
        //                }
        //                break;

        //            case "START_FINISH":
        //                if (isCompleting && !sourceStarted)
        //                {
        //                    warnings.Add($"Milestone '{entity.Key}' can only be completed after {dep.FromType.ToLower()} '{dep.LinkedFrom}' has started.");
        //                }
        //                break;
        //        }
        //    }

        //    entity.Status = status;
        //    entity.UpdatedAt = DateTime.UtcNow;

        //    try
        //    {
        //        await _repo.Update(entity);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Failed to change milestone status: {ex.Message}", ex);
        //    }

        //    var result = _mapper.Map<MilestoneResponseDTO>(entity);
        //    result.Warnings = warnings;
        //    return result;
        //}


        public async Task<MilestoneResponseDTO> ChangeMilestoneSprint(string key, int sprintId)
        {
            var entity = await _repo.GetByKeyAsync(key);
            if (entity == null)
                throw new KeyNotFoundException($"Milestone with Key {key} not found.");

            if (sprintId == 0)
            {
                entity.SprintId = null;
            }
            else
            {
                var sprint = await _sprintRepo.GetByIdAsync(sprintId);
                if (sprint == null)
                    throw new KeyNotFoundException($"Sprint with ID {sprintId} not found.");
                entity.SprintId = sprintId;
            }


            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _repo.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change milestone sprint: {ex.Message}", ex);
            }

            return _mapper.Map<MilestoneResponseDTO>(entity);
        }



        public async Task<List<MilestoneResponseDTO>> GetMilestonesByProjectIdAsync(int projectId)
        {
            if (projectId <= 0)
                throw new ArgumentException("Project ID must be greater than 0.");

            var entities = await _repo.GetMilestonesByProjectIdAsync(projectId);

            if (entities == null || !entities.Any())
                throw new KeyNotFoundException($"No milestones found for Project ID {projectId}.");

            return _mapper.Map<List<MilestoneResponseDTO>>(entities);
        }






        public async Task<string> SendMilestoneEmail(int projectId, int milestoneId, string token)
        {
            var decode = _decodeToken.Decode(token);
            if (decode == null || string.IsNullOrEmpty(decode.username))
                throw new UnauthorizedAccessException("Invalid token data.");

            
            var project = await _projectRepo.GetByIdAsync(projectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");

      
            var milestone = await _repo.GetByIdAsync(milestoneId);
            if (milestone == null)
                throw new KeyNotFoundException($"Milestone with ID {milestoneId} not found.");

            if (milestone.ProjectId != projectId)
                throw new InvalidOperationException("Milestone does not belong to the specified project.");

       
            var membersWithPositions = await _projectMemberService.GetProjectMemberWithPositionsByProjectId(projectId);
            if (membersWithPositions == null || !membersWithPositions.Any())
                throw new KeyNotFoundException($"No project members found for Project ID {projectId}.");

            var clients = membersWithPositions
                .Where(m =>
                    m.ProjectPositions != null &&
                    m.ProjectPositions.Any(p => p.Position == AccountPositionEnum.CLIENT.ToString()) &&
                    m.Status == ProjectMemberStatusEnum.ACTIVE.ToString())
                .ToList();

            if (!clients.Any())
                throw new ArgumentException("No active clients found for the project.");

            var milestoneDetailsUrl = $"{_frontendUrl}/projectclient?projectKey={project.ProjectKey}#timeline";


            // Gửi email đến tất cả client
            var failedEmails = new List<string>();
            foreach (var client in clients)
            {
                if (string.IsNullOrEmpty(client.FullName) || string.IsNullOrEmpty(client.Email))
                {
                    _logger.LogWarning("Client with ID {ClientId} has missing full name or email.", client.Id);
                    failedEmails.Add($"Client ID {client.Id}");
                    continue;
                }

                try
                {
                    await _emailService.SendMilestoneNotificationEmail(
                        client.FullName,
                        client.Email,
                        project.Name,
                        project.ProjectKey,
                        projectId,
                        milestone.Name,
                        milestone.Status,
                        milestone.StartDate,
                        milestone.EndDate,
                        milestone.Description,
                        milestoneDetailsUrl
                    );

                    _logger.LogInformation("Email sent successfully to client {Email} for Milestone ID {MilestoneId}", client.Email, milestoneId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to client {Email} for Milestone ID {MilestoneId}", client.Email, milestoneId);
                    failedEmails.Add(client.Email);
                }
            }

            // Trả về kết quả
            if (failedEmails.Any())
            {
                throw new Exception($"Failed to send email to some clients: {string.Join(", ", failedEmails)}");
            }

            return $"Email sent successfully to {clients.Count} client(s) for Milestone ID {milestoneId}.";
        }

    }
}
