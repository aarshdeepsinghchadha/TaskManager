using Application.Interface;
using Domain;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistance;

namespace TaskManagerBackend.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddTaskManagerServices(this IServiceCollection services, string connectionString)
        {
            // Configure the connection string
            services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(connectionString));

            // Add Identity
            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<DataContext>()
                .AddDefaultTokenProviders();

            // Add your custom services here
            services.AddScoped<IRegisterLoginService, RegisterLoginService>();
            services.AddScoped<IResponseGeneratorService, ResponseGeneratorService>();

            // Add other services as needed

            return services;
        }
    }
}
