using AutoMapper;
using IntelliPM.Data.DTOs.DynamicCategory.Request;
using IntelliPM.Data.DTOs.DynamicCategory.Response;
using IntelliPM.Data.Entities;
using IntelliPM.Repositories.DynamicCategoryRepos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliPM.Services.DynamicCategoryServices
{
    public class DynamicCategoryService : IDynamicCategoryService
    {
        private readonly IMapper _mapper;
        private readonly IDynamicCategoryRepository _repo;
        private readonly ILogger<DynamicCategoryService> _logger;

        public DynamicCategoryService(IMapper mapper, IDynamicCategoryRepository repo, ILogger<DynamicCategoryService> logger)
        {
            _mapper = mapper;
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<DynamicCategoryResponseDTO>> GetAllDynamicCategories()
        {
            var entities = await _repo.GetDynamicCategories();
            return _mapper.Map<List<DynamicCategoryResponseDTO>>(entities);
        }

        public async Task<DynamicCategoryResponseDTO> GetDynamicCategoryById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Dynamic category with ID {id} not found or inactive.");

            return _mapper.Map<DynamicCategoryResponseDTO>(entity);
        }

        public async Task<List<DynamicCategoryResponseDTO>> GetDynamicCategoryByNameOrCategoryGroup(string name, string categoryGroup)
        {
            List<DynamicCategory> entities;

            // Nếu không nhập gì, trả về toàn bộ danh sách
            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(categoryGroup))
            {
                entities = await _repo.GetDynamicCategories();
            }
            else
            {
                entities = await _repo.GetByNameOrCategoryGroupAsync(name, categoryGroup);
            }

            if (!entities.Any())
            {
                throw new KeyNotFoundException("No matching dynamic categories found.");
            }

            return _mapper.Map<List<DynamicCategoryResponseDTO>>(entities);
        }



        public async Task<List<DynamicCategoryResponseDTO>> GetDynamicCategoryByCategoryGroup(string categoryGroup)
        {
            List<DynamicCategory> entities;

            // Nếu không nhập gì, trả về toàn bộ danh sách
            if (string.IsNullOrEmpty(categoryGroup))
            {
                entities = await _repo.GetDynamicCategories();
            }
            else
            {
                entities = await _repo.GetByCategoryGroupAsync(categoryGroup);
            }

            if (!entities.Any())
            {
                throw new KeyNotFoundException("No matching dynamic categories found.");
            }

            return _mapper.Map<List<DynamicCategoryResponseDTO>>(entities);
        }



        public async Task<DynamicCategoryResponseDTO> CreateDynamicCategory(DynamicCategoryRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            var entity = _mapper.Map<DynamicCategory>(request);
            entity.IsActive = true;

            await _repo.Add(entity);

            return _mapper.Map<DynamicCategoryResponseDTO>(entity);
        }

        public async Task<DynamicCategoryResponseDTO> UpdateDynamicCategory(int id, DynamicCategoryRequestDTO request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Dynamic category with ID {id} not found or inactive.");

            _mapper.Map(request, entity);
            await _repo.Update(entity);

            return _mapper.Map<DynamicCategoryResponseDTO>(entity);
        }

        public async Task DeleteDynamicCategory(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Dynamic category with ID {id} not found or inactive.");

            await _repo.Delete(entity);
        }

        public async Task<DynamicCategoryResponseDTO> ChangeDynamicCategoryStatus(int id, bool isActive)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Dynamic category with ID {id} not found or inactive.");

            entity.IsActive = isActive;
            await _repo.Update(entity);

            return _mapper.Map<DynamicCategoryResponseDTO>(entity);
        }
    }
}