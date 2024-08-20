namespace FiveamTechCv.Api.Models.User;

public class CreateUserModel
{
    public string Username { get; set; }
    public string Password { get; set; }
    public bool IsAdmin { get; set; }
}