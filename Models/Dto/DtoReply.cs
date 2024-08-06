public class DtoReply
{
    public int MessageId { get; set; }
    public string Subject { get; set; }
    public string BodyText { get; set; }
    public List<IFormFile>? Files { get; set; }
}


