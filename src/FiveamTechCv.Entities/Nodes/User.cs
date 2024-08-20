namespace FiveamTechCv.Entities.Nodes;

public class User : BaseNode
{
    public string Username { get; set; }
    public string Password { get; set; }
    public bool IsAdmin { get; set; }
}