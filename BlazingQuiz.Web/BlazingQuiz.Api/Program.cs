using BlazingQuiz.Api.Data;
using BlazingQuiz.Api.Data.Entities;
using BlazingQuiz.Api.Endpoints;
using BlazingQuiz.Api.Hubs;
using BlazingQuiz.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization; // Add this using directive

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlazingQuiz API", Version = "v1" });
    
    // Configure JWT authentication for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

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

var corsPolicyName = "QuizCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy("QuizCorsPolicy", policy =>
    {
        policy.WithOrigins("https://b861mvjb-7194.asse.devtunnels.ms")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddTransient<AuthService>()
                .AddTransient<CategoryService>()
                .AddTransient<QuizService>()
                .AddTransient<AdminService>()
                .AddTransient<StudentQuizService>()
                .AddTransient<RoomQuizService>()
                .AddTransient<BookmarkService>()
                .AddTransient<IImageUploadService, ImageUploadService>()
                .AddTransient<IAudioUploadService, AudioUploadService>()
                .AddTransient<PasswordResetService>()
                .AddTransient<OtpService>()
                .AddTransient<GmailOtpService>();

// Add SignalR
builder.Services.AddSignalR();
builder.Services.AddTransient<RoomService>();
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

app.UseCors(corsPolicyName);

app.UseAuthentication()
    .UseAuthorization();

app.MapAuthEndpoints();
app.MapCategoryEndpoints();
app.MapCategoryImageEndpoints();
app.MapQuizEndpoints();
app.MapQuizAudioEndpoints();
app.MapQuizImageEndpoints();
app.MapQuestionAudioEndpoints();
app.MapQuestionImageEndpoints();
app.MapAdminEndpoints();
app.MapStudentQuizEndpoints();
app.MapRoomQuizEndpoints();
app.MapBookmarkEndpoints();
app.MapRoomEndpoints();
app.MapGeneralAudioEndpoints();
app.MapUserAvatarEndpoints();

// Map SignalR hubs
app.MapHub<QuizHub>("/quizhub");

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
