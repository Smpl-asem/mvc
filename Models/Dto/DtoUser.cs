using Microsoft.IdentityModel.Tokens;

public class DtodUser
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
    public string Addres { get; set; }
    public string NatinalCode { get; set; }
    public string PerconalCode { get; set; }
    public IFormFile Profile { get; set; }
    public string Path { get; set; }

}