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

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IFoodRepository, FoodRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddScoped<ASM1_NET.Services.IEmailService, ASM1_NET.Services.EmailService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ASM1_NET.Services.IActivityLogService, ASM1_NET.Services.ActivityLogService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
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

app.Run();
