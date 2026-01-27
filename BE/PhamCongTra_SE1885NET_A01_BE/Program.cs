using DataAccess.Data;
using DataAccess.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using Repositories;
using Services;
using System.Text;
using System.Text.Json.Serialization;

namespace FuNewsManagementAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 🔹 JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            // 🔹 DbContext
            builder.Services.AddDbContext<NewsContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn"))
            );

            // 🔹 Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = key,
                        ClockSkew = TimeSpan.Zero,
                        RoleClaimType = System.Security.Claims.ClaimTypes.Role
                    };
                });

            // 🔹 Authorization Policies - ĐƠN GIẢN VÀ RÕ RÀNG
            builder.Services.AddAuthorization(options =>
            {
                // Policy cho Admin - Full control
                options.AddPolicy("AdminOnly", policy => 
                    policy.RequireRole("ADMIN"));

                // Policy cho Staff - Quản lý categories và articles
                options.AddPolicy("StaffAccess", policy => 
                    policy.RequireRole("STAFF", "ADMIN"));

                // Policy cho Lecturer - Chỉ đọc
                options.AddPolicy("LecturerAccess", policy => 
                    policy.RequireRole("LECTURER", "STAFF", "ADMIN"));

                // Policy cho mọi user đã đăng nhập
                options.AddPolicy("Authenticated", policy => 
                    policy.RequireAuthenticatedUser());
            });

            // 🔹 Add Controllers + OData
            builder.Services.AddControllers()
                .AddOData(options =>
                    options.Select()
                           .Filter()
                           .Expand()
                           .OrderBy()
                           .Count()
                           .SetMaxTop(100)
                           .AddRouteComponents("odata", GetEdmModel())
                )
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            // 🔹 Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FU News Management API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter JWT with Bearer format: Bearer {token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
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
                        new string[] { }
                    }
                });
            });

            // 🔹 Dependency Injection
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
            builder.Services.AddScoped<INewsArticleService, NewsArticleService>();
            builder.Services.AddScoped<ISystemAccountRepository, SystemAccountRepository>();
            builder.Services.AddScoped<ISystemAccountService, SystemAccountService>();
            builder.Services.AddScoped<ITagRepository, TagRepository>();
            builder.Services.AddScoped<ITagService, TagService>();

            var app = builder.Build();

            // 🔹 Middleware pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<NewsArticle>("Articles");
            builder.EntitySet<Category>("Categories");
            builder.EntitySet<Tag>("Tags");
            builder.EntitySet<SystemAccount>("SystemAccounts");
            return builder.GetEdmModel();
        }
    }
}
