using SS.Mancala.PL.Data;
using Microsoft.EntityFrameworkCore;
using SS.Mancala.BL;

var builder = WebApplication.CreateBuilder(args);

// Use the connection string from appsettings.json
builder.Services.AddDbContext<MancalaEntities>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MancalaConnection")));

// Add other services and configurations
builder.Services.AddControllersWithViews();

var app = builder.Build();

  builder.Services.AddLogging(); 


builder.Services.AddScoped<MoveManager>();
builder.Services.AddScoped<GameManager>(); 

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();