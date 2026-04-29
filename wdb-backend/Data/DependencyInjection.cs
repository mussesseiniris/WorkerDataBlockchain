using wdb_backend.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using wdb_backend.Abstractions;
using wdb_backend.Models;

namespace wdb_backend.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // already inject db instance in program.cs
        // inject authority related
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };
            });

        services.AddAuthorization();
        services.AddHttpContextAccessor();

        // register jwt related services for DI
        services.AddScoped<IJwtTokenService, JwtTokenServiceImpl>();
        services.AddScoped<IPasswordHasher, PasswordHasherServiceImpl>();
        services.AddScoped<AuthService<Worker>>();
        services.AddScoped<AuthService<Employer>>();

        // register user repo for DI
        services.AddScoped<IWorkerRepository, WorkerRepoImpl>();
        services.AddScoped<IUserRepository<Worker>>(sp => sp.GetRequiredService<IWorkerRepository>());
        services.AddScoped<IEmployerRepository, EmployerRepoImpl>();
        services.AddScoped<IUserRepository<Employer>>(sp => sp.GetRequiredService<IEmployerRepository>());

        return services;
    }
}
