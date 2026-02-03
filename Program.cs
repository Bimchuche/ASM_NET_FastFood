using ASM1_NET.Data;
using ASM1_NET.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseLazyLoadingProxies()
);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IFoodRepository, FoodRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddScoped<ASM1_NET.Services.IEmailService, ASM1_NET.Services.EmailService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ASM1_NET.Services.IActivityLogService, ASM1_NET.Services.ActivityLogService>();
builder.Services.AddScoped<ASM1_NET.Services.ILoyaltyService, ASM1_NET.Services.LoyaltyService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                // For AJAX/API requests, return 401 instead of redirect
                if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["GoogleAuth:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["GoogleAuth:ClientSecret"] ?? "";
        options.CallbackPath = "/signin-google";
        options.SaveTokens = true;
    });

builder.Services.AddAuthorization();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// SignalR Hubs
app.MapHub<ASM1_NET.Hubs.ChatHub>("/chathub");

app.Run();
