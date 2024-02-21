using Application.Interface;
using Domain;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Persistance;
using System.Text;
using System.Reflection;
using System.Xml;
using TaskManagerBackend;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        builder.Logging.ClearProviders();
     //   builder.Logging.AddLog4Net();

        XmlDocument log4netConfig = new XmlDocument();
        log4netConfig.Load(File.OpenRead("log4net.config"));
        var repo = log4net.LogManager.CreateRepository(
                    Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
        log4net.Config.XmlConfigurator.Configure(repo, log4netConfig["log4net"]);


        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();

        // Integrating Swagger with Jwt 
        // Add SwaggerGen with JWT support
        // Add SwaggerGen with JWT support
        builder.Services.AddSwaggerGen(c =>
        {
            // Define Swagger document information (API version, title, etc.)
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

            // Add JWT authentication to Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                // Describe how the API should be authenticated using JWT
                Description = "JWT Authorization header using the Bearer scheme.",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer" // Corrected to "bearer" for JWT
            });

            // Specify that JWT is required to access the API using Swagger
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "bearer",
                        Name = "Bearer",
                        In = ParameterLocation.Header   
                    },
                    new List<string>()
         

        
                }
            });

        });

        // Configure the connection string
        var connectionString = builder.Configuration.GetConnectionString("TaskManagerDatabaseDotnet8");

        // Add services to the container.
        builder.Services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(connectionString));

        // Add Identity
        builder.Services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders();

        //Configure JWT based Authentication 

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
            };
        });

        builder.Services.AddAuthorization();
        // In ConfigureServices method (this is what we are missing)
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        // Explicitly add UserManager and SignInManager
        builder.Services.AddScoped<UserManager<AppUser>>();
        builder.Services.AddScoped<SignInManager<AppUser>>();

        builder.Services.AddScoped<IResponseGeneratorService, ResponseGeneratorService>();
        builder.Services.AddScoped<ITaskManagerService, TaskManagerService>();
        builder.Services.AddScoped<ITaskCategoryService, TaskCategoryService>();
        builder.Services.AddScoped<ITaskService, TaskService>();
        builder.Services.AddScoped<ICommentService, CommentService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IEmailSenderService , EmailSenderService>();
        builder.Services.AddScoped<IAdminService, AdminService>();



     //  builder.Services.AddSingleton<ILog>(LogManager.GetLogger(typeof(Program)));

        // builder.Services.AddScoped<ReturnResponse>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .WithOrigins("https://localhost:44342") // Replace with your front-end application's URL
                        .WithExposedHeaders("WWW-Authenticate", "Pagination");
            });
        });



        var app = builder.Build();

        // Seed roles and admin user
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            await Seed.Initialize(roleManager, userManager);
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseCors("CorsPolicy");

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

       

        app.MapControllers();

        app.Run();
    }
}