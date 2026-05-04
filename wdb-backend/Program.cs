using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json.Serialization;
using wdb_backend.Data;
using wdb_backend.Abstractions;
using wdb_backend.Services;

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


// DbContext
builder.Services.AddDbContextPool<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers(with JSON enum converter)
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

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
