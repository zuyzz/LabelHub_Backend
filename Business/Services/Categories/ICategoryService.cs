using DataLabelProject.Application.DTOs.Categories;

namespace DataLabelProject.Business.Services.Categories
{
    public interface ICategoryService
    {
        Task<List<CategoryResponse>> GetAllAsync();
        Task<CategoryResponse?> GetByIdAsync(Guid id);
        Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, Guid? createdBy);
        Task<bool> UpdateAsync(Guid id, UpdateCategoryRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
