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

    [HttpPost]
    public async Task<IActionResult> AddMail(DtoMessage message)
    {
        if (message.ReciversId == null)
        {
            ViewBag.NewError = "خطا ، حداقل یک کاربر باید در لیست دریافت کنندگان باشد ... (فایل های پیوستی را مجددا انتخاب نمایید)";
            ViewBag.ReciversId = message.ReciversId;
            ViewBag.CCsId = message.CCsId;
            ViewBag.SerialNumber = message.SerialNumber;
            ViewBag.Subject = message.Subject;
            ViewBag.BodyText = message.BodyText;
            ViewBag.Contacts = HomeController.Contact(db, User);
            int id = 5;
            return index(1,1,1,1,5);
        }
        else if (message.CCsId != null)
        {
            if (message.ReciversId.Any(x => message.CCsId.Contains(x)))
            {
                ViewBag.NewError = "خطا ، کاربر نمیتواند همزمان در لیست گیرنده و رونوشت باشد . لطفا مجددا تلاش کنید... (فایل های پیوستی را مجددا انتخاب نمایید)";
                ViewBag.ReciversId = message.ReciversId;
                ViewBag.CCsId = message.CCsId;
                ViewBag.SerialNumber = message.SerialNumber;
                ViewBag.Subject = message.Subject;
                ViewBag.BodyText = message.BodyText;
                ViewBag.Contacts = HomeController.Contact(db, User);
                int id = 5;
                return index(1,1,1,1,5);
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
        return RedirectToAction("Index", "Email");



    }

    [HttpPost]
    public async Task<IActionResult> AddReply(DtoReply reply){
        if(reply.MessageId == null){
            return Ok("ریدیییییییییییییییییییییییی");
        }
        var UserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        Reply newReply = new Reply{
            ParentId = reply.MessageId,
            SenderUserId = UserId,
            Subject = reply.Subject,
            BodyText = reply.BodyText,
            CreateDateTime = DateTime.UtcNow
        };
        db.Reply_tbl.Add(newReply);
        db.SaveChanges();

        int replyId = Convert.ToInt32(newReply.Id);
        CreateMsgLog(reply.MessageId, UserId, 9);

        if (reply.Files != null)
        {
            foreach (var item in reply.Files)
            {
                string FileExtension = Path.GetExtension(item.FileName);
                var NewFileName = String.Concat(Guid.NewGuid().ToString(), FileExtension);
                var path = $"{_env.WebRootPath}\\uploads\\EmailFiles\\ID{reply.MessageId}({replyId})-{NewFileName}";
                string PathSave = $"\\uploads\\EmailFiles\\ID{replyId}-{NewFileName}";
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await item.CopyToAsync(stream);
                }

                db.AttechedReplies_tbl.Add(new AttechedReply
                {
                    FileName = item.FileName,
                    ReplyId = replyId,
                    FilePath = PathSave,
                    FileType = FileExtension,
                    CreateDateTime = DateTime.UtcNow
                });
                db.SaveChanges();
            }
        }
        ViewBag.Result = "پیام شما با موفقیت ارسال شد";
        return RedirectToAction("Index", "Email");
    }

    [HttpGet]
    public IActionResult index(int inPage = 1, int rePage = 1, int sePage = 1, int dePage = 1, int Id = 1)
    {
        var dataIndex = DataEater(inPage, User, db, false);
        ViewBag.MessagesIndex = dataIndex;

        var dataRecive = DataEater(rePage, User, db, false, false);
        ViewBag.MessagesRecive = dataRecive;

        var dataSent = DataEater(sePage, User, db, false, true);
        ViewBag.MessagesSent = dataSent;

        var dataTrash = DataEater(dePage, User, db, true);
        ViewBag.MessagesTrash = dataTrash;

        ViewBag.page = Id;
        ViewBag.var = 1;

        ViewBag.Contacts = HomeController.Contact(db, User);
        ViewBag.title = "لیست دریافتی";
        ViewBag.route = "index";
        return View("viewMails");
    }
    [HttpGet]
    public IActionResult TrashEmail(int Id = 1, int? msgId = null)
    {
        if (msgId.HasValue)
        {
            var check = db.Messages_tbl.Find((int)msgId);
            check.Trashed.Add(Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            db.Messages_tbl.Update(check);
            db.SaveChanges();
            CreateMsgLog((int)msgId, Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)), 7);

            return RedirectToAction("Index", "Email", new { Id });
        }
        else
        {
            ViewBag.Error = "مشکلی پیش امده است دوباره تلاش کنید <a hraf=/email/index>خانه</a>";
            return RedirectToAction("Index", "Email");
        }
    }
    [HttpGet]
    public IActionResult UnTrashEmail(int Id = 1, int? msgId = null)
    {
        var check = db.Messages_tbl.Find((int)msgId);
        try
        {
            check.Trashed.Remove(Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)));

        }
        catch
        {
            //empty !!!
        }
        finally
        {
            db.Messages_tbl.Update(check);
            db.SaveChanges();
            CreateMsgLog((int)msgId, Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)), 8);
        }
        return RedirectToAction("Index", "Email", new { Id });




    }
    [HttpGet]
    public IActionResult DeleteEmail(int Id = 1, int? msgId = null)
    {
        var check = db.Messages_tbl.Find((int)msgId);
        var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        try
        {
            check.Trashed.Remove(userId);
            check.Deleted.Add(userId);
            db.Messages_tbl.Update(check);
            db.SaveChanges();
            CreateMsgLog((int)msgId, Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)), 6);
        }
        catch
        {
            //empty !!
        }

        return RedirectToAction("index", "Email", new { Id });

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
    public IActionResult ReturnEmail(int? Id = null, int? logPage = 1, int pg = 1)
    {
        var data = search(1, User, db, null, Id);
        if (data.Item1.Count == 0)
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
            if (!db.msgLog_tbl.ToList().Any(x => x.UserId == userId && x.MessageId == Id && x.LogAction == 1))
            {
                CreateMsgLog((int)Id, userId, 1);
            }
            ViewBag.MsgLog = Log.AllMsgLog(db, User, (int)Id, null, 10, logPage);
            ViewBag.pg = pg;
            return View("returnEmail");
        }
    }

    [HttpGet]
    public IActionResult Search(string text, int Id = 1)
    {
        var data = search(Id, User, db, text);
        ViewBag.Messages = data;
        ViewBag.title = $"نتایج جستجو برای \"{text}\"";
        ViewBag.text = text;
        return View("returnSearch");
    }
    [HttpGet]
    public IActionResult searchMail()
    {
        return View("searchMail");
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
        .Include(x => x.Child)
            .ThenInclude(x=>x.SenderUser)
        .Include(x=> x.Child)
            .ThenInclude(x=> x.Atteched)
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
            }),
            Child = (ICollection<ResultReply>)m.Child.Select(a=> new ResultReply{
                ReplyId = (int)a.Id,
                ReplySubject = a.Subject,
                ReplyBodyText = a.BodyText,
                CreateDate = persianDate(a.CreateDateTime).Item1,
                CreateTime = persianDate(a.CreateDateTime).Item2,
                SenderUserId = (int)a.SenderUser.Id,
                SenderUser = a.SenderUser.Username,
                SenderFirstName = a.SenderUser.FirstName,
                SenderLastName = a.SenderUser.LastName,
                SenderProfile = a.SenderUser.Profile,
                Files = (ICollection<ResultFile>)a.Atteched.Select(x=>new ResultFile{
                    FileId = (int)x.Id,
                    FileName = x.FileName,
                    FilePath = x.FilePath,
                    FileType = x.FileType
                })
            })
        })
            .ToList();
        ;

        return (pageData, pageNumber, pageSize, totalPages, totalCount);
        // return View();
    }

    public void CreateMsgLog(int MessageId, int UserId, int LogAction)
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