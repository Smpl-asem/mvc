using System.Data.Common;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

[Authorize]
public class EmailController : Controller
{
    private readonly Context db;
    private readonly IWebHostEnvironment _env;
    public EmailController(Context _db, IWebHostEnvironment env)
    {
        db = _db;
        _env = env;
    }

    [HttpGet]
    public IActionResult AddMail()
    {
        ViewBag.Contacts = HomeController.Contact(db, User);
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddMail(DtoMessage message)
    {
        if (message.ReciversId.Any(x => message.CCsId.Contains(x)))
        {
            ViewBag.Error = "خطا ، کاربر نمیتواند همزمان در لیست گیرنده و رونوشت باشد . لطفا مجددا تلاش کنید... (فایل های پیوستی را مجددا انتخاب نمایید)";
            ViewBag.ReciversId = message.ReciversId;
            ViewBag.CCsId = message.CCsId;
            ViewBag.SerialNumber = message.SerialNumber;
            ViewBag.Subject = message.Subject;
            ViewBag.BodyText = message.BodyText;
            ViewBag.Contacts = HomeController.Contact(db, User);
            return View("AddMail");
        }


        Messages newMessage = new Messages
        {
            SerialNumber = message.SerialNumber,
            SenderUserId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value),
            Subject = message.Subject,
            BodyText = message.BodyText,
            CreateDateTime = DateTime.UtcNow
        };

        db.Messages_tbl.Add(newMessage);
        db.SaveChanges();


        int messageId = Convert.ToInt32(newMessage.Id);
        CreateMsgLog(messageId, (int)newMessage.SenderUserId, 3);

        foreach (var item in message.ReciversId)
        {
            db.Recivers_tbl.Add(new Recivers
            {
                ReciverId = item,
                MessageId = messageId,
                Type = "4",
                CreateDateTime = DateTime.UtcNow
            });
            db.SaveChanges();
            CreateMsgLog(messageId, item, 4);
        }
        foreach (var item in message.CCsId)
        {
            db.Recivers_tbl.Add(new Recivers
            {
                ReciverId = item,
                MessageId = messageId,
                Type = "5",
                CreateDateTime = DateTime.UtcNow
            });
            db.SaveChanges();
            CreateMsgLog(messageId, item, 5);
        }

        if (message.Files != null)
                {
                    foreach (var item in message.Files)
                    {

                        string FileExtension = Path.GetExtension(item.FileName);
                        var NewFileName = String.Concat(Guid.NewGuid().ToString(), FileExtension);
                        var path = $"{_env.WebRootPath}\\uploads\\EmailFiles\\ID{messageId}-{NewFileName}";
                        string PathSave = $"\\uploads\\EmailFiles\\ID{messageId}-{NewFileName}";
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await item.CopyToAsync(stream);
                        }

                        db.Attecheds_tbl.Add(new Atteched
                        {
                            FileName = item.FileName,
                            MessageId = messageId,
                            FilePath = PathSave,
                            FileType = FileExtension,
                            CreateDateTime = DateTime.UtcNow
                        });

                    }
                }




        ViewBag.Result = "پیام شما با موفقیت ارسال شد";
        return RedirectToAction("Index", "home");



    }

    [HttpGet]
    public IActionResult index(int Id)
    {
        var data = DataEater(Id);
        ViewBag.Messages = data ;
        ViewBag.title = "لیست دریافتی";
        ViewBag.route = "index";
        return View("viewMails");
    }

    private (List<ResultMessage> , int , int , int , int) DataEater(int pageNumber, bool isTrash = false, bool isSend = true, bool isOne = false)
    {
        var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var message3Filter = new messagefilter(userId);
        var query = db.Messages_tbl
        .Include(x => x.SenderUser)
        .Include(x => x.Recivers)
            .ThenInclude(x => x.Reciver)
        .Include(x => x.Atteched)
        .AsQueryable();

        message3Filter.RelatedItSelf(ref query);
        if (isTrash)
            message3Filter.ApplyMessageFilters(ref query, new MessageDetailsFilter { Trash = true });
        else
            message3Filter.ApplyMessageFilters(ref query, new MessageDetailsFilter { Trash = false });

        if (isSend)
            message3Filter.ApplyMessageFilters(ref query, new MessageDetailsFilter { SenderUserId = userId });
        else
            message3Filter.ApplyReciverFilters(ref query, new ReciverDetailsFilter { ReciverId = userId });

        var pageSize = isOne ? 1 : 10;
        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        List<ResultMessage> pageData = query
        .OrderByDescending(x => x.Id)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(m => new ResultMessage
        {
            MessageId = (int)m.Id,
            SenderUserId = (int)m.SenderUser.Id,
            SenderUser = m.SenderUser.Username,
            SenderFirstName = m.SenderUser.FirstName,
            SenderLastName = m.SenderUser.LastName,
            SenderProfile = m.SenderUser.Profile,

            CreateDate = persianDate(m.CreateDateTime).Item1,
            CreateTime = persianDate(m.CreateDateTime).Item2,
            MessageSerialNumber = m.SerialNumber,
            MessageSubject = m.Subject,
            MessageBodyText = m.BodyText,
            Recivers = (ICollection<ResultReciver>)m.Recivers.Select(r => new ResultReciver
            {
                    ReciverId = (int)r.Reciver.Id,
                    ReciverUserName = r.Reciver.Username,
                    ReciverFirstName = r.Reciver.FirstName,
                    ReciverLastName = r.Reciver.LastName,
                    ReciverType = r.Type,
                    ReciverProfile = r.Reciver.Profile
            })
            ,
            Files = (ICollection<ResultFile>)m.Atteched.Select(a => new ResultFile
            {
                FileId = (int)a.Id,
                FileName = a.FileName,
                FilePath = a.FilePath,
                FileType = a.FileType,
            })
        })
            .ToList();
        ;

        return (pageData, pageNumber, pageSize, totalPages, totalCount);
        // return View();
    }

    [HttpGet]
    public IActionResult ReturnEmail(int Id = 1){
        return Ok();
    }
    
    [HttpGet]
    public IActionResult Search (string text , int Id = 1){
        var data = search(Id , text);
        ViewBag.Messages = data ;
        ViewBag.title = $"نتایج جستجو برای \"{text}\"";
        ViewBag.route = "Search";
        ViewBag.text = text;
        return View("viewMails");
    }

    private (List<ResultMessage> , int , int , int , int) search(int pageNumber, string text)
    {
        var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var message3Filter = new messagefilter(userId);
        var query = db.Messages_tbl
        .Include(x => x.SenderUser)
        .Include(x => x.Recivers)
            .ThenInclude(x => x.Reciver)
        .Include(x => x.Atteched)
        .AsQueryable();

        message3Filter.SearchBodyAndSubject(ref query, text);
        message3Filter.RelatedItSelf(ref query);


        var totalCount = query.Count();
        var pageSize = 10;
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var pageData = query
        .OrderByDescending(x => x.Id)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(m => new ResultMessage
        {
            MessageId = (int)m.Id,
            SenderUserId = (int)m.SenderUser.Id,
            SenderUser = m.SenderUser.Username,
            SenderFirstName = m.SenderUser.FirstName,
            SenderLastName = m.SenderUser.LastName,
            SenderProfile = m.SenderUser.Profile,

            CreateDate = persianDate(m.CreateDateTime).Item1,
            CreateTime = persianDate(m.CreateDateTime).Item2,
            MessageSerialNumber = m.SerialNumber,
            MessageSubject = m.Subject,
            MessageBodyText = m.BodyText,
            Recivers = (ICollection<ResultReciver>)m.Recivers.Select(r => new ResultReciver
            {
                    ReciverId = (int)r.Reciver.Id,
                    ReciverUserName = r.Reciver.Username,
                    ReciverFirstName = r.Reciver.FirstName,
                    ReciverLastName = r.Reciver.LastName,
                    ReciverType = r.Type,
                    ReciverProfile = r.Reciver.Profile
            })
            ,
            Files = (ICollection<ResultFile>)m.Atteched.Select(a => new ResultFile
            {
                FileId = (int)a.Id,
                FileName = a.FileName,
                FilePath = a.FilePath,
                FileType = a.FileType,
            })
        })
            .ToList();
        ;

        return (pageData, pageNumber, pageSize, totalPages, totalCount);
        // return View();
    }

    private void CreateMsgLog(int MessageId, int UserId, int LogAction)
    {
        // Add Log ---->
        db.msgLog_tbl.Add(new MessageLog
        {
            MessageId = MessageId,
            UserId = UserId,
            LogAction = LogAction,
            CreateDateTime = DateTime.UtcNow
        });
        db.SaveChanges();
    }
    static private (string, string) persianDate(DateTime? date)
    {
        PersianCalendar pc = new PersianCalendar();
        var LocalData = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(date), TimeZoneInfo.Local);
        return new($"{pc.GetYear(LocalData)}/{pc.GetMonth(LocalData)}/{pc.GetDayOfMonth(LocalData)}", $"{pc.GetHour(LocalData)}:{pc.GetMinute(LocalData)}:{pc.GetSecond(LocalData)}");
    }
}