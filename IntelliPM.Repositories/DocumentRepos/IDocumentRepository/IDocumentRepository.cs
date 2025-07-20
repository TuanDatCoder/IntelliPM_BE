using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelliPM.Data.Entities;
namespace IntelliPM.Repositories.DocumentRepos
{
 

    public interface IDocumentRepository
    {
        Task<List<Document>> GetByProjectAsync(int projectId);
        Task<List<Document>> GetAllAsync();
        Task<Document?> GetByIdAsync(int id);
        Task AddAsync(Document doc);
        Task UpdateAsync(Document doc);
        Task SaveChangesAsync();

        Task<List<Document>> GetByUserIdAsync(int userId);

        Task<List<Document>> GetByStatusAsync(string status);

        Task<List<Document>> GetByStatusAndProjectAsync(string status, int projectId);

    
        Task<List<Document>> GetByEpicIdAsync(string epicId);
        Task<List<Document>> GetByTaskIdAsync(string taskId);
        Task<List<Document>> GetBySubtaskIdAsync(string subtaskId);

   
        Task<List<Document>> GetByProjectAndTaskAsync(int projectId, string taskId);

        Task<Document?> GetByKeyAsync(int projectId, string? epicId, string? taskId, string? subTaskId);

        Task<Dictionary<string, int>> GetUserDocumentMappingAsync(int projectId, int userId);

        Task<Dictionary<string, int>> CountByStatusAsync();

        Task<Dictionary<string, int>> CountByStatusInProjectAsync(int projectId);




    }

}
