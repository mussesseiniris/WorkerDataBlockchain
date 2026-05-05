using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json.Serialization;
using wdb_backend.Data;
using wdb_backend.Abstractions;
using wdb_backend.Services;
using System.Reflection;
using System.Text.Json.Serialization;
using wdb_backend.Models;


var builder = WebApplication.CreateBuilder(args);

// ============================
// service 
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

// CORS make sure to add before authentication and authorization middlewares, otherwise the preflight request will be blocked before reaching CORS middleware.
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// infrastructure 
builder.Services.AddInfrastructure(builder.Configuration);

// application services
builder.Services.AddScoped<IWorkerDashboardService, WorkerDashboardServiceImpl>();
builder.Services.AddSingleton<IBlockchainService, BlockchainService>();
builder.Services.AddScoped<IWorkerInfoService, WorkerInfoServiceImpl>();

// CORS for Next.js frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// DbContext
builder.Services.AddDbContextPool<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers(with JSON enum converter)
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddScoped<IPermissionService, PermissionServiceImpl>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepoImpl>();
builder.Services.AddScoped<IRequestService, RequestServiceImpl>();
builder.Services.AddScoped<IRequestRepository, RequestRepoImpl>();
builder.Services.AddScoped<IWorkerInfoService, WorkerInfoServiceImpl>();
builder.Services.AddScoped<IWorkerInfoRepository, WorkerInfoRepoImpl>();
builder.Services.AddScoped<IEmployerService, EmployerServicerImpl>();
builder.Services.AddScoped<IEmployerRepository, EmployerRepoImpl>();
builder.Services.AddSingleton<IBlockchainService, BlockchainService>();
// Services
builder.Services.AddScoped<IWorkerDashboardService, WorkerDashboardServiceImpl>();
builder.Services.AddScoped<IPermissionService, PermissionServiceImpl>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepoImpl>();
builder.Services.AddScoped<IRequestService, RequestServiceImpl>();
builder.Services.AddScoped<IRequestRepository, RequestRepoImpl>();
builder.Services.AddScoped<IWorkerInfoService, WorkerInfoServiceImpl>();
builder.Services.AddScoped<IWorkerInfoRepository, WorkerInfoRepoImpl>();
builder.Services.AddScoped<IEmployerService, EmployerServicerImpl>();
builder.Services.AddScoped<IEmployerRepository, EmployerRepoImpl>();

var app = builder.Build();
//app.UseCors("FrontendPolicy");

app.UseCors("AllowFrontend");

// ============================
// middleware the order matters!
// ============================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");   // CORS use once.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapOpenApi();

app.Run();
