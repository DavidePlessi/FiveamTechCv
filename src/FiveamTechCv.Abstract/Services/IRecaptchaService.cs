namespace FiveamTechCv.Abstract.Services;

public interface IRecaptchaService
{
    Task<bool> VerifyTokenAsync(string token);
}
