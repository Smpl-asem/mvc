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

    private readonly Context db;
    public UserController(Context _db)
    {
        db = _db;
    }

    [HttpGet]
    public IActionResult ProfileUser()
    {
        if (!CheckToken().Item1)
        {
            return RedirectToAction("login" , "auth");
        }
        return View();
    }

    [HttpGet]
    public IActionResult UserSetting()
    {
        var check = CheckToken();
        if (!check.Item1)
        {
            return RedirectToAction("login", "auth");
        }

        Users userCheck = db.Users_tbl.Find(Convert.ToInt32(check.Item2.FirstOrDefault(x => x.Type == "id").Value));
        ViewBag.UserId = userCheck.Id;
        ViewBag.FirstName = userCheck.FirstName;
        ViewBag.LastName = userCheck.LastName;
        ViewBag.Addres = userCheck.Addres;
        ViewBag.Phone = userCheck.Phone;
        ViewBag.Profile = userCheck.Profile;
        ViewBag.NatinalCode = userCheck.NatinalCode;
        ViewBag.PerconalCode = userCheck.PerconalCode;
        return View();
    }
 
}
