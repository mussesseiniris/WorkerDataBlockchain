using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json.Serialization;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.Models;
using wdb_backend.Notification;
using wdb_backend.Services;
using wdb_backend.Usecases;

var builder = WebApplication.CreateBuilder(args);

// ============================
// services
// ============================

// OpenAPI / Swagger
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// CORS for Next.js frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        var origins = (builder.Configuration["Cors:AllowedOrigins"]
                ?? "http://localhost:3000,http://localhost:3001")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Core services
builder.Services.AddScoped<IWorkerService, WorkerServiceImpl>();
builder.Services.AddScoped<IRequestService, RequestServiceImpl>();
builder.Services.AddScoped<IPermissionService, PermissionServiceImpl>();
builder.Services.AddScoped<IEmployerService, EmployerServiceImpl>();
builder.Services.AddScoped<IWorkerInfoService, WorkerInfoServiceImpl>();

builder.Services.AddScoped<IEmployerSentRequestService, EmployerSentRequestServiceImpl>();
builder.Services.AddScoped<IActiveAccessService, ActiveAccessServiceImpl>();
builder.Services.AddScoped<IEmployerActiveAccessService, EmployerActiveAccessServiceImpl>();
builder.Services.AddScoped<IEmployerRequestService, EmployerRequestServiceImpl>();
builder.Services.AddScoped<IWorkerAuditLogService, WorkerAuditLogServiceImpl>();
builder.Services.AddScoped<IWorkerRequestReviewService, WorkerRequestReviewServiceImpl>();

// Use cases
builder.Services.AddScoped<ICreateDataAccessRequestUsecase, CreateDataAccessRequestUsecaseImpl>();
builder.Services.AddScoped<IFindWorkerInfosByEmailUsecase, FindWorkerInfosByEmailUsecaseImpl>();
builder.Services.AddScoped<IAddFlexibleWorkerInfoUsecase, AddFlexibleWorkerInfoUsecaseImpl>();

// Repositories
builder.Services.AddScoped<IWorkerRepository, WorkerRepoImpl>();
builder.Services.AddScoped<IRequestRepository, RequestRepoImpl>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepoImpl>();
builder.Services.AddScoped<IWorkerInfoRepository, WorkerInfoRepoImpl>();
builder.Services.AddScoped<IEmployerRepository, EmployerRepoImpl>();

// Dashboard services
builder.Services.AddScoped<IWorkerDashboardService, WorkerDashboardServiceImpl>();
builder.Services.AddScoped<IEmployerDashboardService, EmployerDashboardServiceImpl>();

// Blockchain service
builder.Services.AddSingleton<IBlockchainService, BlockchainService>();

// Supabase storage (signed URL generation for private file objects)
builder.Services.AddHttpClient<ISupabaseStorageService, SupabaseStorageService>();

// On-chain audit logging (best-effort: swallows failures so the chain never blocks business actions)
builder.Services.AddScoped<IBlockchainAuditService, BlockchainAuditService>();

// DbContext
builder.Services.AddDbContextPool<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers with JSON enum converter
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Register SignalR
builder.Services.AddSignalR();

// Register MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddScoped<INotificationRepository, NotificationRepoImpl>();
builder.Services.AddScoped<INotificationService, NotificationServiceImpl>();

var app = builder.Build();

// ============================
// middleware - order matters
// ============================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");
app.MapOpenApi();

app.Run();
