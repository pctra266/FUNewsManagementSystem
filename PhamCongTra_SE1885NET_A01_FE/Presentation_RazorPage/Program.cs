using BusinessLogic.Services;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheKeyRegistry, CacheKeyRegistry>();

// Add Named HttpClients with Polly Policies
builder.Services.AddHttpClient("CoreClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7215");
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddPolicyHandler(GetRetryPolicy());

builder.Services.AddHttpClient("AnalyticsClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7215");
})
.AddPolicyHandler(GetRetryPolicy());

builder.Services.AddHttpClient("AIClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7215");
})
.AddPolicyHandler(GetRetryPolicy());

// Register ApiService
builder.Services.AddScoped<IApiService, ApiService>();

// Register Background Service
builder.Services.AddHostedService<Presentation_RazorPage.Services.CacheRefreshService>();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2));
}

// Register ExcelExportService
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();

// Configure session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();
app.MapGet("/", async context =>
{
    context.Response.Redirect("/News/Index");
});

// Logout endpoint
app.MapGet("/Logout", async context =>
{
    context.Session.Clear();
    context.Response.Redirect("/Login");
});

app.Run();
