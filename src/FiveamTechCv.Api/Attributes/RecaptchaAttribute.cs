using FiveamTechCv.Abstract.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FiveamTechCv.Api.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RecaptchaAttribute : ActionFilterAttribute
{
    private const string RecaptchaHeaderName = "x-recaptcha-token";

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1. Check for token in header
        if (!context.HttpContext.Request.Headers.TryGetValue(RecaptchaHeaderName, out var tokenValue) || string.IsNullOrWhiteSpace(tokenValue))
        {
            context.Result = new BadRequestObjectResult("Recaptcha token is missing.");
            return;
        }

        // 2. Resolve service
        var recaptchaService = context.HttpContext.RequestServices.GetService<IRecaptchaService>();
        if (recaptchaService == null)
        {
            // Should not happen if registered correctly
            context.Result = new StatusCodeResult(500);
            return;
        }

        // 3. Verify
        var isValid = await recaptchaService.VerifyTokenAsync(tokenValue.ToString());
        if (!isValid)
        {
            context.Result = new BadRequestObjectResult("Recaptcha validation failed.");
            return;
        }

        await next();
    }
}
