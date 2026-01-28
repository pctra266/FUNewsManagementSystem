using BusinessLogic.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add HttpClient
builder.Services.AddHttpClient<IApiService, ApiService>();

// Register ApiService
builder.Services.AddScoped<IApiService, ApiService>();

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
