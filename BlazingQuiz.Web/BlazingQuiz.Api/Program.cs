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

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    var secretKey = builder.Configuration.GetValue<string>("Jwt:Secret");
//    var symmetricKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer"),
//        ValidAudience = builder.Configuration.GetValue<string>("Jwt:Audience"),
//        IssuerSigningKey = symmetricKey,
//    };
//});

// Only add Google authentication if the required configuration values are present
//var googleClientId = builder.Configuration["GoogleOAuth:ClientId"];
//var googleClientSecret = builder.Configuration["GoogleOAuth:ClientSecret"];
//if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
//{
//    builder.Services
//        .AddAuthentication()
//        .AddGoogle("Google", options =>
//        {
//            options.ClientId = googleClientId;
//            options.ClientSecret = googleClientSecret;
//            options.SaveTokens = true;
//            options.CallbackPath = "/authorize/login-callback"; // Use the same callback path as in your GoogleOAuth config
//            options.AccessDeniedPath = "/access-denied";

//            options.Events.OnCreatingTicket = async context =>
//            {
//                // Extract user information from the Google token
//                var email = context.Principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value ??
//                           context.Principal.FindFirst("email")?.Value;
//                var name = context.Principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value ??
//                          context.Principal.FindFirst("name")?.Value;

//                // Add custom claims
//                if (!string.IsNullOrEmpty(email))
//                {
//                    ((List<System.Security.Claims.Claim>)context.Principal.Claims).Add(new System.Security.Claims.Claim("email", email));
//                }
//                if (!string.IsNullOrEmpty(name))
//                {
//                    ((List<System.Security.Claims.Claim>)context.Principal.Claims).Add(new System.Security.Claims.Claim("name", name));
//                }

//                await Task.CompletedTask;
//            };


//            // Handle authentication failure
//            options.Events.OnRemoteFailure = context =>
//            {
//                context.HandleResponse();
//                // If there's a remote failure, redirect to frontend with error
//                var frontendUrl = context.Request.HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetValue<string>("Jwt:Audience") ?? "https://localhost:7194";
//                var redirectUrl = $"{frontendUrl}/auth/login?error=google_auth_failed";
//                context.Response.Redirect(redirectUrl);
//                return Task.CompletedTask;
//            };
//        });
//}

// ===== Authentication & JWT =====
builder.Services
    .AddAuthentication(options =>
    {
        // Mặc định dùng JWT cho API
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
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

// ===== Google OAuth (dùng chung AuthenticationBuilder ở trên, KHÔNG gọi AddAuthentication() lần nữa) =====
var googleClientId = builder.Configuration["GoogleOAuth:ClientId"];
var googleClientSecret = builder.Configuration["GoogleOAuth:ClientSecret"];

if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services
        .AddAuthentication()              // dùng lại builder hiện tại, không reset options
        .AddGoogle("Google", options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
            options.SaveTokens = true;

            // Phải trùng với endpoint callback và redirect URI trên Google Console
            options.CallbackPath = "/authorize/login-callback";
            options.AccessDeniedPath = "/access-denied";

            // Thêm scope nếu cần
            options.Scope.Add("profile");
            options.Scope.Add("email");

            // Tùy chọn: xử lý ticket – đoạn add claim email/name thực ra không cần thiết lắm
            options.Events.OnCreatingTicket = context =>
            {
                // Nếu chỉ cần default claim của Google thì có thể bỏ trống
                return Task.CompletedTask;
            };

            // Xử lý lỗi remote (ví dụ user bấm Cancel hoặc Google trả lỗi)
            options.Events.OnRemoteFailure = context =>
            {
                var logger = context.Request.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("GoogleRemoteFailure");

                logger.LogError(context.Failure, "Google remote failure: {Message}", context.Failure?.Message);

                context.HandleResponse();
                var frontendUrl = context.Request.HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()
                    .GetValue<string>("Jwt:Audience") ?? "https://localhost:7194";
                var redirectUrl = $"{frontendUrl}/auth/login?error=google_auth_failed";
                context.Response.Redirect(redirectUrl);
                return Task.CompletedTask;
            };
        });
}

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
builder.Services.AddTransient<GoogleAuthService>();
builder.Services.AddHttpClient();
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
