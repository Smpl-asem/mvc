using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using test.Models;

public class AuthController : Controller
{
   


    [HttpGet]
    public IActionResult login()
    {
        return View();
    }


   [HttpPost]
    public IActionResult login(string username,string password)
    {
        
        return View();
    }


    public IActionResult Register()
    {
        return View();
    }

    public IActionResult Forget()
    {
        return View();
    }

     public IActionResult Verify()
    {
        return View();
    }



 
}
