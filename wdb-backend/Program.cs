using Microsoft.EntityFrameworkCore;
using wdb_backend.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDbContextPool<AppDbContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("SupabaseConnection")));


var app = builder.Build();
app.MapControllers();
app.MapOpenApi();


app.Run();
