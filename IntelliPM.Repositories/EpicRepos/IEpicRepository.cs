using IntelliPM.Data.Contexts;
using IntelliPM.Data.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.EpicRepos
{
    public interface IEpicRepository
    {
        Task<List<Epic>> GetAllEpics();
        Task<Epic?> GetByIdAsync(string id);
        Task<List<Epic>> GetByNameAsync(string name);
        Task<List<Epic>> GetByProjectKeyAsync(string projectKey); 
        Task Add(Epic epic);
        Task Update(Epic epic);
        Task Delete(Epic epic);
        Task<List<Epic>> GetByAccountIdAsync(int accountId);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task SaveChangesAsync();
        Task AddRangeAsync(List<Epic> epics);
        Su25Sep490IntelliPmContext GetContext();
    }
}
