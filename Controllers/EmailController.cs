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

    [HttpPost]
    public async Task<IActionResult> AddMail(DtoMessage message)
    {
        using (var transaction = db.Database.BeginTransaction())
        {
            try
            {
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

                    // CreateMsgLog(messageId, item.ReciverId, item.Type == "to" ? 4 : 5);
                }
                if (message.Files != null)
                {
                    foreach (var item in message.Files)
                    {

                        string FileExtension = Path.GetExtension(item.FileName);
                        var NewFileName = String.Concat(Guid.NewGuid().ToString(), FileExtension);
                        var path = $"{_env.WebRootPath}\\uploads\\EmailFiles\\{messageId}\\{NewFileName}";
                        string PathSave = $"\\uploads\\EmailFiles\\{messageId}\\{NewFileName}";
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

                db.SaveChanges();
                transaction.Commit();

                ViewBag.Result = "پیام شما با موفقیت ارسال شد";
                return RedirectToAction("Index", "home");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }

    [HttpGet]
    public IActionResult viewMails(int pageNumber, bool isTrash = false, bool isSend = true, bool isOne = false)
    {
        var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var message3Filter = new messagefilter(userId);
        var query = db.Messages_tbl
        .Include(x => x.SenderUser)
        .Include(x => x.Recivers)
            .ThenInclude(x => x.Reciver)
        .Include(x => x.Atteched)
        .AsQueryable();

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
        var pageData = query
        .OrderByDescending(x => x.Id)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(m => new
        {
            m.Id,
            SenderUser = new
            {
                m.SenderUser.Id,
                m.SenderUser.Username,
                m.SenderUser.FirstName,
                m.SenderUser.LastName,
                // m.SenderUser.Phone,   حفظ حریم شخصی افراد با فاش نکردن اطلاعات حساس
                // m.SenderUser.Addres,
                // m.SenderUser.NatinalCode,
                // m.SenderUser.PerconalCode,
                // m.SenderUser.CreateDateTime
            },
            CreateDate = persianDate(m.CreateDateTime).Item1,
            CreateTime = persianDate(m.CreateDateTime).Item2,
            m.SerialNumber,
            m.Subject,
            m.BodyText,
            Recivers = m.Recivers.Select(r => new
            {
                // r.Id,
                // r.ReciverId,
                // r.MessageId,
                // r.Type,
                // r.CreateDateTime,
                Reciver = new
                {
                    r.Reciver.Id,
                    r.Reciver.Username,
                    r.Reciver.FirstName,
                    r.Reciver.LastName,
                    r.Type
                    // r.Reciver.Phone,
                    // r.Reciver.Addres,
                    // r.Reciver.NatinalCode,
                    // r.Reciver.PerconalCode,
                    // r.Reciver.CreateDateTime
                }
            }),
            Atteched = m.Atteched.Select(a => new
            {
                a.Id,
                a.FileName,
                // a.MessageId,
                a.FilePath,
                a.FileType,
                // a.CreateDateTime
            })
        })
            .ToList();
        var pagedResponse = new PagedResponse<object>(pageData, pageNumber, pageSize, totalPages, totalCount);

        ViewBag.Emails = pagedResponse;
        return Ok(pagedResponse);
        // return View();
    }

    [HttpGet]
    public IActionResult searchMail(int pageNumber , string text)
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
        .Select(m => new
        {
            m.Id,
            SenderUser = new
            {
                m.SenderUser.Id,
                m.SenderUser.Username,
                m.SenderUser.FirstName,
                m.SenderUser.LastName,
                // m.SenderUser.Phone,   حفظ حریم شخصی افراد با فاش نکردن اطلاعات حساس
                // m.SenderUser.Addres,
                // m.SenderUser.NatinalCode,
                // m.SenderUser.PerconalCode,
                // m.SenderUser.CreateDateTime
            },
            CreateDate = persianDate(m.CreateDateTime).Item1,
            CreateTime = persianDate(m.CreateDateTime).Item2,
            m.SerialNumber,
            m.Subject,
            m.BodyText,
            Recivers = m.Recivers.Select(r => new
            {
                // r.Id,
                // r.ReciverId,
                // r.MessageId,
                // r.Type,
                // r.CreateDateTime,
                Reciver = new
                {
                    r.Reciver.Id,
                    r.Reciver.Username,
                    r.Reciver.FirstName,
                    r.Reciver.LastName,
                    r.Type
                    // r.Reciver.Phone,
                    // r.Reciver.Addres,
                    // r.Reciver.NatinalCode,
                    // r.Reciver.PerconalCode,
                    // r.Reciver.CreateDateTime
                }
            }),
            Atteched = m.Atteched.Select(a => new
            {
                a.Id,
                a.FileName,
                // a.MessageId,
                a.FilePath,
                a.FileType,
                // a.CreateDateTime
            })
        })
            .ToList();
        var pagedResponse = new PagedResponse<object>(pageData, pageNumber, pageSize, totalPages, totalCount);

        ViewBag.Emails = pagedResponse;
        return Ok(pagedResponse);
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