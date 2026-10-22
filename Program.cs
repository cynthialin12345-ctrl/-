var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<transcript.Models.DataBase>();
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

var app = builder.Build();

// Global exception handlers to capture unhandled exceptions for diagnostics
AppDomain.CurrentDomain.UnhandledException += (s, e) =>
{
    try
    {
        var ex = e.ExceptionObject as Exception;
        System.Diagnostics.Debug.WriteLine($"UnhandledException: {ex}");
    }
    catch { }
};

TaskScheduler.UnobservedTaskException += (s, e) =>
{
    try
    {
        System.Diagnostics.Debug.WriteLine($"UnobservedTaskException: {e.Exception}");
    }
    catch { }
};

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UsePathBase("/b/");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
