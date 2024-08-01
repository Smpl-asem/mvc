public class ResultMessage
{
    public int MessageId { get; set; }
    public string MessageSerialNumber { get; set; }
    public string MessageSubject { get; set; }
    public string MessageBodyText { get; set; }
    public string CreateDate { get; set; }
    public string CreateTime { get; set; }
    public int SenderUserId { get; set; }
    public string SenderUser { get; set; }
    public string SenderFirstName { get; set; }
    public string SenderLastName { get; set; }
    public string SenderProfile { get; set; }
    public ICollection<ResultReciver> Recivers { get; set; }
    public ICollection<ResultFile> Files { get; set; }

}