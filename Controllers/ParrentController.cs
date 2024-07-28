using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class ParrentController : Controller
{
    public (bool, Claim[]) CheckToken()
    {

        var tokenHandler = new JwtSecurityTokenHandler();
        //تنظیماتی که با استاد توی پروگرم .سی اس زدیم 
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "Issuer",
            ValidAudience = "Audience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.Default.GetBytes("SymmetricSecurityKey secretKey Encoding.Default.GetBytes"))
        };
        if (Request.Cookies.TryGetValue("JWT_TOKEN", out string token)) // چک میکنه ببینه کوکیی اصن داریم یا نه
        {
            try
            {
                var moz = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken); // چک میکنه ببینه درسته یا نه
                HttpContext.Request.Headers.Add("Authorization", $"Bearer {token}"); // درست بود نمیدونم چیکارش میکنه !
                return (true, moz.Claims.ToArray());
            }
            catch (Exception ex)
            {
                Response.Cookies.Delete("JWT_TOKEN");
                return (false, null);
            }
            // اضافه کردن توکن به هدر Authorization

        }
        else
        {
            // توکن موجود نیست
            // انجام عملیات مورد نظر (مثلاً ارسال پیغام خطا)
            return (false, null);
        }
    }
}

