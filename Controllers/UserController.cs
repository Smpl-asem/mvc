using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using test.Models;


public class UserController : ParrentController
{
   


    [HttpGet]
    public IActionResult ProfileUser()
    {
        if (!CheckToken())
        {
            return RedirectToAction("login" , "auth");
        }
        return View();
    }

    [HttpGet]
    [Authorize]
    public IActionResult UserSetting()
    {
        if (!CheckToken())
        {
            return RedirectToAction("login");
        }
        return View();
    }
 
}
