namespace FiveamTechCv.Entities.DTO;

public class SearchResultDto
{
    public string? Id { get; set; }
    public string? Label { get; set; }
    public string? Properties { get; set; }
    
    public string? EmbeddedString { get; set; }
    public double Score { get; set; }
}
