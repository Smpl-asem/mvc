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
}