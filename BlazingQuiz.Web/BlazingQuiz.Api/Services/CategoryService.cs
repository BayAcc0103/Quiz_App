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
        public async Task<QuizApiResponse> SaveCategoryAsync(CategoryDto dto)
        {
            if(await _context.Categories
                .AsNoTracking()
                .AnyAsync(c => c.Name == dto.Name && c.Id != dto.Id))
            {
                // Category with the same name already exists, throw error or return some response
                return QuizApiResponse.Failure("Category with same name exists already");
            }
            if(dto.Id == 0)
            {
                // Add new category
                var category = new Category
                {
                    Name = dto.Name
                };
                _context.Categories.Add(category);
            }
            else
            {
                // Update existing category
               var dbCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == dto.Id);
                if (dbCategory == null)
                {
                    //category does not exist, throw error, or send some error response
                    return QuizApiResponse.Failure("Category does not exists");
                }
                dbCategory.Name = dto.Name;
                _context.Categories.Update(dbCategory);
            }
            await _context.SaveChangesAsync();
            return QuizApiResponse.Success();
        }

        public async Task<CategoryDto[]> GetCategoriesAsync() =>
                await _context.Categories.AsNoTracking()
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToArrayAsync();
    }
}
