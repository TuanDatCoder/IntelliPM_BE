using IntelliPM.Data.Contexts;
using IntelliPM.Data.DTOs.DocumentPermission;
using IntelliPM.Repositories.DocumentPermissionRepos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.DocumentPermissionServices
{
    public class DocumentPermissionServices : IDocumentPermissionService
    {
        private readonly IDocumentPermissionRepository _repo;
        private readonly Su25Sep490IntelliPmContext _context;

        public DocumentPermissionServices(
            IDocumentPermissionRepository repo,
            Su25Sep490IntelliPmContext context)
        {
            _repo = repo;
            _context = context;
        }

        public async Task<bool> UpdatePermissionTypeAsync(int documentId, string newType)
        {
            var permissions = await _repo.GetByDocumentAsync(documentId);
            if (permissions == null || !permissions.Any())
                return false;

            foreach (var p in permissions)
            {
                p.PermissionType = newType;
                await _repo.UpdateAsync(p);
            }

            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<List<SharedUserDTO>> GetSharedUsersAsync(int documentId)
        {
            return await _repo.GetSharedUsersByDocumentIdAsync(documentId);
        }


    }
}

