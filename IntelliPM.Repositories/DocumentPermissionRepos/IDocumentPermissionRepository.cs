using IntelliPM.Data.DTOs.DocumentPermission;
using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.DocumentPermissionRepos
{
    public interface IDocumentPermissionRepository
    {
        Task<List<DocumentPermission>> GetByDocumentIdAsync(int documentId);
        Task<string?> GetPermissionTypeAsync(int documentId, int accountId);

        Task<List<DocumentPermission>> GetByDocumentAsync(int documentId);



        Task UpdateAsync(DocumentPermission permission);


        Task<List<SharedUserDTO>> GetSharedUsersByDocumentIdAsync(int documentId);
        Task AddRangeAsync(IEnumerable<DocumentPermission> permissions);
        void RemoveRange(IEnumerable<DocumentPermission> permissions);
        Task SaveChangesAsync();
    }

}
