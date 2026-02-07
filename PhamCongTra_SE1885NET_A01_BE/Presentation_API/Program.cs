using DataAccess.Models;
using DataAccess.Repositories;
using BussinessLogic.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using DataAccess.Data;
using Presentation_API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure OData
var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<NewsArticle>("NewsArticles");
modelBuilder.EntitySet<Category>("Categories");
modelBuilder.EntitySet<Tag>("Tags");

// Explicitly configure SystemAccount key
var systemAccountEntity = modelBuilder.EntitySet<SystemAccount>("SystemAccounts");
systemAccountEntity.EntityType.HasKey(x => x.AccountId);

builder.Services.AddControllers()
    .AddOData(options =>
    {
        options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100);
        options.AddRouteComponents("odata", modelBuilder.GetEdmModel());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.MaxDepth = 64;
    });

builder.Services.AddSignalR();

// Add AutoMapper - Temporarily disabled
// builder.Services.AddAutoMapper(typeof(MappingProfile));

// DbContext
builder.Services.AddDbContext<NewsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

// Register Repository and UnitOfWork
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register All Services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<INewsArticleService, NewsArticleService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();


// JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = jwtSection.GetValue<string>("Key");
var issuer = jwtSection.GetValue<string>("Issuer");
var audience = jwtSection.GetValue<string>("Audience");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateLifetime = true
    };
});

// Authorization policies - Use ClaimTypes.Role for consistency
builder.Services.AddAuthorization(options =>
{
    // StaffOnly: allow Staff (1) or Admin
    options.AddPolicy("StaffOnly", policy => policy.RequireClaim(ClaimTypes.Role, "1", "Admin"));
    // LecturerOrAbove: allow Lecturer (2), Staff (1) or Admin
    options.AddPolicy("LecturerOrAbove", policy => policy.RequireClaim(ClaimTypes.Role, "2", "1", "Admin"));
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim(ClaimTypes.Role, "Admin"));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCors", builder =>
    {
        builder.SetIsOriginAllowed(origin => true) // Allow any origin
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

    app.UseHttpsRedirection();

    app.UseStaticFiles(); // Enable static file serving (for uploaded images)

    app.UseCors("MyCors");

// Critical: Authentication MUST come before custom middleware
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hub/notifications");
app.MapHealthChecks("/api/health")
   .WithMetadata(new HttpMethodMetadata(new[] { "GET", "HEAD" }));

app.Run();
