using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json.Serialization;
using wdb_backend.Data;
using wdb_backend.Abstractions;
using wdb_backend.Services;
using wdb_backend.Usecases;
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
        policy.WithOrigins("http://localhost:3000","http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddScoped<IWorkerService, WorkerServiceImpl>();
builder.Services.AddScoped<IRequestService, RequestServiceImpl>();
builder.Services.AddScoped<IPermissionService, PermissionServiceImpl>();
builder.Services.AddScoped<IWorkerInfoService, WorkerInfoServiceImpl>();
builder.Services.AddScoped<IEmployerService, EmployerServiceImpl>();
builder.Services.AddScoped<ICreateDataAccessRequestUsecase,CreateDataAccessRequestUsecaseImpl>();
builder.Services.AddScoped<IFindWorkerInfosByEmailUsecase,FindWorkerInfosByEmailUsecaseImpl>();
builder.Services.AddScoped<IWorkerRepository, WorkerRepoImpl>();
builder.Services.AddScoped<IRequestRepository, RequestRepoImpl>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepoImpl>();
builder.Services.AddScoped<IWorkerInfoRepository, WorkerInfoRepoImpl>();
// application services
builder.Services.AddScoped<IWorkerDashboardService, WorkerDashboardServiceImpl>();
builder.Services.AddSingleton<IBlockchainService, BlockchainService>();
builder.Services.AddScoped<IWorkerInfoService, WorkerInfoServiceImpl>();


// DbContext
builder.Services.AddDbContextPool<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers(with JSON enum converter)
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Services
builder.Services.AddScoped<IWorkerDashboardService, WorkerDashboardServiceImpl>();

var app = builder.Build();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapOpenApi();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
