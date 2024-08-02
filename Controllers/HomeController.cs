using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test.Models;


[Authorize]
public class HomeController : Controller
{
    private readonly Context db;
    public HomeController(Context _db)
    {
        db = _db;
    }
    public IActionResult Index()
    {
        ViewBag.Contacts = Contact(db , User);
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



    static public List<(string, int, string , string)> Contact(Context db , ClaimsPrincipal User) // گرفتن لیست کاربران
    {
        List<(string, int, string , string)> Result = new List<(string, int, string , string)>();
        foreach (var item in db.Users_tbl.ToList())
        {
            if (item.Id != Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value))
            {
                Result.Add(new($"../../../{item.Profile}", (int)item.Id, $"{item.FirstName} {item.LastName}",item.Username));
            }
        }

        return Result;
    }

    [HttpPost]
    public IActionResult AddMail(DtoMessage message){
        return RedirectToAction("AddMail","Email",message);
    }
}
