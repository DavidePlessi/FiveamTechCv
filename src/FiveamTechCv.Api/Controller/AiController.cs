using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Api.Attributes;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiveamTechCv.Api.Controller;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly IAiCVService _aiService;
    private readonly IVectorSearchService _vectorSearchService;

    public AiController(IAiCVService aiService, IVectorSearchService vectorSearchService)
    {
        _aiService = aiService;
        _vectorSearchService = vectorSearchService;
    }

    [HttpPost("ask")]
    [Recaptcha]
    public async Task<IActionResult> Ask([FromBody] AskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest("Question is required.");
        }

        try 
        {
            var answer = await _aiService.AskAsync(request.Question, request.History);
            return Ok(new { Answer = answer });
        }
        catch (Exception ex)
        {
             // Log exception
             Console.WriteLine(ex);
             return StatusCode(500, "An error occurred while processing your request.");
        }
    }
    
    [HttpPost("create-index")]
    [Authorize]
    public async Task<IActionResult> CreateIndex()
    {
        var isAdmin = User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value == "true";
        if(!isAdmin)
        {
            return Unauthorized("Unauthorized action");
        }
        try
        {
            await _vectorSearchService.CreateIndexAsync();
            return Ok("Index created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, "An error occurred while creating the index.");
        }
    }

    [HttpPost("recalculate-embeddings")]
    [Authorize]
    public async Task<IActionResult> RecalculateEmbeddings()
    {
        var isAdmin = User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value == "true";
        if(!isAdmin)
        {
            return Unauthorized("Unauthorized action");
        }
        try
        {
            await _vectorSearchService.RecalculateEmbeddingsAsync();
            return Ok("Embeddings recalculated successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, "An error occurred while recalculating embeddings.");
        }
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest("Query is required.");
        }

        try
        {
            var results = await _vectorSearchService.SearchAsync(request.Query, request.K);
            return Ok(results);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, "An error occurred while searching.");
        }
    }
}

public class AskRequest
{
    public string? Question { get; set; }
    public List<string> History { get; set; } = new();
}

public class SearchRequest
{
    public string? Query { get; set; }
    public int K { get; set; } = 5;
}
