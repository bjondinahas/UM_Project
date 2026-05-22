using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using UM_Project.Data;
using UM_Project.Filters;
using UM_Project.Middleware;
using UM_Project.Models;
using UM_Project.Resources;
using UM_Project.Services;
using UM_Project.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Server=localhost;Database=UM_ProjectDB;User=root;Password=;"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 15;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("global", limiter =>
    {
        limiter.PermitLimit = 200;
        limiter.Window = TimeSpan.FromMinutes(1);
    });
});

builder.Services.AddHttpClient("ipgeo", c => c.Timeout = TimeSpan.FromSeconds(2));
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews(options => options.Filters.AddService<AdminAuditFilter>())
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
        options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(SharedResource)));
builder.Services.AddRazorPages();

builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAuthAuditService, AuthAuditService>();
builder.Services.AddScoped<IAdminAuditService, AdminAuditService>();
builder.Services.AddScoped<IIpGeoService, IpGeoService>();
builder.Services.AddScoped<AdminAuditFilter>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<AccountProvisioningSettings>(builder.Configuration.GetSection("AccountProvisioning"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAccountProvisioningService, AccountProvisioningService>();
builder.Services.AddScoped<IAdminPasswordService, AdminPasswordService>();
builder.Services.AddScoped<ISystemSettingsService, SystemSettingsService>();
builder.Services.AddScoped<ISchoolCalendarService, SchoolCalendarService>();
builder.Services.AddScoped<IAtRiskService, AtRiskService>();
builder.Services.AddScoped<ITranscriptPdfService, TranscriptPdfService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IReportExportService, ReportExportService>();
builder.Services.AddSingleton<IUiText, UiText>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseHttpsRedirection();
app.UseStaticFiles();

var supportedCultures = new[] { new CultureInfo("sq") };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("sq"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    RequestCultureProviders = []
};
app.UseRequestLocalization(localizationOptions);

// Ensure Albanian UTF-8 in HTML responses
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        if (context.Response.ContentType?.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) == true
            && !context.Response.ContentType.Contains("charset", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.ContentType = "text/html; charset=utf-8";
        }
        return Task.CompletedTask;
    });
    await next();
});
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<MustChangePasswordMiddleware>();
app.UseAuthorization();

var applyMigrations = builder.Configuration.GetValue("Database:ApplyMigrationsOnStartup", false);
var applySchemaBootstrap = builder.Configuration.GetValue("Database:ApplySchemaBootstrapOnStartup", false);

Console.WriteLine("Menaxhimi Shkollor — preparing database...");
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (applyMigrations)
    {
        Console.WriteLine("  Applying EF migrations...");
        await db.Database.MigrateAsync();
    }
    else
    {
        Console.WriteLine("  EF migrations skipped (Database:ApplyMigrationsOnStartup = false).");
    }

    if (applySchemaBootstrap)
    {
        await DatabaseSchemaBootstrap.ApplyAsync(db);
        Console.WriteLine("  Schema bootstrap applied.");
    }
    else
    {
        Console.WriteLine("  Schema bootstrap skipped (Database:ApplySchemaBootstrapOnStartup = false).");
    }

    var seedOnStartup = builder.Configuration.GetValue("Database:SeedOnStartup", true);
    var reseedDemoData = builder.Configuration.GetValue("Database:ReseedDemoData", false);

    if (seedOnStartup)
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await DemoDataSeeder.SeedAsync(db, userManager, roleManager, reseedDemoData);
    }
    else
    {
        Console.WriteLine("  Data seed skipped (Database:SeedOnStartup = false).");
    }

    var settingsService = scope.ServiceProvider.GetRequiredService<ISystemSettingsService>();
    await settingsService.ApplyGradeScaleOneToFiveAsync();
    await settingsService.MigrateLegacyGradesToCurrentScaleAsync();

    Console.WriteLine("Menaxhimi Shkollor — database startup step done.");
}

app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine();
    Console.WriteLine("============================================");
    Console.WriteLine("  Menaxhimi Shkollor is running. Open in browser:");
    Console.WriteLine("  https://localhost:7059");
    Console.WriteLine("  http://localhost:5199");
    Console.WriteLine("============================================");
    Console.WriteLine();
});

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
    .RequireRateLimiting("global");
app.MapRazorPages().RequireRateLimiting("global");

app.Run();
