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
        Task<Document?> GetByIdAsync(int id);
        Task AddAsync(Document doc);
        Task UpdateAsync(Document doc);
        Task SaveChangesAsync();
    }

}
