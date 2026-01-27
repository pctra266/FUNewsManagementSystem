using BusinessLogic.Services;
using DataAccess.Models;
using PhamCongTra_SE1885NET_A01_FE.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();

// Register HttpClient & ApiClient
builder.Services.AddHttpClient<IApiClient, DataAccess.Models.ApiClient>();

// Register Business Services
builder.Services.AddScoped<INewsArticleService, NewsArticleService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<INewsTagService, NewsTagService>();

// Session for authentication
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// Use custom authentication middleware
app.UseAuthenticationMiddleware();

app.UseAuthorization();
app.MapRazorPages();

app.Run();