using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test.Models;


[Authorize]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult NotAuthorized()
    {
        return View();
    }

    public IActionResult test()
    {
        return View();
    }



 
}
