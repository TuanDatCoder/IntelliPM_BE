using IntelliPM.Data.Entities;
using IntelliPM.Repositories.EpicRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.TaskRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.Utilities
{
    public static class IdGenerator
    {
        public static async Task<string> GenerateNextId(string projectKey, IEpicRepository epicRepo, ITaskRepository taskRepo, IProjectRepository projectRepo)
        {
            if (string.IsNullOrEmpty(projectKey))
                throw new ArgumentException("Project key cannot be null or empty.");

            // Lấy project để lấy ID hoặc xác nhận tồn tại
            var project = await projectRepo.GetProjectByKeyAsync(projectKey);
            if (project == null)
                throw new KeyNotFoundException($"Project with key '{projectKey}' not found.");

            var allEpics = await epicRepo.GetByProjectKeyAsync(projectKey);
            var allTasks = taskRepo != null ? await taskRepo.GetByProjectIdAsync(project.Id) : new List<Tasks>();

            var allIds = new List<string>();
            allIds.AddRange(allEpics.Select(e => e.Id));
            allIds.AddRange(allTasks.Select(t => t.Id));

            int maxNumber = 0;
            foreach (var id in allIds)
            {
                var numberPart = id.Split('-').Last();
                if (int.TryParse(numberPart, out int number) && number > maxNumber)
                {
                    maxNumber = number;
                }
            }

            return $"{projectKey}-{maxNumber + 1}";
        }

    }
}
