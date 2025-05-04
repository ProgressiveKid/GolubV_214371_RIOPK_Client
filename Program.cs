using CorporateRiskManagementSystemBack.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<RiskDbContext>(options => options.UseNpgsql(connection)); // подключение к бд

builder.Services.AddRazorPages();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("https://localhost:7100") // URL фронтенда
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Для разрешения передачи куки
    });
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.SameSite = SameSiteMode.None; // SameSite политики, чтобы куки работали в разных приложениях
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.Path = "/";

    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.Run();
