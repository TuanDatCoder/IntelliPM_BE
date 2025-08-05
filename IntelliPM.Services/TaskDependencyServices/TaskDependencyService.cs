using AutoMapper;
using IntelliPM.Data.DTOs.TaskDependency.Request;
using IntelliPM.Data.DTOs.TaskDependency.Response;
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

        public async Task<TaskDependencyResponseDTO> CreateAsync(TaskDependencyRequestDTO dto)
        {
            var entity = _mapper.Map<TaskDependency>(dto);
            await _taskDependencyRepo.Add(entity);
            return _mapper.Map<TaskDependencyResponseDTO>(entity);
        }

        //public async Task<List<TaskDependencyResponseDTO>> CreateManyAsync(List<TaskDependencyRequestDTO> dtos)
        //{
        //    var entities = _mapper.Map<List<TaskDependency>>(dtos);
        //    await _taskDependencyRepo.AddMany(entities);
        //    return _mapper.Map<List<TaskDependencyResponseDTO>>(entities);
        //}

        public async Task<List<TaskDependencyResponseDTO>> CreateManyAsync(List<TaskDependencyIdRequestDTO> dtos)
        {
            var createDtos = dtos.Where(x => x.Id == 0).ToList();
            var updateDtos = dtos.Where(x => x.Id != 0).ToList();

            var createEntities = _mapper.Map<List<TaskDependency>>(createDtos);
            var updateEntities = _mapper.Map<List<TaskDependency>>(updateDtos);

            if (createEntities.Any())
            {
                await _taskDependencyRepo.AddMany(createEntities);
            }

            if (updateEntities.Any())
            {
                await _taskDependencyRepo.UpdateMany(updateEntities);
            }

            var result = new List<TaskDependency>();
            result.AddRange(createEntities);
            result.AddRange(updateEntities);

            return _mapper.Map<List<TaskDependencyResponseDTO>>(result);
        }

        public async Task<bool> DeleteConnectionAsync(string linkedFrom, string linkedTo)
        {
            var dependency = await _taskDependencyRepo.GetByConnectionAsync(linkedFrom, linkedTo);
            if (dependency == null) return false;

            await _taskDependencyRepo.DeleteAsync(dependency);
            return true;
        }

        public async Task<bool> DeleteTaskDependencyAsync(int id)
        {
            var existing = await _taskDependencyRepo.GetByIdAsync(id);
            if (existing == null)
                return false;

            await _taskDependencyRepo.DeleteAsync(existing);
            return true;
        }

        public async Task<List<TaskDependencyResponseDTO>> GetByLinkedFromAsync(string linkedFrom)
        {
            var dependencies = await _taskDependencyRepo.GetDependenciesByLinkedFromAsync(linkedFrom);
            return _mapper.Map<List<TaskDependencyResponseDTO>>(dependencies);
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
