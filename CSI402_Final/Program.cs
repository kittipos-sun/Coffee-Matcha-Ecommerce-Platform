using Microsoft.EntityFrameworkCore;
using CSI402_Final.Models.DB;

var builder = WebApplication.CreateBuilder(args);

// ================= SERVICES =================
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<Csi402FinalContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

builder.Services.AddSession(); // ✅ ถูก

// ================= BUILD =================
var app = builder.Build();

// This project does not ship EF migrations. EnsureCreated keeps a fresh local
// database usable after the first start; use reviewed migrations for production.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Csi402FinalContext>();
    db.Database.EnsureCreated();
}

// ================= MIDDLEWARE =================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseRouting();

app.UseSession(); // ✅ ต้องอยู่ตรงนี้

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
