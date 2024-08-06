using System.ComponentModel.DataAnnotations.Schema;
using NuGet.Protocol.Plugins;

public class Reply : Parent
{
    public int? ParentId { get; set; }
    public Messages? Parent { get; set; }
    public int? SenderUserId { get; set; }
    public Users? SenderUser { get; set; }
    public string? Subject { get; set; }
    public string? BodyText { get; set; }
    public List<AttechedReply>? Atteched { get; set; }

    public Reply()
    {
        this.ParentId = null;
        this.Parent = null;
        this.SenderUserId = null;
        this.SenderUser = null;
        this.Subject = null;
        this.BodyText = null;
        this.Atteched = new List<AttechedReply>();
    }

}