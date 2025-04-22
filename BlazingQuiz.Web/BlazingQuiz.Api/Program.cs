using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddDbContext<QuizContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("QuizApp");
    options.UseSqlServer(connectionString);
});
var app = builder.Build();

#if DEBUG
ApplyDbMigrations(app.Services);
#endif

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();

static void ApplyDbMigrations(IServiceProvider services)
{
    var scope = services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<QuizContext>();
    if(dbContext.Database.GetPendingMigrations().Any())
    {
        dbContext.Database.Migrate();
    }
}
