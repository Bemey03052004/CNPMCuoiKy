using BanHang.Models;
using BanHang.Repository;
using BanHang.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =========================================================================
// === CẤU HÌNH IDENTITY - ĐÂY LÀ PHẦN QUAN TRỌNG CẦN SỬA LẠI ===
// =========================================================================
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Cấu hình mật khẩu (tùy chọn, nhưng nên có)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Cấu hình Lockout (tùy chọn)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình User - PHẦN SỬA LỖI CHÍNH
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; // <-- CHO PHÉP DÙNG EMAIL
    options.User.RequireUniqueEmail = true; // Bắt buộc Email phải là duy nhất

})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI(); // .AddDefaultUI() thường được dùng với Razor Pages

// =========================================================================

// Cấu hình chuyển hướng khi truy cập bị từ chối
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddRazorPages();

// Đăng ký các Repository và Service
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<ICartRepository, EFCartRepository>();
builder.Services.AddTransient<EmailSender>();


builder.Services.AddSignalR();
builder.Services.AddAuthentication(); // Nếu có login bằng Identity




// Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



// Thêm dịch vụ MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Gọi hàm tạo Role và Admin mặc định khi ứng dụng khởi động
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedRolesAndAdmin(services); // Sửa tên hàm cho rõ nghĩa
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Cấu hình HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Đảm bảo dòng này có mặt

app.UseAuthentication(); // Đặt trước UseAuthorization
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");


// Xóa Middleware chuyển hướng cứng vì nó có thể gây lỗi
// app.Use(async (context, next) =>
// { ... });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=BanHang}/{action=MainPage}/{id?}"); // Sửa action mặc định thành MainPage

app.Run();

async Task SeedRolesAndAdmin(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Tạo các vai trò nếu chúng chưa tồn tại
    string[] roleNames = { "Admin", "User" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Tạo tài khoản Admin mặc định nếu chưa có
    string adminEmail = "Admin@gmail.com";
    string adminPassword = "Admin@123";

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new IdentityUser
        {
            UserName = adminEmail, // Dùng Email làm UserName
            Email = adminEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}