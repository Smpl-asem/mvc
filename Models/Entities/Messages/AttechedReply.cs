using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class AttechedReply : Parent
{
    public string? FileName { get; set; }

    [ForeignKey("Reply")]
    public int? ReplyId { get; set; }
    public Reply? Reply { get; set; }
    public string? FilePath { get; set; }
    public string? FileType { get; set; }

    public AttechedReply()
    {
        this.FileName = null;
        this.ReplyId = null;
        this.Reply = null;
        this.FilePath = null;
        this.FileType = null;
    }
}