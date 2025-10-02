using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BlazingQuiz.Api.Services
{
    public class CategoryService
    {
        private readonly QuizContext _context;
        public CategoryService(QuizContext context)
        {
            _context = context;
        }
        public async Task<QuizApiResponse<CategoryDto>> SaveCategoryAsync(CategoryDto dto)
        {
            if(await _context.Categories
                .AsNoTracking()
                .AnyAsync(c => c.Name == dto.Name && c.Id != dto.Id))
            {
                // Category with the same name already exists, throw error or return some response
                return QuizApiResponse<CategoryDto>.Failure("Category with same name exists already");
            }
            if(dto.Id == 0)
            {
                // Add new category
                var category = new Category
                {
                    Name = dto.Name,
                    ImagePath = dto.ImagePath
                };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync(); // Save to get the new ID
                
                // Return the created category with its new ID
                return QuizApiResponse<CategoryDto>.Success(new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    ImagePath = category.ImagePath
                });
            }
            else
            {
                // Update existing category
               var dbCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == dto.Id);
                if (dbCategory == null)
                {
                    //category does not exist, throw error, or send some error response
                    return QuizApiResponse<CategoryDto>.Failure("Category does not exists");
                }
                dbCategory.Name = dto.Name;
                dbCategory.ImagePath = dto.ImagePath;
                _context.Categories.Update(dbCategory);
                await _context.SaveChangesAsync();

                // Return the updated category
                return QuizApiResponse<CategoryDto>.Success(new CategoryDto
                {
                    Id = dbCategory.Id,
                    Name = dbCategory.Name,
                    ImagePath = dbCategory.ImagePath
                });
            }
        }

        public async Task<CategoryDto[]> GetCategoriesAsync() =>
                await _context.Categories.AsNoTracking()
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ImagePath = c.ImagePath
                })
                .ToArrayAsync();
    }
}
