using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<Context>();


builder.Services.AddAuthentication(options =>  
{  
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;  
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;  
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;  
})  
.AddCookie(options =>  
{  
    options.LoginPath = "/auth/login"; // Redirect to the login page  
    options.AccessDeniedPath = "/auth/NotAuthorized"; // Redirect to access denied page  
    options.LogoutPath = "/auth/login"; // Optional, add if you have a logout page  
});


builder.Services.AddSession();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
 app.UseSession();

// 
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=login}/{id?}");

app.Run();
