using System.Data.Common;
using System.Globalization;
using System.Net.Sockets;
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
        if (message.ReciversId == null)
        {
            ViewBag.Error = "خطا ، حداقل یک کاربر باید در لیست دریافت کنندگان باشد ... (فایل های پیوستی را مجددا انتخاب نمایید)";
            ViewBag.ReciversId = message.ReciversId;
            ViewBag.CCsId = message.CCsId;
            ViewBag.SerialNumber = message.SerialNumber;
            ViewBag.Subject = message.Subject;
            ViewBag.BodyText = message.BodyText;
            ViewBag.Contacts = HomeController.Contact(db, User);
            return View("AddMail");
        }
        else if (message.CCsId != null)
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
        if (message.CCsId != null)
        {
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
                db.SaveChanges();

            }
        }




        ViewBag.Result = "پیام شما با موفقیت ارسال شد";
        return RedirectToAction("Index", "home");



    }

    [HttpGet]
    public IActionResult index(int Id = 1)
    {
        var data = DataEater(Id, User, db, false);
        ViewBag.Messages = data;
        ViewBag.title = "لیست دریافتی";
        ViewBag.route = "index";
        return View("viewMails");
    }
    [HttpGet]
    public IActionResult recive(int Id = 1)
    {
        var data = DataEater(Id, User, db, false, false);
        ViewBag.Messages = data;
        ViewBag.title = "لیست دریافتی";
        ViewBag.route = "index";
        return View("viewMails");
    }
    [HttpGet]
    public IActionResult Sent(int Id = 1)
    {
        var data = DataEater(Id, User, db, false, true);
        ViewBag.Messages = data;
        ViewBag.title = "لیست ارسالی";
        ViewBag.route = "sent";
        return View("viewMails");
    }
    [HttpGet]
    public IActionResult searchMail(int Id = 1)
    {
        var data = DataEater(Id, User, db, false, true);
        ViewBag.Messages = data;
        ViewBag.title = "لیست ارسالی";
        ViewBag.route = "sent";
        return View();
    }
    [HttpGet]
    public IActionResult trash(int Id = 1)
    {
        var data = DataEater(Id, User, db, true);
        ViewBag.Messages = data;
        ViewBag.title = "سطل زباله";
        ViewBag.route = "trash";
        return View("viewMails");
    }
    [HttpGet]
    public IActionResult TrashEmail(string lastRoute, string page = "1", int Id = 1)
    {
        var check = db.Messages_tbl.Find(Id);
        check.Trashed.Add(Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)));
        db.Messages_tbl.Update(check);
        db.SaveChanges();
        CreateMsgLog(Id, Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)), 7);
        Id = Convert.ToInt32(page);

        return RedirectToAction(lastRoute, "Email", new { Id });
    }
    [HttpGet]
    public IActionResult UnTrashEmail(string lastRoute, string page = "1", int Id = 1)
    {
        var check = db.Messages_tbl.Find(Id);
        check.Trashed.Remove(Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)));
        db.Messages_tbl.Update(check);
        db.SaveChanges();
        CreateMsgLog(Id, Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)), 8);
        Id = Convert.ToInt32(page);

        return RedirectToAction(lastRoute, "Email", new { Id });

    }
    [HttpGet]
    public IActionResult DeleteEmail(string lastRoute, string page = "1", int Id = 1)
    {
        var check = db.Messages_tbl.Find(Id);
        var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        check.Trashed.Remove(userId);
        check.Deleted.Add(userId);
        db.Messages_tbl.Update(check);
        db.SaveChanges();
        CreateMsgLog(Id, Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)), 6);
        Id = Convert.ToInt32(page);

        return RedirectToAction(lastRoute, "Email", new { Id });

    }

    static public (List<ResultMessage>, int, int, int, int) DataEater(int pageNumber, ClaimsPrincipal User, Context db, bool isTrash = false, bool? isSend = null, int? pSize = null)
    {
        var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var message3Filter = new messagefilter(userId);
        var query = db.Messages_tbl
        .Include(x => x.SenderUser)
        .Include(x => x.Recivers)
            .ThenInclude(x => x.Reciver)
        .Include(x => x.Atteched)
        .AsQueryable();

        message3Filter.DeleteCheck(ref query);
        message3Filter.RelatedItSelf(ref query);
        if (isTrash)
            message3Filter.ApplyMessageFilters(ref query, new MessageDetailsFilter { Trash = true });
        else
            message3Filter.ApplyMessageFilters(ref query, new MessageDetailsFilter { Trash = false });

        if (isSend == true)
            message3Filter.ApplyMessageFilters(ref query, new MessageDetailsFilter { SenderUserId = userId });
        else if (isSend == false)
            message3Filter.ApplyReciverFilters(ref query, new ReciverDetailsFilter { ReciverId = userId });

        var pageSize = pSize.HasValue ? (int)pSize : 10;
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
    public IActionResult ReturnEmail(int? Id = null, int? logPage = 1 , bool isLog = false)
    {
        var data = search(1, User, db, null, Id);
        if (data.Item1.Count == 0 )
        {
            ViewBag.Error = "مشکلی پیش امده ، ایمیل مورد نظر یافت نشد .";
            return View("viewMails");

        }
        else
        {
            ViewBag.Messages = data;
            ViewBag.title = $"ایمیل شماره {data.Item1[0].MessageSerialNumber}";
            ViewBag.route = "ReturnEmail";
            var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if(!db.msgLog_tbl.ToList().Any(x=> x.UserId == userId && x.MessageId == Id && x.LogAction == 1)){
                CreateMsgLog((int)Id, userId, 1);
            }
            ViewBag.MsgLog = Log.AllMsgLog(db, User, (int)Id, null, 10, logPage);
            ViewBag.isLog= isLog;
            return View("returnEmail");
        }
    }

    [HttpGet]
    public IActionResult Search(string text, int Id = 1)
    {
        var data = search(Id, User, db, text);
        ViewBag.Messages = data;
        ViewBag.title = $"نتایج جستجو برای \"{text}\"";
        ViewBag.route = "Search";
        ViewBag.text = text;
        return View("viewMails");
    }

    static public (List<ResultMessage>, int, int, int, int) search(int pageNumber, ClaimsPrincipal User, Context db, string? text = null, int? messageId = null)
    {
        var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var message3Filter = new messagefilter(userId);
        var query = db.Messages_tbl
        .Include(x => x.SenderUser)
        .Include(x => x.Recivers)
            .ThenInclude(x => x.Reciver)
        .Include(x => x.Atteched)
        .AsQueryable();

        if (!String.IsNullOrEmpty(text))
            message3Filter.SearchBodyAndSubject(ref query, (string)text);
        if (messageId.HasValue)
            message3Filter.SearchByMessageId(ref query, (int)messageId);

        message3Filter.RelatedItSelf(ref query);
        message3Filter.DeleteCheck(ref query);


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
    static public (string, string) persianDate(DateTime? date)
    {
        PersianCalendar pc = new PersianCalendar();
        var LocalData = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(date), TimeZoneInfo.Local);
        var year = pc.GetYear(LocalData).ToString().Length != 1 ? pc.GetYear(LocalData).ToString() : "0" + pc.GetYear(LocalData).ToString();
        var Month = pc.GetMonth(LocalData).ToString().Length != 1 ? pc.GetMonth(LocalData).ToString() : "0" + pc.GetMonth(LocalData).ToString();
        var day = pc.GetDayOfMonth(LocalData).ToString().Length != 1 ? pc.GetDayOfMonth(LocalData).ToString() : "0" + pc.GetDayOfMonth(LocalData).ToString();
        var hour = pc.GetHour(LocalData).ToString().Length != 1 ? pc.GetHour(LocalData).ToString() : "0" + pc.GetHour(LocalData).ToString();
        var min = pc.GetMinute(LocalData).ToString().Length != 1 ? pc.GetMinute(LocalData).ToString() : "0" + pc.GetMinute(LocalData).ToString();
        var second = pc.GetSecond(LocalData).ToString().Length != 1 ? pc.GetSecond(LocalData).ToString() : "0" + pc.GetSecond(LocalData).ToString();
        return new($"{year}/{Month}/{day}", $"{hour}:{min}:{second}");
    }
}