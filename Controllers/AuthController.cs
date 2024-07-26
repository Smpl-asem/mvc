using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using test.Models;

public class AuthController : Controller
{
    private readonly string salt;
    private readonly Context db;


    public AuthController(Context _db)
    {
        salt = "S@lt?";
        db = _db;
    }


    [HttpGet]
    [Route("Login")]
    public IActionResult login()
    {
        return View();
    }


    [HttpPost]
    [Route("Login")]
    public IActionResult login(string Username, string Password)
    {
        Users check = db.Users_tbl.FirstOrDefault(x => x.Username == Username.ToLower());
        if (check == null)
        {
            return NotFound($"{Username} not found");
        }
        else if (!BCrypt.Net.BCrypt.Verify(Password + salt + Username.ToLower(), check.Password))
        {
            CreateUserLog((int)check.Id, 1, false);
            return Ok("Invalid Password !");
        }
        else
        {
            CreateUserLog((int)check.Id, 1, true);
            //return Ok(CreateToken(check.Username, check.Id.ToString()));
            return Ok("login Done");
        }
        return View();
    }

    [HttpGet]
    [Route("Register")]
    public IActionResult Register()
    {
        return View();
    }
    [HttpPost]
    [Route("Register")]
    public IActionResult Register(DtodUser user)
    {
        if (user.IsNullOrEmpty())
        {
            return Ok("Complete Data Pls");
        }
        Users check = db.Users_tbl.FirstOrDefault(x => x.Username == user.Username || x.NatinalCode == user.NatinalCode || x.Phone == user.Phone);
        if (check != null)
        {
            if (check.Username == user.Username.ToLower())
            {
                return Ok("Invalid Username");
            }
            else if (check.NatinalCode == user.NatinalCode)
            {
                return Ok("Invalid Natinal Code");
            }
            else if (check.Phone == user.Phone)
            {
                return Ok("Invalid Phone");
            }
        }
        var NewUser = new Users
        {
            Username = user.Username.ToLower(),
            Password = BCrypt.Net.BCrypt.HashPassword(user.Password + salt + user.Username.ToLower()),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            Addres = user.Addres,
            NatinalCode = user.NatinalCode,
            PerconalCode = user.PerconalCode,
            Profile = Uploadimage.Upload(user.Profile),
            CreateDateTime = DateTime.Now
        };
        db.Users_tbl.Add(NewUser);
        db.SaveChanges();

        db.UserRoles_tbl.Add(new UserRole
        {
            UserId = (int)NewUser.Id,
            RoleId = 2
        });
        db.SaveChanges();

        CreateUserLog((int)NewUser.Id, 7, true);
        CreateUserLog((int)NewUser.Id, 3, true);
        return Ok("Succesful !");
        return View();
    }

    [HttpGet]
    [Route("forget")]
    public IActionResult Forget()
    {
        return View();
    }
    [HttpPost]
    [Route("forget")]
    public IActionResult Forget(string Username, string NatinalCode)
    {
        Users check = db.Users_tbl.FirstOrDefault(x => x.Username == Username.ToLower() && x.NatinalCode == NatinalCode);
        if (check == null)
        {
            return Ok("Invalid Data");
        }

        // sms check
        smsUser request = db.sms_tbl.FirstOrDefault(x => x.UserId == check.Id);



        if (request != null)
        {
            if (DateTime.Now.AddMinutes(-10) < request.CreateDateTime)
            {

                CreateUserLog((int)check.Id, 4, false);

                //return Ok("you Must Wait about 10 min");

                ViewBag.smsPhone = check.Phone.Substring(check.Phone.Count() - 4) + "*****09";
                ViewBag.userId = check.Id;
                return View("Verify");
            }
            else
            {
                db.sms_tbl.Remove(request);
            }
        }
        Random random = new Random();
        smsUser newSms = new smsUser
        {
            TryCount = 0,
            SmsCode = random.Next(100000, 999999).ToString(),
            UserId = (int)check.Id,
            IsValid = true,
            CreateDateTime = DateTime.Now
        };
        db.sms_tbl.Add(newSms);
        db.SaveChanges();

        CreateUserLog((int)check.Id, 4, true);

        //return Ok(SmsCode(newSms.SmsCode, check.Phone));
        ViewBag.smsPhone =  check.Phone.Substring(check.Phone.Count()-4) + "*****09";
        ViewBag.userId = check.Id;
        return View("Verify");

    }
    [HttpPost]
    [Route("api/verify")]
     public IActionResult Verify(int userid , string otp)
    {
        Users check = db.Users_tbl.Find(userid);
        if (check == null)
        {
            return Ok("Invalid User");
        }
        //sms Check
        smsUser smsCheck = db.sms_tbl.FirstOrDefault(x => x.UserId == check.Id);
        if (smsCheck == null)
        {
            CreateUserLog((int)check.Id, 5, false);
            return Ok("Haven't Code Requset. try Reset First");

        }
        else if (DateTime.Now.AddMinutes(-10) > smsCheck.CreateDateTime)
        { //Time Passed
            db.sms_tbl.Remove(smsCheck);
            db.SaveChanges();
            CreateUserLog((int)check.Id, 5, false);
            return Ok("Code Time Expire ... Try again");
        }
        else if (smsCheck.IsValid == true)
        {
            if (otp == smsCheck.SmsCode)
            {
                //check.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword + salt + Username.ToLower());
                //db.Users_tbl.Update(check);
                db.sms_tbl.Remove(smsCheck);
                db.SaveChanges();
                //CreateUserLog((int)check.Id, 5, true);
                return Ok("Sucssesful");
            }
            else
            {
                if (smsCheck.TryCount > 3) // start from 0 -> 1,2,3,4 -> when 4 still can try 5 ! done
                    smsCheck.IsValid = false;
                else
                    ++smsCheck.TryCount;
                db.sms_tbl.Update(smsCheck);
                db.SaveChanges();
                CreateUserLog((int)check.Id, 5, false);
                return Ok("Code is Invalid");
            }
        }
        else
        {
            CreateUserLog((int)check.Id, 5, false);
            return Ok("you Must Try 10 min later.");
        }
    }

    private string SmsCode(string Code, string Phone)
    {
        // real sms
        // KavenegarApi SmsApi = new KavenegarApi(db.smsTokens.Find(1).Token);
        // SmsApi.VerifyLookup(Phone, Code, "demo");
        // return "Sms Sended";

        // price less
        return $"{Code} Sent to {Phone} .";
    }

    private void CreateUserLog(int UserId, int LogAction, bool isSucces)
    {
        db.userLogs_tbl.Add(new UserLog
        {
            UserId = UserId,
            LogAction = LogAction,
            isSucces = isSucces,
            CreateDateTime = DateTime.Now
        });
        db.SaveChanges();
    }

}
