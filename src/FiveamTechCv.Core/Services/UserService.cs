using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities;
using FiveamTechCv.Entities.Filters;
using FiveamTechCv.Entities.Nodes;
using FiveamTechCv.Graph;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FiveamTechCv.Core.Services;

public class JwtOptions
{
    public string Key { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
}

public class UserService(GraphDriver driver, IOptions<JwtOptions> jwtOptions)
    : BaseService<User, UserFilter>(driver), IUserService
{
    private static string GetPasswordHash(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }

    private string GenerateToken(string username, bool isAdmin)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim("IsAdmin", isAdmin ? "true" : "false"),
            // Add other claims as needed
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Value.Issuer,
            audience: jwtOptions.Value.Audience,
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public override Task<string> CreateAsync(User node)
    {
        node.Password = GetPasswordHash(node.Password);
        return base.CreateAsync(node);
    }

    public override Task<User> UpdateAsync(User node)
    {
        node.Password = GetPasswordHash(node.Password);
        return base.UpdateAsync(node);
    }

    public async Task<string> Login(string username, string password)
    {
        var user = (await ListAsync(new UserFilter
        {
            Username = username,
            Password = GetPasswordHash(password)
        })).FirstOrDefault();
        
        if (user == null)
        {
            throw new FiveamTechCvException(
                HttpStatusCode.NotFound,
                FiveamTechCvException.NotFound,
                $"User not found"
            );
        }
        
        return GenerateToken(user.Username, user.IsAdmin);
        
        
    }

    public string ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtOptions.Value.Key);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        SecurityToken validatedToken;
        try
        {
            tokenHandler.ValidateToken(token, parameters, out validatedToken);
        }
        catch
        {
            throw new SecurityTokenException("Invalid token");
        }

        var jwtToken = (JwtSecurityToken)validatedToken;
        var usernameClaim = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name);
        var isAdminClaim = jwtToken.Claims.First(x => x.Type == "IsAdmin");

        var username = usernameClaim.Value;
        var isAdmin = bool.Parse(isAdminClaim.Value);

        return GenerateToken(username, isAdmin);
    }
}