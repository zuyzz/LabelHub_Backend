using DataLabelProject.Application.DTOs.Categories;
using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Services.Users;
using DataLabelProject.Data.Repositories.Abstractions;

namespace DataLabelProject.Business.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICurrentUserService _currentUserService;

        public CategoryService(ICategoryRepository categoryRepository, ICurrentUserService currentUserService)
        {
            _categoryRepository = categoryRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResponse<CategoryResponse>> GetCategories(CategoryQueryParameters @params)
        {
            var (categories, totalCount) = await _categoryRepository.GetAllAsync(@params);

            return new PagedResponse<CategoryResponse>
            {
                Items = categories.Select(MapToResponse).ToList(),
                TotalItems = totalCount,
                Page = @params.Page,
                PageSize = @params.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)@params.PageSize)
            };
        }

        public async Task<CategoryResponse?> GetCategoryById(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return null;

            return MapToResponse(category);
        }

        public async Task<CategoryResponse> CreateCategory(CreateCategoryRequest request)
        {
            var category = new Category
            {
                CategoryId = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUserService.UserId!.Value
            };

            await _categoryRepository.CreateAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return MapToResponse(category);
        }

        public async Task<CategoryResponse?> UpdateCategory(Guid id, UpdateCategoryRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return null;

            if (!string.IsNullOrWhiteSpace(request.Name))
                category.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.Description))
                category.Description = request.Description;

            if (request.IsActive.HasValue)
                category.IsActive = request.IsActive.Value;

            await _categoryRepository.UpdateAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return MapToResponse(category);
        }

        private CategoryResponse MapToResponse(Category category)
        {
            return new CategoryResponse
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt
            };
        }
    }
}
