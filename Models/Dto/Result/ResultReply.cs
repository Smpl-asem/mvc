public class ResultReply
{
    public int ReplyId { get; set; }
    public string ReplySubject { get; set; }
    public string ReplyBodyText { get; set; }
    public string CreateDate { get; set; }
    public string CreateTime { get; set; }
    public int SenderUserId { get; set; }
    public string SenderUser { get; set; }
    public string SenderFirstName { get; set; }
    public string SenderLastName { get; set; }
    public string SenderProfile { get; set; }
    public ICollection<ResultFile> Files { get; set; }
}