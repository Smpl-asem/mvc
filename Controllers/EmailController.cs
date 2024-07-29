using System.Data.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;

[Authorize]
public class EmailController:Controller
{
    private readonly Context db;
    private readonly IWebHostEnvironment _env;
    public EmailController(Context _db , IWebHostEnvironment env)
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
                    CreateDateTime = DateTime.Now
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
                        CreateDateTime = DateTime.Now
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
                            CreateDateTime = DateTime.Now
                        });

                    }
                }

                db.SaveChanges();
                transaction.Commit();

                ViewBag.Result = "پیام شما با موفقیت ارسال شد";
                return RedirectToAction("Index","home");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
    private void CreateMsgLog(int MessageId, int UserId, int LogAction)
    {
        // Add Log ---->
        db.msgLog_tbl.Add(new MessageLog
        {
            MessageId = MessageId,
            UserId = UserId,
            LogAction = LogAction,
            CreateDateTime = DateTime.Now
        });
        db.SaveChanges();
    }
}