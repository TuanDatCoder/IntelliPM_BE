﻿using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.TaskDependencyRepos
{
    public interface ITaskDependencyRepository
    {
        Task<List<TaskDependency>> GetByProjectIdAsync(int projectId);
        Task<List<TaskDependency>> GetByTaskIdAsync(string taskId);
        Task DeleteByTaskIdAsync(string taskId);
        Task AddRangeAsync(List<TaskDependency> dependencies);
        Task SaveChangesAsync();
    }

}
