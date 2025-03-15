using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BuddyUp.API.Data;
using BuddyUp.API.Data.Repositories;
using BuddyUp.API.Middleware;
using BuddyUp.API.Services.Implementations;
using BuddyUp.API.Services.Interfaces;
using BuddyUp.API.Utils;
using BuddyUp.API.Configuration;

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
            services.AddControllers(options => { JwtSecurityTokenHandler.DefaultInboundClaimFilter.Clear(); })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });


            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins",
                    policy =>
                    {
                        var allowedOrigins = Configuration["AllowedOrigins"];
                        if (string.IsNullOrEmpty(allowedOrigins))
                        {
                            // In development, if no origins are specified, allow any origin
                            policy.AllowAnyOrigin()
                                  .AllowAnyMethod()
                                  .AllowAnyHeader();

                            Console.WriteLine("CORS configured to allow any origin (development mode)");
                        }
                        else
                        {
                            // Ensure http://localhost:5100 is included in the allowed origins
                            var origins = allowedOrigins.Split(',').ToList();
                            if (!origins.Contains("http://localhost:5100"))
                            {
                                origins.Add("http://localhost:5100");
                            }

                            policy.WithOrigins(origins.ToArray())
                                  .AllowAnyMethod()
                                  .AllowAnyHeader()
                                  .AllowCredentials();

                            Console.WriteLine($"CORS configured with specific origins: {string.Join(", ", origins)}");
                        }
                    });
            });

            // Configure Auth0 authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = $"https://{Configuration["Auth0:Domain"]}/";
                    options.Audience = Configuration["Auth0:Audience"];

                    // Clear the default mappings first to preserve original claim types
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = $"https://{Configuration["Auth0:Domain"]}/",
                        ValidateAudience = true,
                        ValidAudience = Configuration["Auth0:Audience"],
                        ValidateLifetime = true,
                        NameClaimType = "name",
                        RoleClaimType = "https://buddyup.com/roles"
                    };

                    // Add event handlers for debugging
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            // Map the 'sub' claim to ClaimTypes.NameIdentifier manually
                            var subClaim = context.Principal.FindFirst("sub");
                            if (subClaim != null)
                            {
                                var identity = context.Principal.Identity as ClaimsIdentity;
                                if (identity != null)
                                {
                                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, subClaim.Value));
                                }
                            }

                            Console.WriteLine("Token validated successfully!");
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                            Console.WriteLine($"Exception details: {context.Exception}");
                            return Task.CompletedTask;
                        },
                        OnMessageReceived = context =>
                        {
                            Console.WriteLine("JWT token received");
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            Console.WriteLine($"Authentication challenge issued: {context.Error}, {context.ErrorDescription}");
                            return Task.CompletedTask;
                        }
                    };
                });

            // Configure Authorization policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy =>
                    policy.RequireClaim("https://buddyup.com/roles", "admin"));
            });

            // Add PostgreSQL DbContext with proper configuration
            services.AddPostgreSqlDbContext(Configuration);

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

            app.UseCors("AllowSpecificOrigins");

            // Global exception handling middleware
            app.UseMiddleware<ExceptionMiddleware>();

            // Ensure database is properly created and can be connected to
            app.EnsureDatabaseCreated();

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