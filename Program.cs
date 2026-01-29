using ASM1_NET.Data;
using ASM1_NET.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ================= DB CONTEXT với Lazy Loading =================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseLazyLoadingProxies()  // ✅ Lazy Loading enabled
);

// ================= MVC =================
builder.Services.AddControllersWithViews();

// ================= REPOSITORIES - Dependency Injection =================
// Generic Repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Specific Repositories
builder.Services.AddScoped<IFoodRepository, FoodRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// ================= EMAIL SERVICE =================
builder.Services.AddScoped<ASM1_NET.Services.IEmailService, ASM1_NET.Services.EmailService>();

// ================= ACTIVITY LOG SERVICE =================
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ASM1_NET.Services.IActivityLogService, ASM1_NET.Services.ActivityLogService>();

// ================= AUTH =================
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

// Session với cấu hình đầy đủ
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ================= PIPELINE =================
app.UseStaticFiles();

app.UseRouting();

app.UseSession();            // nếu dùng
app.UseAuthentication();     // BẮT BUỘC
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
