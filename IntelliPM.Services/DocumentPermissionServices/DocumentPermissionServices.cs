using IntelliPM.Data.Contexts;
using IntelliPM.Data.DTOs.DocumentPermission;
using IntelliPM.Repositories.DocumentPermissionRepos;
using IntelliPM.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace IntelliPM.Services.DocumentPermissionServices
{
    public class DocumentPermissionServices : IDocumentPermissionService
    {
        private readonly IDocumentPermissionRepository _repo;
        private readonly Su25Sep490IntelliPmContext _context;
        private readonly IHubContext<DocumentHub> _hubContext;

        public DocumentPermissionServices(
            IDocumentPermissionRepository repo,
            Su25Sep490IntelliPmContext context,
           IHubContext<DocumentHub> hubContext)

        {
            _repo = repo;
            _context = context;
            _hubContext = hubContext;
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
            await _hubContext.Clients
    .Group($"document-{documentId}")
    .SendAsync("PermissionChanged", documentId);

            return true;
        }

        public async Task<List<SharedUserDTO>> GetSharedUsersAsync(int documentId)
        {
            return await _repo.GetSharedUsersByDocumentIdAsync(documentId);
        }

        public async Task<string?> GetPermissionTypeAsync(int documentId)
        {
            var permissionType = await _repo.GetPermissionTypeByDocumentIdAsync(documentId);
            if (permissionType == null)
                throw new KeyNotFoundException("Permission not found for this document.");
            return permissionType;
        }


    }
}

