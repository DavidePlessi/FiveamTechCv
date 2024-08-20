using System.Net;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Api.Models.User;
using FiveamTechCv.Entities;
using FiveamTechCv.Entities.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveamTechCv.Api.Controller;

[ApiController]
[Route("api/user")]
public class UserController(IUserService userService, IWebHostEnvironment env) : ControllerBase
{
    [HttpPost("login")]
    public async Task<string> LoginAsync([FromBody] LoginModel model)
    {
        return await userService.Login(model.Username, model.Password);
    }

    [HttpPost("validate-token")]
    [Authorize]
    public string ValidateToken()
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        
        return userService.ValidateToken(token);
    }

    [HttpPost("create-user")]
    // [Authorize]
    public async Task<string> CreateUser(CreateUserModel data)
    {
        var isAuthorized = env.IsDevelopment() || User.Identity?.IsAuthenticated == true;
        if(!isAuthorized)
        {
            throw new FiveamTechCvException(
                HttpStatusCode.Unauthorized,
                FiveamTechCvException.Unauthorized,
                "Unauthorized action"
            );
        }
        
        var isAdmin = env.IsDevelopment() || User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value == "true";
        if(!isAdmin)
        {
            throw new FiveamTechCvException(
                HttpStatusCode.Unauthorized,
                FiveamTechCvException.Unauthorized,
                "Unauthorized action"
            );
        }
        
        
        var result = await userService.CreateAsync(new User
        {
            Username = data.Username,
            Password = data.Password,
            IsAdmin = data.IsAdmin
        });
        
        return result;
    }
}