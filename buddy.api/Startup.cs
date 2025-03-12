using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
// You need to add this NuGet package: Microsoft.AspNetCore.Authentication.JwtBearer
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
// You need to add this NuGet package: Npgsql.EntityFrameworkCore.PostgreSQL
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BuddyUp.API.Data;
using BuddyUp.API.Data.Repositories;
using BuddyUp.API.Middleware;
using BuddyUp.API.Services.Implementations;
using BuddyUp.API.Services.Interfaces;
using BuddyUp.API.Utils;

namespace BuddyUp.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
            // Debug check for configuration
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            Console.WriteLine($"Connection string found: {!string.IsNullOrEmpty(connectionString)}");
            
            var allowedOrigins = Configuration["AllowedOrigins"];
            Console.WriteLine($"AllowedOrigins found: {!string.IsNullOrEmpty(allowedOrigins)}");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configure controllers with JSON options
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            // Configure CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins",
                    policy =>
                    {
                        var allowedOrigins = Configuration["AllowedOrigins"];
                        if (string.IsNullOrEmpty(allowedOrigins))
                        {
                            policy.AllowAnyOrigin()
                                  .AllowAnyMethod()
                                  .AllowAnyHeader();
                        }
                        else
                        {
                            policy.WithOrigins(allowedOrigins.Split(','))
                                  .AllowAnyMethod()
                                  .AllowAnyHeader()
                                  .AllowCredentials();
                        }
                    });
            });

            // Configure Auth0 Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = $"https://{Configuration["Auth0:Domain"]}/";
                    options.Audience = Configuration["Auth0:Audience"];
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "https://buddyup.com/roles"
                    };
                });

            // Configure Authorization policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy =>
                    policy.RequireClaim("https://buddyup.com/roles", "admin"));
            });

            // Add DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(
                    Configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions => npgsqlOptions.UseNetTopologySuite()
                );
            });

            // Register repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Register services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IMatchingService, MatchingService>();
            services.AddScoped<IMessagingService, MessagingService>();
            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<IVerificationService, VerificationService>();
            services.AddScoped<IAdminService, AdminService>();

            // Add AutoMapper
            services.AddAutoMapper(typeof(MappingProfiles));

            // Configure Swagger/OpenAPI
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BuddyUp API", Version = "v1" });
                
                // Add JWT Authorization to Swagger
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
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // First, exception handling
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }
            
            // Global exception handling middleware
            app.UseMiddleware<ExceptionMiddleware>();

            // IMPORTANT: UseRouting must come before Swagger middleware
            app.UseRouting();
            
            // IMPORTANT: Swagger middleware must be placed here, after UseRouting
            app.UseSwagger();
            app.UseSwaggerUI(c => 
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "BuddyUp API v1");
                c.RoutePrefix = "swagger";
            });

            app.UseHttpsRedirection();
            app.UseCors("AllowSpecificOrigins");

            app.UseAuthentication();
            app.UseAuthorization();

            // UseEndpoints must be last
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}