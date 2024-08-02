using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using test.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;


public class UserController : Controller
{
    private readonly Context db;
    private readonly IWebHostEnvironment _env;


    public UserController(Context _db, IWebHostEnvironment env)
    {
        db = _db;
        _env = env;

    }

    [HttpGet]
    public IActionResult ProfileUser()
    {
        var UserId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);


        Users Check = db.Users_tbl.Find(UserId);
        ViewBag.data = Check;

        var sentCheck = EmailController.DataEater(1, User, db, false, true, 5);
        ViewBag.dataSent = sentCheck;

        var reciveCheck = EmailController.DataEater(1, User, db, false, false, 5);
        ViewBag.dataRecive = reciveCheck;

        var UserLogCheck = Log.AllUserLog(db, User);
        ViewBag.dataUserLog = UserLogCheck;
        return View();
    }

    [HttpGet]
    public IActionResult UserSetting()
    {

        var UserId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

        Users Check = db.Users_tbl.Find(Convert.ToInt32(UserId));
        ViewBag.FirstName = Check.FirstName;
        ViewBag.LastName = Check.LastName;
        ViewBag.Addres = Check.Addres;
        ViewBag.Phone = Check.Phone;
        ViewBag.Profile = Check.Profile;
        ViewBag.NatinalCode = Check.NatinalCode;
        ViewBag.PerconalCode = Check.PerconalCode;

        return View();

    }

    [HttpPost]
    public async Task<IActionResult> UserSetting(DtodUser user)
    {
        Users check = db.Users_tbl.Find(Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value));
        string PathSave = check.Profile;

        if (user.Profile != null)
        {
            string FileExtension = Path.GetExtension(user.Profile.FileName);
            var NewFileName = System.String.Concat(Guid.NewGuid().ToString(), FileExtension);
            var path = $"{_env.WebRootPath}\\uploads\\{NewFileName}";
            PathSave = $"\\uploads\\{NewFileName}";
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await user.Profile.CopyToAsync(stream);
            }
        }

        check.FirstName = user.FirstName;
        check.LastName = user.LastName;
        check.Addres = user.Addres;
        check.PerconalCode = user.PerconalCode;
        check.Profile = PathSave;

        db.Users_tbl.Update(check);
        db.SaveChanges();

        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        ClaimsIdentity Identity = new ClaimsIdentity(new[]
            {

                new Claim(ClaimTypes.Name,check.FirstName+" "+check.LastName),
                new Claim(ClaimTypes.NameIdentifier,check.Id.ToString()),
                new Claim("Profile",check.Profile)

            }, CookieAuthenticationDefaults.AuthenticationScheme);


        var princpal = new ClaimsPrincipal(Identity);

        var properties = new AuthenticationProperties
        {
            ExpiresUtc = DateTime.UtcNow.AddMonths(1),
            IsPersistent = true
        };

        HttpContext.SignInAsync(princpal, properties);

        ViewBag.Result = "به روزرسانی اطلاعات با موفقیت انجام شد";

        ViewBag.FirstName = check.FirstName;
        ViewBag.LastName = check.LastName;
        ViewBag.Addres = check.Addres;
        ViewBag.Phone = check.Phone;
        ViewBag.Profile = check.Profile;
        ViewBag.NatinalCode = check.NatinalCode;
        ViewBag.PerconalCode = check.PerconalCode;

        return View();
    }
}
//< option data - avatar = \"{item.Profile}\" value = \"{item.FirstName} {item.LastName}\" > {item.FirstName} {item.LastName} </ option >