using IntelliPM.Data.DTOs.DocumentRequestMeeting;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.DocumentRequestMeetingRepos;
using IntelliPM.Services.CloudinaryStorageServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.DocumentRequestMeetingServices
{
    public class DocumentRequestMeetingService : IDocumentRequestMeetingService
    {
        private readonly IDocumentRequestMeetingRepository _repo;
        private readonly ICloudinaryStorageService _cloudinary;

        public DocumentRequestMeetingService(IDocumentRequestMeetingRepository repo, ICloudinaryStorageService cloudinary)
        {
            _repo = repo;
            _cloudinary = cloudinary;
        }

        public async Task<DocumentRequestMeetingResponseDTO> CreateAsync(CreateDocumentRequestMeetingDTO dto)
        {
            var stream = dto.File.OpenReadStream();
            var fileUrl = await _cloudinary.UploadFileAsync(stream, dto.File.FileName);

            var entity = new DocumentRequestMeeting
            {
                FileUrl = fileUrl,
                TeamLeaderId = dto.TeamLeaderId,
                ProjectManagerId = dto.ProjectManagerId,
                Status = dto.Status,
                Reason = dto.Reason,
                FeedbackId = dto.FeedbackId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            return MapToDTO(entity);
        }

        public async Task<DocumentRequestMeetingResponseDTO> UpdateAsync(int id, UpdateDocumentRequestMeetingDTO dto)
        {
            var entity = await _repo.GetByIdAsync(id) ?? throw new Exception("Not found");

            entity.Status = dto.Status ?? entity.Status;
            entity.Reason = dto.Reason ?? entity.Reason;

            entity.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(entity);
            await _repo.SaveChangesAsync();

            return MapToDTO(entity);
        }

        public async Task<List<DocumentRequestMeetingResponseDTO>> GetAllAsync() =>
            (await _repo.GetAllAsync()).Select(MapToDTO).ToList();

        public async Task<DocumentRequestMeetingResponseDTO?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : MapToDTO(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id) ?? throw new Exception("Not found");
            await _repo.DeleteAsync(entity);
            await _repo.SaveChangesAsync();
        }

        private static DocumentRequestMeetingResponseDTO MapToDTO(DocumentRequestMeeting d) => new()
        {
            Id = d.Id,
            FileUrl = d.FileUrl,
            TeamLeaderId = d.TeamLeaderId,
            ProjectManagerId = d.ProjectManagerId,
            Status = d.Status,
            Reason = d.Reason,
            FeedbackId = d.FeedbackId,
            SentToClient = d.SentToClient,
            ClientViewed = d.ClientViewed,
            ClientApproved = d.ClientApproved,
            ClientApprovalTime = d.ClientApprovalTime,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt
        };

        public async Task<List<DocumentRequestMeetingResponseDTO>> GetInboxForPMAsync(
       int pmId, string? status = null, bool? sentToClient = null, bool? clientViewed = null,
       int? page = null, int? pageSize = null)
        {
            int? skip = null, take = null;
            if (page.HasValue && pageSize.HasValue && page > 0 && pageSize > 0)
            {
                skip = (page.Value - 1) * pageSize.Value;
                take = pageSize.Value;
            }

            var list = await _repo.GetByProjectManagerAsync(pmId, status, sentToClient, clientViewed, skip, take);
            return list.Select(MapToDTO).ToList();
        }


    }

}
