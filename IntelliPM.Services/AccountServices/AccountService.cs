using AutoMapper;
using IntelliPM.Data.DTOs.Account.Response;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.DTOs.ProjectPosition.Response;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.EpicRepos;
using IntelliPM.Repositories.ProjectPositionRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.AuthenticationServices;
using IntelliPM.Services.CloudinaryStorageServices; // Thay Firebase bằng Cloudinary
using IntelliPM.Services.EmailServices;
using IntelliPM.Services.Helper.DecodeTokenHandler;
using IntelliPM.Services.ProjectMemberServices;
using IntelliPM.Services.TaskServices;
using MimeKit.Cryptography;

namespace IntelliPM.Services.AccountServices
{
    public class AccountService : IAccountService
    {
        private readonly IMapper _mapper;
        private readonly IAccountRepository _accountRepo;
        private readonly IDecodeTokenHandler _decodeTokenHandler;
        private readonly IEmailService _emailService;
        private readonly ICloudinaryStorageService _cloudinaryStorageService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IEpicRepository _epicRepo;
        private readonly ITaskRepository _taskRepo;
        private readonly ISubtaskRepository _subTaskRepo;
        private readonly ITaskService _taskService;
        private readonly IProjectRepository _projectRepo;
        private readonly IProjectPositionRepository _projectPositionRepo;
        private readonly IProjectMemberService _projectMemberService;

        public AccountService(
            IMapper mapper,
            IAccountRepository accountRepository,
            IDecodeTokenHandler decodeTokenHandler,
            IEmailService emailService,
            ICloudinaryStorageService cloudinaryStorageService,
            IAuthenticationService authenticationService,
            IEpicRepository epicRepo,
            ITaskRepository taskRepo,
            ISubtaskRepository subTaskRepo,
            ITaskService taskService,
            IProjectRepository projectRepo,
            IProjectPositionRepository projectPositionRepo,
            IProjectMemberService projectMemberService
        )
        {
            _mapper = mapper;
            _accountRepo = accountRepository;
            _decodeTokenHandler = decodeTokenHandler;
            _emailService = emailService;
            _cloudinaryStorageService = cloudinaryStorageService;
            _authenticationService = authenticationService;
            _epicRepo = epicRepo;
            _taskRepo = taskRepo;
            _subTaskRepo = subTaskRepo;
            _taskService = taskService;
            _projectRepo = projectRepo;
            _projectPositionRepo = projectPositionRepo;
            _projectMemberService = projectMemberService;
        }


        public async Task<string> UploadProfilePictureAsync(int accountId, Stream fileStream, string fileName)
        {
            var account = await _accountRepo.GetAccountById(accountId);
            if (account == null)
            {
                throw new Exception("Account not found");
            }

            var fileUrl = await _cloudinaryStorageService.UploadFileAsync(fileStream, fileName);
            account.Picture = fileUrl;
            account.UpdatedAt = DateTime.UtcNow;
            await _accountRepo.Update(account);

            return fileUrl;
        }

        public async Task<string> UploadPictureAsync(string token, Stream fileStream, string fileName)
        {
            var account = await _authenticationService.GetAccountByToken(token);
            var fileUrl = await _cloudinaryStorageService.UploadFileAsync(fileStream, fileName);
            account.Picture = fileUrl;
            account.UpdatedAt = DateTime.UtcNow;
            await _accountRepo.Update(account);

            return fileUrl;
        }

        public async Task<bool> CheckUsernameExisted(string username)
        {
            return await _accountRepo.IsExistedByUsername(username);
        }

        public async Task<bool> CheckEmailExisted(string email)
        {
            return await _accountRepo.IsExistedByEmail(email);
        }

        public async Task<AccountResponseDTO> ChangeAccountStatus(int id, string newStatus)
        {
            var existingAccount = await _accountRepo.GetAccountById(id);
            if (existingAccount == null)
            {
                throw new KeyNotFoundException($"Account with ID {id} not found.");
            }

            existingAccount.Status = newStatus.ToString();
            await _accountRepo.Update(existingAccount);
            return _mapper.Map<AccountResponseDTO>(existingAccount);
        }

        public async Task<AccountResponseDTO> GetAccountByEmail(string email)
        {
            var entity = await _accountRepo.GetAccountByEmail(email);
            if (entity == null)
                throw new KeyNotFoundException($"Account with Email {email} not found.");

            return _mapper.Map<AccountResponseDTO>(entity);
        }

        public async Task<AccountWithWorkItemDTO> GetAccountAndWorkItemById(int accountId)
        {
            var entity = await _accountRepo.GetAccountById(accountId);
            if (entity == null)
                throw new KeyNotFoundException($"Account with id {accountId} not found.");

            var epics = await _epicRepo.GetByAccountIdAsync(accountId);
            var tasks = await _taskService.GetTasksByAccountIdAsync(accountId);
            var subtasks = await _subTaskRepo.GetByAccountIdAsync(accountId);

            var accountDto = new AccountWithWorkItemDTO
            {
                Id = entity.Id,
                Username = entity.Username,
                FullName = entity.FullName,
                WorkItems = new List<WorkItemResponseDTO>()
            };

            foreach (var epic in epics)
            {
                accountDto.WorkItems.Add(new WorkItemResponseDTO
                {
                    Key = epic.Id, 
                    ProjectId = epic.ProjectId, 
                    Summary = epic.Name, 
                    Status = epic.Status, 
                    Type = "EPIC",
                    CreatedAt = epic.CreatedAt, 
                    UpdatedAt = epic.UpdatedAt 
                });
            }

            foreach (var task in tasks)
            {
                accountDto.WorkItems.Add(new WorkItemResponseDTO
                {
                    Key = task.Id, 
                    ProjectId = task.ProjectId, 
                    Summary = task.Title, 
                    Status = task.Status,
                    Type = task.Type ?? "TASK",
                    CreatedAt = task.CreatedAt, 
                    UpdatedAt = task.UpdatedAt 
                });
            }

            foreach (var subtask in subtasks)
            {
                accountDto.WorkItems.Add(new WorkItemResponseDTO
                {
                    Key = subtask.Id, 
                    ProjectId = subtask.Task.ProjectId, 
                    Summary = subtask.Title,
                    Status = subtask.Status, 
                    Type = "SUBSTACK",
                    CreatedAt = subtask.CreatedAt, 
                    UpdatedAt = subtask.UpdatedAt 
                });
            }

            accountDto.WorkItems = accountDto.WorkItems.OrderByDescending(w => w.CreatedAt).ToList();

            return accountDto;
        }


        public async Task<ProfileResponseDTO> GetProfileByEmail(string email)
        {
            var entity = await _accountRepo.GetAccountByEmail(email);
            if (entity == null)
                throw new KeyNotFoundException($"Account with Email {email} not found.");

            var profile = _mapper.Map<ProfileResponseDTO>(entity);

            // ====== Lấy danh sách project ======
            var projects = await _projectMemberService.GetProjectsByAccountId(entity.Id);
            profile.TotalProjects = projects.Count;
            profile.UpcomingProjects = projects.Count(p => p.ProjectStatus == "PLANNING");
            profile.ActiveProjects = projects.Count(p =>
                p.ProjectStatus == "IN_PROGRESS" || p.ProjectStatus == "ON_HOLD" || p.ProjectStatus == "IN_REVIEW");
            profile.CompletedProjects = projects.Count(p => p.ProjectStatus == "COMPLETED");
            profile.CancelledProjects = projects.Count(p => p.ProjectStatus == "CANCELLED");

            profile.ProjectList = projects
                .Select(p => _mapper.Map<ProjectByAccountResponseDTO>(p))
                .ToList();

            // ====== Lấy danh sách positions ======
            var positions = await _projectMemberService.GetProjectPositionsByAccountId(entity.Id);

            profile.TotalPositions = positions.Count;
            profile.PositionsList = positions
                .Select(p => p.Position)
                .Distinct()
                .ToList();

            // Lọc recent positions
            profile.RecentPositions = positions
                .GroupBy(p => p.Position)
                .Select(g =>
                {
                    var latestAssignedAt = g.Max(x => x.AssignedAt);
                    var latestPositions = g.Where(x => x.AssignedAt == latestAssignedAt);
                    return latestPositions
                        .OrderByDescending(x => x.ProjectCreatedAt)
                        .First();
                })
                .OrderByDescending(p => p.AssignedAt)
                .Take(5)
                .Select(p => _mapper.Map<ProjectPositionResponseDTO>(p))
                .ToList();

            return profile;
        }


    }
}