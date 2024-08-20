using System.Dynamic;
using System.Text;
using FiveamTechCv.Abstract.Filters;
using Microsoft.Extensions.Primitives;

namespace FiveamTechCv.Abstract.Filters;

public class UserFilter : BaseNodeFilter
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    
    //
    // public string ToCypherCondition()
    // {
    //     var conditions = new List<string>();
    //     
    //     if (Username != null)
    //     {
    //         conditions.Add("Username: $username");
    //     }
    //     
    //     if (Password != null)
    //     {
    //         conditions.Add("Password: $password");
    //     }
    //     
    //     return conditions.Count > 0 ? "{" + string.Join(", ", conditions) + "}" : "";
    // }
    //
    // public object ToCypherParams()
    // {
    //     var result = new ExpandoObject();
    //     var dictionary = result as IDictionary<string, object>;
    //     
    //     if (Username != null)
    //     {
    //         dictionary.Add("username", Username);
    //     }
    //
    //     if (Password != null)
    //     {
    //         dictionary.Add("password", Password);
    //     }
    //     
    //     return result;
    // }
}