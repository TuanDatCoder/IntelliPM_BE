using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.EpicRepos;
using IntelliPM.Repositories.ProjectRepos;
using IntelliPM.Repositories.SubtaskRepos;
using IntelliPM.Repositories.TaskRepos;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IntelliPM.Services.Utilities
{
    public static class IdGenerator
    {
        public static async Task<string> GenerateNextId(string projectKey, Su25Sep490IntelliPmContext context)
        {
            if (string.IsNullOrEmpty(projectKey))
                throw new ArgumentException("Project key cannot be null or empty.");

            // Kiểm tra project tồn tại
            var project = await context.Project
                .Where(p => p.ProjectKey == projectKey)
                .FirstOrDefaultAsync();

            if (project == null)
                throw new KeyNotFoundException($"Project with key '{projectKey}' not found.");

            // Tên sequence chung
            var sequenceName = $"{projectKey.ToLower()}_id_seq";
            var sequenceExistsQuery = @"
                SELECT EXISTS (
                    SELECT 1
                    FROM pg_catalog.pg_class c
                    JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace
                    WHERE c.relkind = 'S' AND c.relname = @p0
                ) AS ""Value""";
            var sequenceExists = await context.Database
                .SqlQueryRaw<bool>(sequenceExistsQuery, sequenceName)
                .FirstOrDefaultAsync();

            if (!sequenceExists)
            {
                // Tạo sequence nếu chưa tồn tại
                await context.Database.ExecuteSqlRawAsync($"CREATE SEQUENCE {sequenceName} START 1");
            }

            // Lấy số tiếp theo từ sequence
            var nextValQuery = $"SELECT NEXTVAL('{sequenceName}') AS \"Value\"";
            var nextVal = await context.Database
                .SqlQueryRaw<long>(nextValQuery)
                .FirstOrDefaultAsync();

            return $"{projectKey}-{nextVal}";
        }



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




