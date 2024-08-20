namespace FiveamTechCv.Entities.Nodes;

public class WorkExperience : BaseNode
{
    public string? Company { get; set; }
    public string? Position { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}