namespace FiveamTechCv.Abstract.Services;

public interface IAiCVService
{
    Task<string> AskAsync(string question, List<string> history);
}
