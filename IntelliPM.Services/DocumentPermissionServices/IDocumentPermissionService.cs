using IntelliPM.Data.DTOs.DocumentPermission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.DocumentPermissionServices
{
    public interface IDocumentPermissionService
    {
        Task<bool> UpdatePermissionTypeAsync(int documentId,  string newType);

        Task<List<SharedUserDTO>> GetSharedUsersAsync(int documentId);


    }
}
