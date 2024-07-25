using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using test.Models;


public class UserController : Controller
{
   


    [HttpGet]
    public IActionResult ProfileUser()
    {
        
        return View();
    }

    [HttpGet]
    public IActionResult UserSetting()
    {
        
        return View();
    }
 
}
