using System.Data.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class EmailController:Controller
{
    private readonly Context db;
    public EmailController(Context _db)
    {
        db = _db;
    }
    
    public IActionResult AddMail(){
        return Ok();
    }
}