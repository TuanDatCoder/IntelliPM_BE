using ConstructionEquipmentRental.API.Middlewares;
using IntelliPM.Data.Contexts;
using IntelliPM.Repositories.AccountRepos;
using IntelliPM.Repositories.DocumentRepos;
using IntelliPM.Repositories.DocumentRepos.DocumentRepository;
using IntelliPM.Repositories.DynamicCategoryRepos;
using IntelliPM.Repositories.EpicRepos;
using IntelliPM.Repositories.MeetingLogRepos;
using IntelliPM.Repositories.MeetingParticipantRepos;
using IntelliPM.Repositories.MeetingRepos;
using IntelliPM.Repositories.MeetingTranscriptRepos;
using IntelliPM.Repositories.MilestoneRepos;
using IntelliPM.Repositories.ProjectMemberRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.RefreshTokenRepos;
using IntelliPM.Repositories.RequirementRepos;
using IntelliPM.Repositories.SystemConfigurationRepos;
using IntelliPM.Repositories.TaskCheckListRepos;
using IntelliPM.Repositories.TaskCommentRepos;
using IntelliPM.Repositories.TaskRepos;
using IntelliPM.Services.AccountServices;
using IntelliPM.Services.AdminServices;
using IntelliPM.Services.AuthenticationServices;
using IntelliPM.Services.CloudinaryStorageServices;
using IntelliPM.Services.DocumentServices;
using IntelliPM.Services.DynamicCategoryServices;
using IntelliPM.Services.EmailServices;
using IntelliPM.Services.EpicServices;
using IntelliPM.Services.Helper.DecodeTokenHandler;
using IntelliPM.Services.Helper.MapperProfiles;
using IntelliPM.Services.Helper.VerifyCode;
using IntelliPM.Services.JWTServices;
using IntelliPM.Services.MeetingLogServices;
using IntelliPM.Services.MeetingParticipantServices;
using IntelliPM.Services.MeetingServices;
using IntelliPM.Services.MeetingTranscriptServices;
using IntelliPM.Services.MilestoneServices;
using IntelliPM.Services.ProjectMemberServices;
using IntelliPM.Services.ProjectServices;
using IntelliPM.Services.RequirementServices;
using IntelliPM.Services.SystemConfigurationServices;
using IntelliPM.Services.TaskCheckListServices;
using IntelliPM.Services.TaskCommentServices;
using IntelliPM.Services.TaskServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


//------------------------------AUTOMAPPER---------------------------
builder.Services.AddAutoMapper(typeof(MapperProfiles).Assembly);



//-------------------------REPOSITORIES-------------------------------
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IDynamicCategoryRepository, DynamicCategoryRepository>();
builder.Services.AddScoped<ISystemConfigurationRepository, SystemConfigurationRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IEpicRepository, EpicRepository>();
builder.Services.AddScoped<IMilestoneRepository, MilestoneRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
builder.Services.AddScoped<IRequirementRepository, RequirementRepository>();
builder.Services.AddScoped<IMeetingRepository, MeetingRepository>();
builder.Services.AddScoped<IMeetingParticipantRepository, MeetingParticipantRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<ITaskCheckListRepository, TaskCheckListRepository>();
builder.Services.AddScoped<ITaskCommentRepository, TaskCommentRepository>();
//
builder.Services.AddScoped<IMeetingLogRepository, MeetingLogRepository>();
builder.Services.AddScoped<IMeetingTranscriptRepository, MeetingTranscriptRepository>();




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
builder.Services.AddScoped<IMilestoneService, MilestoneService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IProjectMemberService, ProjectMemberService>();
builder.Services.AddScoped<IRequirementService, RequirementService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<IMeetingParticipantService, MeetingParticipantService>();
builder.Services.AddScoped<ITaskCheckListService, TaskCheckListService>();
builder.Services.AddScoped<ITaskCommentService, TaskCommentService>();

builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IMeetingLogService, MeetingLogService>();
builder.Services.AddScoped<IMeetingTranscriptService, MeetingTranscriptService>();

//----------------------------DB-----------------------------------
builder.Services.AddDbContext<Su25Sep490IntelliPmContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));




//------------------------------CORS--------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAll",
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
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
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);






var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    c.RoutePrefix = "swagger";
});


app.UseMiddleware<ExceptionMiddleware>();

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
