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
using DataAccess.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure OData
var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<NewsArticle>("NewsArticles");
modelBuilder.EntitySet<Category>("Categories");
modelBuilder.EntitySet<Tag>("Tags");
modelBuilder.EntitySet<SystemAccount>("SystemAccounts");
modelBuilder.EntitySet<NewsArticleImage>("NewsArticleImages");
// Explicitly configure SystemAccount key
var systemAccountEntity = modelBuilder.EntitySet<SystemAccount>("SystemAccounts");
systemAccountEntity.EntityType.HasKey(x => x.AccountId);

var auditLogEntity = modelBuilder.EntitySet<AuditLog>("AuditLogs");
auditLogEntity.EntityType.HasKey(x => x.LogId);

// Register Report DTOs as Complex Types
modelBuilder.ComplexType<PeriodDto>();
modelBuilder.ComplexType<CategoryStatisticDto>();
modelBuilder.ComplexType<CategoryReportDto>();
modelBuilder.ComplexType<AuthorStatisticDto>();
modelBuilder.ComplexType<AuthorReportDto>();
modelBuilder.ComplexType<StatusStatisticDto>();
modelBuilder.ComplexType<StatusReportDto>();

// Register Report Functions
// var reportsEntity = modelBuilder.EntitySet<NewsArticle>("Reports"); // REMOVED: Redundant and causing CRUD routes for Reports

var getActiveFunc = modelBuilder.EntityType<NewsArticle>().Collection.Function("GetActive");
getActiveFunc.ReturnsCollectionFromEntitySet<NewsArticle>("NewsArticles");

var dashboardFunc = modelBuilder.EntityType<NewsArticle>().Collection.Function("Dashboard");
dashboardFunc.Returns<string>(); 

var categoryReportFunc = modelBuilder.EntityType<NewsArticle>().Collection.Function("ArticlesByCategory");
categoryReportFunc.Parameter<string>("startDate");
categoryReportFunc.Parameter<string>("endDate");
categoryReportFunc.Parameter<bool?>("status").Optional();
categoryReportFunc.Returns<CategoryReportDto>();

var authorReportFunc = modelBuilder.EntityType<NewsArticle>().Collection.Function("ArticlesByAuthor");
authorReportFunc.Parameter<string>("startDate");
authorReportFunc.Parameter<string>("endDate");
authorReportFunc.Parameter<bool?>("status").Optional();
authorReportFunc.Returns<AuthorReportDto>();

var statusReportFunc = modelBuilder.EntityType<NewsArticle>().Collection.Function("ArticlesByStatus");
statusReportFunc.Parameter<string>("startDate");
statusReportFunc.Parameter<string>("endDate");
statusReportFunc.Returns<StatusReportDto>();

var trendingFunc = modelBuilder.EntityType<NewsArticle>().Collection.Function("Trending");
trendingFunc.Parameter<int?>("top");
trendingFunc.ReturnsCollectionFromEntitySet<NewsArticle>("NewsArticles");

var exportFunc = modelBuilder.EntityType<NewsArticle>().Collection.Function("Export");
exportFunc.Returns<byte[]>();

var recommendFunc = modelBuilder.EntityType<NewsArticle>().Function("Recommend");
recommendFunc.ReturnsCollectionFromEntitySet<NewsArticle>("NewsArticles");

var byCategoryFunc = modelBuilder.EntityType<NewsArticle>().Collection.Function("ByCategory");
byCategoryFunc.Parameter<short>("categoryId");
byCategoryFunc.ReturnsCollectionFromEntitySet<NewsArticle>("NewsArticles");

var duplicateAction = modelBuilder.EntityType<NewsArticle>().Action("Duplicate");
duplicateAction.ReturnsFromEntitySet<NewsArticle>("NewsArticles");

var edmModel = modelBuilder.GetEdmModel();

builder.Services.AddControllers()
    .AddOData(options =>
    {
        options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100);
        // Set maximum expansion depth to prevent depth-related reflection/token errors
        options.SetMaxTop(100); 
        options.AddRouteComponents("odata", edmModel);
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
builder.Services.AddScoped<INewsArticleImageService, NewsArticleImageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();


// JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key") ?? "YourSuperSecretDefaultKeyForDevelopmentOnly123!";
var issuer = jwtSection.GetValue<string>("Issuer");
var audience = jwtSection.GetValue<string>("Audience");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

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

// Function Request Logging Middleware
app.UseMiddleware<Presentation_API.Middleware.RequestLoggingMiddleware>();

app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hub/notifications");
//app.MapHealthChecks("/api/health")
//   .WithMetadata(new HttpMethodMetadata(new[] { "GET", "HEAD" }));

app.Run();
