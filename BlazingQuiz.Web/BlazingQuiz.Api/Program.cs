using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Api.Endpoints;
using BlazingQuiz.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization; // Add this using directive

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JSON options to handle enum as strings
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddMvc().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddTransient<IPasswordHasher<User>, PasswordHasher<User>>();

var connectionString = builder.Configuration.GetConnectionString("QuizApp");
builder.Services.AddDbContext<QuizContext>(options =>
{
    options.UseSqlServer(connectionString);
}, optionsLifetime:ServiceLifetime.Singleton);
builder.Services.AddDbContextFactory<QuizContext>(options =>
{
    options.UseSqlServer(connectionString);
});

//builder.Services.AddSingleton<Func<QuizContext>>(sp => () =>
//{
//    var scope = sp.CreateScope();
//    return scope.ServiceProvider.GetRequiredService<QuizContext>();
//});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var secretKey = builder.Configuration.GetValue<string>("Jwt:Secret");
    var symmetricKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer"),
        ValidAudience = builder.Configuration.GetValue<string>("Jwt:Audience"),
        IssuerSigningKey = symmetricKey,
    };
});
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(p =>
    {
        var allowedOriginsStr = builder.Configuration.GetValue<string>("AllowedOrigins");
        var allowedOrigins = allowedOriginsStr.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        p.WithOrigins(allowedOrigins)
         .AllowAnyMethod()
         .AllowAnyHeader();
    });
});
builder.Services.AddTransient<AuthService>()
                .AddTransient<CategoryService>()
                .AddTransient<QuizService>()
                .AddTransient<AdminService>()
                .AddTransient<StudentQuizService>()
                .AddTransient<IImageUploadService, ImageUploadService>();
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

// Enable static files middleware to serve images from wwwroot
app.UseStaticFiles();

app.UseCors();

app.UseAuthentication()
    .UseAuthorization();

app.MapAuthEndpoints()
   .MapCategoryEndpoints()
   .MapCategoryImageEndpoints()
   .MapQuizEndpoints()
   .MapAdminEndpoints()
   .MapStudentQuizEndpoints();


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
