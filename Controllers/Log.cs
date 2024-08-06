using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class Log
{
    static public (List<ResultUserLog>, int, int, int, int) AllUserLog(Context db, ClaimsPrincipal User, int? UserId = null, int? LogAction = null, int? PageSize = 5, int? PageNumber = 1)
    {
        var query = db.userLogs_tbl
        .Include(x => x.User)
        .OrderByDescending(x => x.Id)
        .AsQueryable();

        var userId = UserId.HasValue ? (int)UserId : Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (LogAction.HasValue)
        {//چک کردن بر اساس عملیات و اکشن
            query = query.Where(x => x.LogAction == (int)LogAction);
        }
        query = query.Where(x => x.UserId == userId);

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / (int)PageSize);

        var pagedData = query
            .OrderByDescending(x => x.Id)
            .Skip(((int)PageNumber - 1) * (int)PageSize)
            .Take((int)PageSize)
            .Select(m => new ResultUserLog
            {
                Id = (int)m.Id,
                WhoDone = new DtoGetUser
                {
                    Id = (int)m.User.Id,
                    Name = $"{m.User.FirstName} {m.User.LastName}",
                    Profile = m.User.Profile,
                    Username = m.User.Username
                },
                LogAction = userCodeToAction(m.LogAction, m.isSucces),
                CreateDate = EmailController.persianDate(m.CreateDateTime).Item1,
                CreateTime = EmailController.persianDate(m.CreateDateTime).Item2
            })
            .ToList();


        return (pagedData, (int)PageNumber, (int)PageSize, totalPages, totalCount);
    }

    static public string GetUserLog(ResultUserLog data)
    {
        return $"{data.WhoDone.Name} ({data.WhoDone.Username})  {data.LogAction} در تاریخ {data.CreateDate} و ساعت {data.CreateTime}";
    }

    static private string userCodeToAction(int code, bool done)
    {
        switch (code)
        { //1) Login /2) Logout /3) Register /4) ResetPassword /5) Verify password /6) Update User /7) Turn to Clinet /8) admin /9) owner
            case 1:
                return done ? "موفق به ورود شد" : "موفق به ورود نشد";
            case 2:
                return done ? "موفق به خروج شد" : "موفق به خروج نشد";
            case 3:
                return done ? "موفق به ثبت نام شد" : "موفق به ثبت نام نشد";
            case 4:
                return done ? "درخواست تغییر رمز داد" : "نتوانست درخواست تغییر رمز بدهد";
            case 5:
                return done ? "موفق به تغییر رمز شد" : "موفق به تغیر رمز نشد";
            case 6:
                return done ? "اطلاعات خود را بروز کرد" : "نتوانست اطلاعات خود را بروز کند";
            case 7:
                return done ? "به عنوان کلاینت تعریف شد" : "عنوان کلاینت را از دست داد";
            case 8:
                return done ? "به عنوان ادمین تعریف شد" : "عنوان ادمین را از دست داد";
            case 9:
                return done ? "به عنوان مالک تعریف شد" : "عنوان مالک را از دست داد";
            default:
                return "WTF ? how You Get THERE ???";
        }
    }
    static public (List<ResultMsgLog>, int, int, int, int) AllMsgLog(Context db, ClaimsPrincipal User, int MessageId, int? LogAction = null, int? PageSize = 10, int? PageNumber = null)
    {
        var query = db.msgLog_tbl
        .Where(x => x.MessageId == MessageId)
        .Include(x => x.User)
        .Include(x => x.Message)
        .AsQueryable();

        PageNumber = PageNumber.HasValue ? PageNumber : 1 ;


        if (LogAction.HasValue)
        {//چک کردن بر اساس عملیات و اکشن
            query = query.Where(x => x.LogAction == (int)LogAction);
        }

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / (int)PageSize);

        var pagedData = query
            .OrderByDescending(x => x.Id)
            .Skip(((int)PageNumber - 1) * (int)PageSize)
            .Take((int)PageSize)
            .Select(m => new ResultMsgLog
            {
                Id = (int)m.Id,
                WhoDone = new DtoGetUser
                {
                    Id = (int)m.User.Id,
                    Name = $"{m.User.FirstName} {m.User.LastName}",
                    Profile = m.User.Profile,
                    Username = m.User.Username
                },
                WhatDone = new DtoGetMessage
                {
                    MessageId = (int)m.Message.Id,
                    SerialNumber = m.Message.SerialNumber,
                    BodyText = m.Message.BodyText,
                    SenderUserId = (int)m.Message.SenderUserId,
                    Subject = m.Message.Subject
                },
                CreateDate = EmailController.persianDate(m.CreateDateTime).Item1,
                CreateTime = EmailController.persianDate(m.CreateDateTime).Item2,
                LogAction = msgCodeToAction(m.LogAction)
            }

            )
            .ToList();
        return (pagedData, (int)PageNumber, (int)PageSize, totalPages, totalCount);
    }

    static public string GetMsgLog(ResultMsgLog data)
    {
        return $"{data.WhoDone.Name} ({data.WhoDone.Username}) نامه شماره {data.WhatDone.SerialNumber} را {data.LogAction}";
    }

    static private string msgCodeToAction(int code)
    {
        switch (code)
        { //1) read / 2) empty / 3) send / 4) Recive as to / 5) Recive As CC / 6)upload / 7) trash / 8) Untrash
            case 1:
                return "خواند";
            case 2:
                return "SOON";
            case 3:
                return "ارسال کرد";
            case 4:
                return "دریافت کرد (to)";
            case 5:
                return "رونوشت را دریافت کرد (CC)";
            case 6:
                return "حذف دائمی کرد";
            case 7:
                return "حذف موقت کرد";
            case 8:
                return "از لیست حذف شده ها خارج کرد";
            default:
                return "WTF ? how You Get THERE ???";

        }
    }
    static int startNum = 1;
    static public int countForMe(){
        int Result = startNum ;
        startNum ++ ;
        return Result;
    }
    static public void countForMe(int num){
        startNum = num;
    }

    static public string isActive(int page , int code){
        if(page == code){
            return "tab-pane fade show active";
        }
        else{
            return "tab-pane fade";
        }
    }
}