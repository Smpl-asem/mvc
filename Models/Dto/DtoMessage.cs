public class DtoMessage
{
    public string SerialNumber { get; set; }
    public int SenderUserId { get; set; }
    public string Subject { get; set; }
    public string BodyText { get; set; }
    // public List<DtoRecivers> Resivers { get; set; }
    public List<int> ReciversId { get; set; }
    public List<int> CCsId { get; set; }
    public List<IFormFile>? Files { get; set; }
}


