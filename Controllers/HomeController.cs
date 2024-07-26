using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test.Models;


public class HomeController : ParrentController
{

    public IActionResult Index()
    {
        if (Request.Cookies.TryGetValue("JWT_TOKEN", out string token))
        {
            // اضافه کردن توکن به هدر Authorization
            HttpContext.Request.Headers.Add("Authorization", $"Bearer {token}");
        }
        return View();
    }

    [HttpGet]
    [Authorize]
    public string Privacy()
    {
        return "انجام شد ؟";
    }

    public IActionResult test()
    {
        return View();
    }



 
}
