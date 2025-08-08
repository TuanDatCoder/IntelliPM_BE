using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Repositories.DocumentCommentRepos
{
    public interface IDocumentCommentRepository
    {
        Task<List<DocumentComment>> GetByDocumentIdAsync(int documentId);
        Task<DocumentComment> AddAsync(DocumentComment comment);

        Task<DocumentComment?> GetByIdAsync(int id);
        
        Task UpdateAsync(DocumentComment comment);

        Task DeleteAsync(DocumentComment comment);


        Task<Account?> GetAuthorByIdAsync(int authorId);
    }

}
