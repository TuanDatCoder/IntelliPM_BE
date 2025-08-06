using IntelliPM.Data.DTOs.DocumentComment;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.DocumentCommentRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.DocumentCommentServices
{
    public class DocumentCommentService : IDocumentCommentService
    {
        private readonly IDocumentCommentRepository _repo;

        public DocumentCommentService(IDocumentCommentRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<DocumentCommentResponseDTO>> GetByDocumentIdAsync(int documentId)
        {
            var comments = await _repo.GetByDocumentIdAsync(documentId);

            return comments.Select(c => new DocumentCommentResponseDTO
            {
                Id = c.Id,
                DocumentId = c.DocumentId,
                //AuthorId = c.AuthorId,
                //AuthorName = c.Author?.FullName ?? "Unknown",
                Content = c.Content,
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        public async Task<DocumentCommentResponseDTO> CreateAsync(DocumentCommentRequestDTO request, int userId)

        {
            var comment = new DocumentComment
            {
                DocumentId = request.DocumentId,
                AuthorId = userId,
                Content = request.Content
            };

            await _repo.AddAsync(comment);
            var author = await _repo.GetAuthorByIdAsync(userId);

            return new DocumentCommentResponseDTO
            {
                Id = comment.Id,
                DocumentId = comment.DocumentId,
                //AuthorId = comment.AuthorId,
                //AuthorName = author?.FullName ?? "Unknown",
                Content = comment.Content,
                CreatedAt = comment.CreatedAt 
            };
        }

        public async Task<DocumentCommentResponseDTO?> GetByIdAsync(int id)
        {
            var comment = await _repo.GetByIdAsync(id);
            if (comment == null) return null;

            return new DocumentCommentResponseDTO
            {
                Id = comment.Id,
                DocumentId = comment.DocumentId,
                //AuthorId = comment.AuthorId,
                //AuthorName = comment.Author?.FullName ?? "Unknown",
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }



        public async Task<DocumentCommentResponseDTO> UpdateAsync(int id, DocumentCommentRequestDTO request, int authorId)
        {
            var comment = await _repo.GetByIdAsync(id);
            if (comment == null || comment.AuthorId != authorId)
                throw new UnauthorizedAccessException("Không có quyền hoặc comment không tồn tại");

            comment.Content = request.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _repo.UpdateAsync(comment);
            }
            catch (Exception ex)
            {
                // Ghi log ra hoặc trả lỗi cụ thể để debug
                throw new Exception($"Update failed: {ex.InnerException?.Message ?? ex.Message}");
            }

            var author = await _repo.GetAuthorByIdAsync(authorId);

            return new DocumentCommentResponseDTO
            {
                Id = comment.Id,
                DocumentId = comment.DocumentId,
                //AuthorId = authorId,
                //AuthorName = author?.FullName ?? "Unknown",
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }


        public async Task<bool> DeleteAsync(int id, int authorId)
        {
            var comment = await _repo.GetByIdAsync(id);
            if (comment == null || comment.AuthorId != authorId)
                return false;

            await _repo.DeleteAsync(comment);
            return true;
        }



    }


}
