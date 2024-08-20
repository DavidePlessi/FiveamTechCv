using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;

namespace FiveamTechCv.Abstract.Services;

public interface IUserService : INodeService<User, UserFilter>
{
    public Task<string> Login(string username, string password);
    public string ValidateToken(string token);
}