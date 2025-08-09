using IntelliPM.API.Middlewares;
using Google.Api;
using Hangfire;
using Hangfire.PostgreSql;
using IntelliPM.Data.Contexts;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.ActivityLogRepos;
using IntelliPM.Repositories.AiResponseEvaluationRepos;
using IntelliPM.Repositories.AiResponseHistoryRepos;
using IntelliPM.Repositories.DocumentCommentRepos;
using IntelliPM.Repositories.DocumentExportFileRepos;
using IntelliPM.Repositories.DocumentPermissionRepos;
using IntelliPM.Repositories.DocumentRepos;
using IntelliPM.Repositories.DocumentRepos.DocumentRepository;
using IntelliPM.Repositories.DynamicCategoryRepos;
using IntelliPM.Repositories.EpicCommentRepos;
using IntelliPM.Repositories.EpicFileRepos;
using IntelliPM.Repositories.EpicRepos;
using IntelliPM.Repositories.LabelRepos;
using IntelliPM.Repositories.MeetingDocumentRepos;
using IntelliPM.Repositories.MeetingLogRepos;
using IntelliPM.Repositories.MeetingParticipantRepos;
using IntelliPM.Repositories.MeetingRepos;
using IntelliPM.Repositories.MeetingRescheduleRequestRepos;
using IntelliPM.Repositories.MeetingSummaryRepos;
using IntelliPM.Repositories.MeetingTranscriptRepos;
using IntelliPM.Repositories.MilestoneCommentRepos;
using IntelliPM.Repositories.MilestoneFeedbackRepos;
using IntelliPM.Repositories.MilestoneRepos;
using IntelliPM.Repositories.NotificationRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.ProjectMetricRepos;
using IntelliPM.Repositories.ProjectPositionRepos;
using IntelliPM.Repositories.ProjectRecommendationRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.RecipientNotificationRepos;
using IntelliPM.Repositories.RefreshTokenRepos;
using IntelliPM.Repositories.RequirementRepos;
using IntelliPM.Repositories.RiskCommentRepos;
using IntelliPM.Repositories.RiskFileRepos;
using IntelliPM.Repositories.RiskRepos;
using IntelliPM.Repositories.RiskSolutionRepos;
using IntelliPM.Repositories.SprintRepos;
using IntelliPM.Repositories.SubtaskCommentRepos;
using IntelliPM.Repositories.SubtaskFileRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.SystemConfigurationRepos;
using IntelliPM.Repositories.TaskAssignmentRepos;
using IntelliPM.Repositories.TaskCommentRepos;
using IntelliPM.Repositories.TaskDependencyRepos;
using IntelliPM.Repositories.TaskFileRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Repositories.WorkItemLabelRepos;
using IntelliPM.Repositories.WorkLogRepos;
using IntelliPM.Services.AccountServices;
using IntelliPM.Services.ActivityLogServices;
using IntelliPM.Services.AdminServices;
using IntelliPM.Services.AiResponseEvaluationServices;
using IntelliPM.Services.AiResponseHistoryServices;
using IntelliPM.Services.AiServices.SprintPlanningServices;
using IntelliPM.Services.AiServices.SprintTaskPlanningServices;
using IntelliPM.Services.AiServices.TaskPlanningServices;
using IntelliPM.Services.AuthenticationServices;
using IntelliPM.Services.ChatGPTServices;
using IntelliPM.Services.CloudinaryStorageServices;
using IntelliPM.Services.DocumentCommentServices;
using IntelliPM.Services.DocumentExportService;
using IntelliPM.Services.DocumentServices;
using IntelliPM.Services.DynamicCategoryServices;
using IntelliPM.Services.EmailServices;
using IntelliPM.Services.EpicCommentServices;
using IntelliPM.Services.EpicFileServices;
using IntelliPM.Services.EpicServices;
using IntelliPM.Services.GeminiServices;
using IntelliPM.Services.Helper.DecodeTokenHandler;
using IntelliPM.Services.Helper.DynamicCategoryHelper;
using IntelliPM.Services.Helper.MapperProfiles;
using IntelliPM.Services.Helper.VerifyCode;
using IntelliPM.Services.JWTServices;
using IntelliPM.Services.LabelServices;
using IntelliPM.Services.MeetingDocumentServices;
using IntelliPM.Services.MeetingLogServices;
using IntelliPM.Services.MeetingParticipantServices;
using IntelliPM.Services.MeetingRescheduleRequestServices;
using IntelliPM.Services.MeetingServices;
using IntelliPM.Services.MeetingSummaryServices;
using IntelliPM.Services.MeetingTranscriptServices;
using IntelliPM.Services.MilestoneCommentServices;
using IntelliPM.Services.MilestoneFeedbackServices;
using IntelliPM.Services.MilestoneServices;
using IntelliPM.Services.NotificationServices;
using IntelliPM.Services.ProjectMemberServices;
using IntelliPM.Services.ProjectMetricServices;
using IntelliPM.Services.ProjectPositionServices;
using IntelliPM.Services.ProjectRecommendationServices;
using IntelliPM.Services.ProjectServices;
using IntelliPM.Services.RecipientNotificationServices;
using IntelliPM.Services.RequirementServices;
using IntelliPM.Services.RiskCommentServices;
using IntelliPM.Services.RiskFileServices;
using IntelliPM.Services.RiskServices;
using IntelliPM.Services.RiskSolutionServices;
using IntelliPM.Services.SprintServices;
using IntelliPM.Services.SubtaskCommentServices;
using IntelliPM.Services.SubtaskFileServices;
using IntelliPM.Services.SubtaskServices;
using IntelliPM.Services.SystemConfigurationServices;
using IntelliPM.Services.TaskAssignmentServices;
using IntelliPM.Services.TaskCommentServices;
using IntelliPM.Services.TaskDependencyServices;
using IntelliPM.Services.TaskFileServices;
using IntelliPM.Services.TaskServices;
using IntelliPM.Services.WorkItemLabelServices;
using IntelliPM.Services.WorkLogServices;
using IntelliPM.Shared.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddHangfire(config =>
{
    config.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddHangfireServer();



//------------------------------AUTOMAPPER---------------------------
builder.Services.AddAutoMapper(typeof(MapperProfiles).Assembly);

//-------------------------REPOSITORIES-------------------------------
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IDynamicCategoryRepository, DynamicCategoryRepository>();
builder.Services.AddScoped<ISystemConfigurationRepository, SystemConfigurationRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IEpicRepository, EpicRepository>();
builder.Services.AddScoped<IEpicFileRepository, EpicFileRepository>();
builder.Services.AddScoped<IMilestoneRepository, MilestoneRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
builder.Services.AddScoped<IRequirementRepository, RequirementRepository>();
builder.Services.AddScoped<IMeetingRepository, MeetingRepository>();
builder.Services.AddScoped<IMeetingParticipantRepository, MeetingParticipantRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<ISubtaskRepository, SubtaskRepository>();
builder.Services.AddScoped<ITaskCommentRepository, TaskCommentRepository>();
builder.Services.AddScoped<IMeetingSummaryRepository, MeetingSummaryRepository>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
builder.Services.AddScoped<IDocumentExportFileRepository, DocumentExportFileRepository>();
builder.Services.AddScoped<IDocumentPermissionRepository, DocumentPermissionRepository>();
builder.Services.AddScoped<IMeetingLogRepository, MeetingLogRepository>();
builder.Services.AddScoped<IMeetingTranscriptRepository, MeetingTranscriptRepository>();
builder.Services.AddScoped<IMilestoneFeedbackRepository, MilestoneFeedbackRepository>();
builder.Services.AddScoped<ITaskFileRepository, TaskFileRepository>();
builder.Services.AddScoped<ITaskAssignmentRepository, TaskAssignmentRepository>();
builder.Services.AddScoped<ISprintRepository, SprintRepository>();
builder.Services.AddScoped<IProjectPositionRepository, ProjectPositionRepository>();
builder.Services.AddScoped<IProjectMetricRepository, ProjectMetricRepository>();
builder.Services.AddScoped<ISubtaskFileRepository, SubtaskFileRepository>();
builder.Services.AddScoped<ISubtaskCommentRepository, SubtaskCommentRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IRiskRepository, RiskRepository>();
builder.Services.AddScoped<IRiskSolutionRepository, RiskSolutionRepository>();
builder.Services.AddScoped<IMeetingRescheduleRequestRepository, MeetingRescheduleRequestRepository>();
builder.Services.AddScoped<IEpicCommentRepository, EpicCommentRepository>();
builder.Services.AddScoped<ILabelRepository, LabelRepository>();
builder.Services.AddScoped<IWorkItemLabelRepository, WorkItemLabelRepository>();
builder.Services.AddScoped<IProjectRecommendationRepository, ProjectRecommendationRepository>();
builder.Services.AddScoped<ITaskDependencyRepository, TaskDependencyRepository>();
builder.Services.AddScoped<IWorkLogRepository, WorkLogRepository>();
builder.Services.AddScoped<IRecipientNotificationRepository, RecipientNotificationRepository>();
builder.Services.AddScoped<IRiskFileRepository, RiskFileRepository>();
builder.Services.AddScoped<IRiskCommentRepository, RiskCommentRepository>();
builder.Services.AddScoped<IMeetingDocumentRepository, MeetingDocumentRepository>();
builder.Services.AddScoped<IMilestoneCommentRepository, MilestoneCommentRepository>();
builder.Services.AddScoped<IDocumentCommentRepository, DocumentCommentRepository>();
builder.Services.AddScoped<IAiResponseHistoryRepository, AiResponseHistoryRepository>();
builder.Services.AddScoped<IAiResponseEvaluationRepository, AiResponseEvaluationRepository>();

//--------------------------SERVICES---------------------------------
builder.Services.AddScoped<IJWTService, JWTService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ICloudinaryStorageService, CloudinaryStorageService>();
builder.Services.AddScoped<IDecodeTokenHandler, DecodeTokenHandler>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IDynamicCategoryService, DynamicCategoryService>();
builder.Services.AddScoped<ISystemConfigurationService, SystemConfigurationService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IEpicService, EpicService>();
builder.Services.AddScoped<IEpicFileService, EpicFileService>();
builder.Services.AddScoped<IMilestoneService, MilestoneService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IProjectMemberService, ProjectMemberService>();
builder.Services.AddScoped<IRequirementService, RequirementService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<IMeetingParticipantService, MeetingParticipantService>();
builder.Services.AddScoped<ISubtaskService, SubtaskService>();
builder.Services.AddScoped<ITaskCommentService, TaskCommentService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IMeetingSummaryService, MeetingSummaryService>();
builder.Services.AddScoped<ITaskFileService, TaskFileService>();
builder.Services.AddScoped<ITaskAssignmentService, TaskAssignmentService>();
builder.Services.AddScoped<ITaskPlanningService, TaskPlanningService>();
builder.Services.AddScoped<IProjectPositionService, ProjectPositionService>();
builder.Services.AddScoped<ISprintService, SprintService>();
builder.Services.AddScoped<IProjectMetricService, ProjectMetricService>();
builder.Services.AddScoped<IGeminiService, GeminiService>();
builder.Services.AddScoped<IMeetingLogService, MeetingLogService>();
builder.Services.AddScoped<IMeetingTranscriptService, MeetingTranscriptService>();
builder.Services.AddScoped<IMilestoneFeedbackService, MilestoneFeedbackService>();
builder.Services.AddScoped<ISubtaskFileService, SubtaskFileService>();
builder.Services.AddScoped<ISubtaskCommentService, SubtaskCommentService>();
builder.Services.AddScoped<IRiskService, RiskService>();
builder.Services.AddScoped<IMeetingRescheduleRequestService, MeetingRescheduleRequestService>();
builder.Services.AddScoped<IEpicCommentService, EpicCommentService>();
builder.Services.AddScoped<ILabelService, LabelService>();
builder.Services.AddScoped<IWorkItemLabelService, WorkItemLabelService>();
builder.Services.AddHttpClient<IDocumentService, DocumentService>();
builder.Services.AddScoped<IProjectRecommendationService, ProjectRecommendationService>();
builder.Services.AddScoped<IWorkLogService, WorkLogService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<IRecipientNotificationService, RecipientNotificationService>();
builder.Services.AddScoped<IRiskSolutionService, RiskSolutionService>();
builder.Services.AddScoped<IRiskFileService, RiskFileService>();
builder.Services.AddScoped<IRiskCommentService, RiskCommentService>();
builder.Services.AddScoped<INotificationPushService, SignalRNotificationPushService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMeetingDocumentService, MeetingDocumentService>();
builder.Services.AddScoped<ITaskDependencyService, TaskDependencyService>();
builder.Services.AddScoped<IMilestoneCommentService, MilestoneCommentService>();
builder.Services.AddScoped<DocumentExportService>();
builder.Services.AddScoped<IDocumentCommentService, DocumentCommentService>();
builder.Services.AddScoped<IAiResponseHistoryService, AiResponseHistoryService>();
builder.Services.AddScoped<IAiResponseEvaluationService, AiResponseEvaluationService>();
builder.Services.AddSignalR();
builder.Services.AddScoped<ISprintPlanningService, SprintPlanningService>();
builder.Services.AddTransient<CloudConvertService>();
builder.Services.AddScoped<ISprintTaskPlanningService, SprintTaskPlanningService>();
builder.Services.AddScoped<IDynamicCategoryHelper, DynamicCategoryHelper>();


// ------------------------- HttpClient -----------------------------
builder.Services.AddHttpClient<ITaskPlanningService, TaskPlanningService>(client =>
{
    client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/");
});
builder.Services.AddHttpClient<IGeminiService, GeminiService>();
builder.Services.AddHttpClient<IChatGPTService, ChatGPTService>();



//----------------------------DB-----------------------------------
builder.Services.AddDbContext<Su25Sep490IntelliPmContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));




//------------------------------CORS--------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAll",
                      policy =>
                      {
                          policy.AllowAnyMethod()
                         .AllowAnyHeader()
                         .SetIsOriginAllowed(_ => true)
                         .AllowCredentials();
                      });
});

builder.Services.AddRouting(options => options.LowercaseUrls = true);



//----------------------AUTHENTICATION------------------------------


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:JwtKey"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
    };
});


//--------------------------AUTHORIZATION---------------------------
builder.Services.AddAuthorization();


//------------------------------------------------------------------


builder.Services.AddScoped<VerificationCodeCache>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(option =>
{
    ////JWT Config
    option.DescribeAllParametersInCamelCase();
    option.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

//appsettings
//builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
//builder.Services.AddSignalR().AddAzureSignalR(builder.Configuration["Azure:SignalR:ConnectionString"]!);






var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    c.RoutePrefix = "swagger";
});



app.UseCors("AllowAll");

app.UseMiddleware<DynamicCategoryValidationMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();



app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<NotificationHub>("/hubs/notification");


app.UseHangfireDashboard();  
app.UseHangfireServer();

RecurringJob.AddOrUpdate<IWorkLogService>(
    "generate-daily-worklog",
    x => x.GenerateDailyWorkLogsAsync(),
     "0 17 * * *"
     //"0 1 * * *"
     // "*/1 * * * *"
);

app.UseDefaultFiles();   
app.UseStaticFiles();

app.MapControllers();
app.Run();
