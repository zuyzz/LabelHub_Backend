using DataLabel_Project_BE.DTOs.Category;
using DataLabel_Project_BE.Models;
using DataLabel_Project_BE.Repositories;

namespace DataLabel_Project_BE.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<CategoryResponse>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            return categories.Select(c => new CategoryResponse
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        public async Task<CategoryResponse?> GetByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return null;

            return new CategoryResponse
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt
            };
        }

        public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, Guid? createdBy)
        {
            var category = new Category
            {
                CategoryId = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            await _categoryRepository.CreateAsync(category);

            return new CategoryResponse
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateCategoryRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            category.Name = request.Name;
            category.Description = request.Description;
            category.IsActive = request.IsActive;

            await _categoryRepository.UpdateAsync(category);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            // SOFT DELETE
            category.IsActive = false;
            await _categoryRepository.UpdateAsync(category);
            return true;
        }
    }
}
