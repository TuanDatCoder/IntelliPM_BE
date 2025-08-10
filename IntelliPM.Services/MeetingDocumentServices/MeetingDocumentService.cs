using IntelliPM.Data.DTOs.MeetingDocument;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.MeetingDocumentRepos;

namespace IntelliPM.Services.MeetingDocumentServices
{
    public class MeetingDocumentService : IMeetingDocumentService
    {
        private readonly IMeetingDocumentRepository _repo;

        public MeetingDocumentService(IMeetingDocumentRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<MeetingDocumentResponseDTO>> GetAllAsync()
        {
            var docs = await _repo.GetAllAsync();
            return docs.Select(ToResponse).ToList();
        }

        public async Task<MeetingDocumentResponseDTO?> GetByMeetingIdAsync(int meetingId)
        {
            var doc = await _repo.GetByMeetingIdAsync(meetingId);
            return doc == null ? null : ToResponse(doc);
        }

        public async Task<MeetingDocumentResponseDTO> CreateAsync(MeetingDocumentRequestDTO request)
        {
            var doc = new MeetingDocument
            {
                MeetingId = request.MeetingId,
                Title = request.Title,
                Description = request.Description,
                FileUrl = request.FileUrl,
                IsActive = request.IsActive,
                AccountId = request.AccountId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(doc);
            await _repo.SaveChangesAsync();
            return ToResponse(doc);
        }

        public async Task<MeetingDocumentResponseDTO> UpdateAsync(int meetingId, MeetingDocumentRequestDTO request)
        {
            var doc = await _repo.GetByMeetingIdAsync(meetingId);
            if (doc == null) throw new Exception("MeetingDocument not found");
            doc.Title = request.Title;
            doc.Description = request.Description;
            doc.FileUrl = request.FileUrl;
            doc.IsActive = request.IsActive;
            doc.AccountId = request.AccountId;
            doc.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(doc);
            await _repo.SaveChangesAsync();
            return ToResponse(doc);
        }

        public async Task DeleteAsync(int meetingId)
        {
            var doc = await _repo.GetByMeetingIdAsync(meetingId);
            if (doc == null) throw new Exception("MeetingDocument not found");
            await _repo.DeleteAsync(doc);
            await _repo.SaveChangesAsync();
        }

        private static MeetingDocumentResponseDTO ToResponse(MeetingDocument doc)
        {
            return new MeetingDocumentResponseDTO
            {
                MeetingId = doc.MeetingId,
                Title = doc.Title,
                Description = doc.Description,
                FileUrl = doc.FileUrl,
                IsActive = doc.IsActive,
                AccountId = doc.AccountId,
                CreatedAt = doc.CreatedAt,
                UpdatedAt = doc.UpdatedAt
            };
        }
    }
}