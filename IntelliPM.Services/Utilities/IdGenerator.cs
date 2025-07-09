using IntelliPM.Data.Entities;
using IntelliPM.Repositories.EpicRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliPM.Services.Utilities
{
    public static class IdGenerator
    {
        public static async Task<string> GenerateNextId(string projectKey, IEpicRepository epicRepo, ITaskRepository taskRepo, IProjectRepository projectRepo, ISubtaskRepository subtaskRepo)
        {
            if (string.IsNullOrEmpty(projectKey))
                throw new ArgumentException("Project key cannot be null or empty.");

            // Lấy project để xác nhận tồn tại và lấy ID
            var project = await projectRepo.GetProjectByKeyAsync(projectKey);
            if (project == null)
                throw new KeyNotFoundException($"Project with key '{projectKey}' not found.");

            // Lấy tất cả epic thuộc project
            var allEpics = await epicRepo.GetByProjectKeyAsync(projectKey);

            // Lấy tất cả task thuộc project, đảm bảo taskRepo không null
            var allTasks = taskRepo != null ? await taskRepo.GetByProjectIdAsync(project.Id) : new List<Tasks>();
            if (taskRepo == null)
            {
                throw new InvalidOperationException("ITaskRepository is required to generate a consistent ID across Tasks and Epics.");
            }

            var allSubtasks = new List<Subtask>();
            foreach (var task in allTasks)
            {
                var subtasks = await subtaskRepo.GetSubtaskByTaskIdAsync(task.Id);
                allSubtasks.AddRange(subtasks);
            }

            // Kết hợp tất cả ID từ cả epic và task
            var allIds = new List<string>();
            allIds.AddRange(allEpics.Select(e => e.Id).Where(id => !string.IsNullOrEmpty(id)));
            allIds.AddRange(allTasks.Select(t => t.Id).Where(id => !string.IsNullOrEmpty(id)));
            allIds.AddRange(allSubtasks.Select(s => s.Id).Where(id => !string.IsNullOrEmpty(id)));


            // Tìm số lớn nhất hiện tại từ tất cả ID
            int maxNumber = 0;
            foreach (var id in allIds)
            {
                var parts = id.Split('-');
                if (parts.Length == 2 && parts[0] == projectKey && int.TryParse(parts[1], out int number) && number > maxNumber)
                {
                    maxNumber = number;
                }
            }

            // Trả về ID mới với số tiếp theo
            return $"{projectKey}-{maxNumber + 1}";
        }
    }
}