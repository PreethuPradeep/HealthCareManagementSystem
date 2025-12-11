
using HealthCare.Database;
using HealthCare.Services;
using HealthCareManagementSystem.Helper;
using HealthCareManagementSystem.Models;
using HealthCareManagementSystem.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

namespace HealthCareManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "HealthCare API",
                    Version = "v1"
                });

                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "Paste your JWT token here",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
            });

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.WriteIndented = true;
            });

            builder.Services.AddCors(options => //adding cors policy
            {
                options.AddPolicy("MyPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            //admin
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            // Database Context
            builder.Services.AddDbContext<HealthCareDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("default");
                options.UseSqlServer(connectionString);
            });

            // ASP.NET Core Identity Configuration
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<HealthCareDbContext>()
                .AddDefaultTokenProviders();

            // Force JWT bearer as the default scheme (Identity registers cookie schemes by default)
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // allow local dev over http/https mix
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,   // relax for local to accept our issued tokens
                    ValidateAudience = false, // relax for local to accept our issued tokens
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name
                };
                options.SaveToken = true;
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"JWT auth failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("JWT token validated successfully.");
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                // Do not redirect for APIs—return 401/403 instead
                options.Events.OnRedirectToLogin = ctx =>
                {
                    ctx.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = ctx =>
                {
                    ctx.Response.StatusCode = 403;
                    return Task.CompletedTask;
                };
            });

            // Configure Identity options
            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // User settings
                options.User.RequireUniqueEmail = true;
            });

            // Repository services-admin
            builder.Services.AddScoped<IUserRepository, UserSqlServerRepositoryImpl>();
            builder.Services.AddScoped<IRoleRepository, RoleSqlServerRepositoryImpl>();
            builder.Services.AddScoped<ISpecializationRepository, SpecializationSqlServerRepositoryImpl>();
            //receptionist
            builder.Services.AddScoped<IPatientRepository, PatientRepository>();
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            builder.Services.AddScoped<IBillingRepository, BillingRepository>();
            //pharmacist
            builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
            builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
            builder.Services.AddScoped<IBillingPharmacyRepository, BillingPharmacyRepository>();
            //doctor
            builder.Services.AddScoped<IConsultationRepository, ConsultationRepository>();

            builder.Services.AddScoped<JwtTokenHelper>();
            builder.Services.AddScoped<PdfService>();
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("MyPolicy");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            
            // Map Identity API endpoints (login, register, etc.)
            //app.MapIdentityApi<ApplicationUser>();
            
            // Map custom controllers
            app.MapControllers();
            SeedRolesAndAdmin(app).GetAwaiter().GetResult();
            SeedExistingUsers(app).GetAwaiter().GetResult();
            app.Run();
        }
        private static async Task SeedRolesAndAdmin(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HealthCareDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var requiredRoles = new[]
            {
                "Admin",
                "Doctor",
                "Receptionist",
                "Pharmacist",
                "Lab"
            };

            // Ensure role records exist in custom Roles table
            foreach (var roleName in requiredRoles)
            {
                if (!context.Roles.Any(r => r.RoleName == roleName))
                {
                    context.Roles.Add(new Role
                    {
                        RoleName = roleName,
                        IsActive = true
                    });
                }
            }

            await context.SaveChangesAsync();

            // Seed a default admin if none exists
            var adminEmail = "admin@hospital.com";
            var adminUser = await userManager.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == adminEmail);

            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
            if (adminRole == null)
            {
                return;
            }

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    Gender = "Other",
                    DateOfJoin = DateTime.UtcNow.Date,
                    DateOfBirth = new DateTime(1990, 1, 1),
                    MobileNumber = "9999999999",
                    Address = "System",
                    RoleId = adminRole.RoleId,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(newAdmin, "Admin@123");
                if (!createResult.Succeeded)
                {
                    throw new Exception($"Failed to seed admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
            else if (adminUser.RoleId != adminRole.RoleId)
            {
                adminUser.RoleId = adminRole.RoleId;
                context.Users.Update(adminUser);
                await context.SaveChangesAsync();
            }
        }
        private static async Task SeedExistingUsers(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HealthCareDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();

            // Known intended passwords provided by user for existing seeded accounts.
            var resetPasswords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["sreejith.menon@hospital.com"] = "Doctor@123",
                ["anjali.jose@hospital.com"] = "Doctor@234",
                ["meera.pillai@hospital.com"] = "Recep@123",
                ["jithin.mathew@hospital.com"] = "Pharma@123",
                ["sneha.tech@hospital.com"] = "Lab@123"
            };

            var usersNeedingFix = await context.Users
                .Where(u =>
                    string.IsNullOrWhiteSpace(u.NormalizedEmail) ||
                    string.IsNullOrWhiteSpace(u.NormalizedUserName) ||
                    string.IsNullOrWhiteSpace(u.PasswordHash))
                .ToListAsync();

            var resetEmailsUpper = resetPasswords.Keys
                .Select(e => e.ToUpperInvariant())
                .ToList();

            var usersToReset = await context.Users
                .Where(u => resetEmailsUpper.Contains(u.Email.ToUpper()))
                .ToListAsync();

            var allUsers = usersNeedingFix
                .Concat(usersToReset)
                .DistinctBy(u => u.Id)
                .ToList();

            if (!allUsers.Any())
                return;

            foreach (var user in allUsers)
            {
                // Identity lookups rely on normalized fields; populate if missing.
                user.NormalizedEmail ??= user.Email?.ToUpperInvariant();
                user.NormalizedUserName ??= (user.UserName ?? user.Email)?.ToUpperInvariant();

                // Ensure required Identity stamps exist
                user.SecurityStamp ??= Guid.NewGuid().ToString();
                user.ConcurrencyStamp ??= Guid.NewGuid().ToString();

                // If a specific password is provided, reset to that value.
                if (resetPasswords.TryGetValue(user.Email, out var providedPassword))
                {
                    user.PasswordHash = passwordHasher.HashPassword(user, providedPassword);
                }
                else if (string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    // Default bootstrap password so the account is usable; should be reset after first login.
                    user.PasswordHash = passwordHasher.HashPassword(user, "Admin@123");
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
