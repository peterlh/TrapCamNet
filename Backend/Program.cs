using System.Text;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using TrapCam.Backend.Data;
using TrapCam.Backend.Services;
using TrapCam.Backend.Settings;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog if enabled
var serilogEnabled = builder.Configuration["Serilog:Enabled"];
var serilogServerUrl = builder.Configuration["Serilog:ServerUrl"];

if (!string.IsNullOrEmpty(serilogEnabled) && bool.TryParse(serilogEnabled, out bool enabled) && enabled && !string.IsNullOrEmpty(serilogServerUrl))
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "TrapCam.Backend")
        .WriteTo.Console()
        .WriteTo.Seq(serilogServerUrl)
        .CreateLogger();

    builder.Host.UseSerilog();
    
    Log.Information("Serilog configured to send logs to {SerilogServerUrl}", serilogServerUrl);
}
else
{
    // Default logging configuration
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console());
}

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization to always handle DateTime values as UTC
        options.JsonSerializerOptions.Converters.Add(new TrapCam.Backend.Utilities.UtcDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new TrapCam.Backend.Utilities.UtcNullableDateTimeConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// Add HttpClient services
builder.Services.AddHttpClient();

// Configure S3 settings
builder.Services.Configure<S3Settings>(options => 
{
    options.ServiceUrl = builder.Configuration["S3:ServiceUrl"] ?? "http://minio:9000";
    options.AccessKey = builder.Configuration["S3:AccessKey"] ?? "minioadmin";
    options.SecretKey = builder.Configuration["S3:SecretKey"] ?? "minioadmin";
    options.ForcePathStyle = bool.TryParse(builder.Configuration["S3:ForcePathStyle"], out bool forcePathStyle) ? forcePathStyle : true;
    options.EmailArchiveBucket = builder.Configuration["S3:EmailArchiveBucket"] ?? "emailarchive";
    options.ImageBucket = builder.Configuration["S3:ImageBucket"] ?? "image";
    options.EmailArchiveTTLDays = int.TryParse(builder.Configuration["S3:EmailArchiveTTLDays"], out int emailTtl) ? emailTtl : 3;
    options.ImageTTLDays = int.TryParse(builder.Configuration["S3:ImageTTLDays"], out int imageTtl) ? imageTtl : 3;
    options.UseHttp = bool.TryParse(builder.Configuration["S3:UseHttp"], out bool useHttp) ? useHttp : false;
});

// Configure AWS S3 client
var s3Config = new AmazonS3Config
{
    ServiceURL = builder.Configuration["S3:ServiceUrl"] ?? "http://minio:9000",
    ForcePathStyle = bool.TryParse(builder.Configuration["S3:ForcePathStyle"], out bool forcePathStyle) ? forcePathStyle : true,
    UseHttp = bool.TryParse(builder.Configuration["S3:UseHttp"], out bool useHttp) ? useHttp : false
};

builder.Services.AddAWSService<IAmazonS3>(new AWSOptions
{
    Credentials = new Amazon.Runtime.BasicAWSCredentials(
        builder.Configuration["S3:AccessKey"] ?? "minioadmin",
        builder.Configuration["S3:SecretKey"] ?? "minioadmin")
});

builder.Services.AddSingleton<IAmazonS3>(new AmazonS3Client(
    builder.Configuration["S3:AccessKey"] ?? "minioadmin",
    builder.Configuration["S3:SecretKey"] ?? "minioadmin",
    s3Config
));

// Register S3 service
builder.Services.AddScoped<IS3Service, S3Service>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TrapCam API", Version = "v1" });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
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
            Array.Empty<string>()
        }
    });
});

// Add JWT Authentication for Firebase
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Firebase project ID from environment variable
        var firebaseProjectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");
        
        options.Authority = $"https://securetoken.google.com/{firebaseProjectId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{firebaseProjectId}",
            ValidateAudience = true,
            ValidAudience = firebaseProjectId,
            ValidateLifetime = true
        };
    });

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Entity Framework with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register services
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IEmailArchiveService, EmailArchiveService>();
builder.Services.AddScoped<ICameraService, CameraService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IBatteryExtractionService, BatteryExtractionService>();
builder.Services.AddScoped<IAnimalService, AnimalService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Register HttpContextAccessor and UserContext service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TrapCam API v1"));

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health endpoint is now handled by HealthController at /api/health

// Apply migrations directly before initializing database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Applying migrations directly...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying migrations");
        throw;
    }
}

// Initialize database with seed data
await TrapCam.Backend.Data.DbInitializer.Initialize(app.Services);

// Ensure S3 buckets exist
using (var scope = app.Services.CreateScope())
{
    var s3Service = scope.ServiceProvider.GetRequiredService<IS3Service>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Ensuring S3 buckets exist...");
        await s3Service.EnsureBucketsExistAsync();
        logger.LogInformation("S3 buckets verified successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while ensuring S3 buckets exist");
        // Don't throw here, as this is not critical for application startup
    }
}

try
{
    app.Run();
    return 0; // Return success code when the application exits normally
}
finally
{
    // Ensure to flush and close the Serilog logger when the application shuts down
    Log.CloseAndFlush();
}
