using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.LabelRepos
{
    public interface ILabelRepository
    {
        Task Add(Label label);
        Task Delete(Label label);
        Task<List<Label>> GetAllLabelAsync();
        Task<Label?> GetByIdAsync(int id);
        Task Update(Label label);
        Task<List<Label>> GetByProjectAsync(int projectId);
    }
}
