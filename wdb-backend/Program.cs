using Microsoft.EntityFrameworkCore;
using wdb_backend.Data;
using wdb_backend.Abstractions;
using wdb_backend.Services;
using System.Reflection;
using System.Text.Json.Serialization;
using wdb_backend.Abstractions;
using wdb_backend.Services;
using wdb_backend.Usecases;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//Add Swagger service.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// CORS: allow the frontend dev server and production origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// registration and loing services
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


builder.Services.AddDbContextPool<AppDbContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

<<<<<<< HEAD
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
=======
builder.Services.AddSingleton<IBlockchainService, BlockchainService>();

>>>>>>> main
var app = builder.Build();
app.UseCors("FrontendPolicy");
app.MapControllers();
app.MapOpenApi();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowFrontend");
app.Run();
