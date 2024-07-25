using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using test.Models;


public class HomeController : Controller
{
   

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult test()
    {
        return View();
    }



 
}
