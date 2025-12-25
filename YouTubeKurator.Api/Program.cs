using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using YouTubeKurator.Api.Data;
using YouTubeKurator.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS for local development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure HttpClient for YouTube API
builder.Services.AddHttpClient();

// Hent connection string fra appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registrer AppDbContext med SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Registrer YouTubeService
builder.Services.AddScoped<YouTubeService>();

// Registrer CacheService
builder.Services.AddScoped<CacheService>();

// Registrer auth services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Registrer filter service
builder.Services.AddScoped<IFilterService, FilterService>();

// Registrer sorting service
builder.Services.AddScoped<ISortingService, SortingService>();

// Registrer video status service
builder.Services.AddScoped<IVideoStatusService, VideoStatusService>();

// Registrer watch later service
builder.Services.AddScoped<IWatchLaterService, WatchLaterService>();

// Registrer related videos service
builder.Services.AddScoped<IRelatedVideosService, RelatedVideosService>();

// Registrer discovery service
builder.Services.AddScoped<IDiscoveryService, DiscoveryService>();

// Configure JWT authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrEmpty(jwtSecretKey) || jwtSecretKey.Length < 32)
{
    jwtSecretKey = "this-is-a-default-32-character-secret-key!!";
}

var key = Encoding.UTF8.GetBytes(jwtSecretKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "YouTubeKurator",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "YouTubeKurator",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Serve static files from wwwroot
app.UseStaticFiles();

// Enable CORS
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map root to serve index.html
app.MapFallbackToFile("index.html");

app.Run();
