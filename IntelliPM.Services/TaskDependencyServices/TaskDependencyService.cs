using AutoMapper;
using IntelliPM.Data.DTOs.TaskDependency.Request;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.TaskDependencyRepos;
using IntelliPM.Services.ProjectServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.TaskDependencyServices
{
    public class TaskDependencyService : ITaskDependencyService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<TaskDependencyService> _logger;
        private readonly ITaskDependencyRepository _taskDependencyRepo;

        public TaskDependencyService(IMapper mapper, ILogger<TaskDependencyService> logger, ITaskDependencyRepository taskDependencyRepo)
        {
            _mapper = mapper;
            _logger = logger;
            _taskDependencyRepo = taskDependencyRepo;
        }

        //public async Task<TaskDependency> CreateDependencyAsync(TaskDependencyRequestDTO request)
        //{
        //    bool toValid = await _taskDependencyRepo.ValidateItemExistsAsync(request.ToType, request.LinkedTo);
        //    if (!toValid)
        //        throw new Exception("Invalid From or To entity");

        //    var dependency = new TaskDependency
        //    {
        //        FromType = request.FromType,
        //        LinkedFrom = request.FromId.ToString(),
        //        ToType = request.ToType,
        //        LinkedTo = request.ToId.ToString(),
        //        Type = request.Type
        //    };

        //    _context.TaskDependencies.Add(dependency);
        //    await _context.SaveChangesAsync();

        //    return dependency;
        //}
    }
}
