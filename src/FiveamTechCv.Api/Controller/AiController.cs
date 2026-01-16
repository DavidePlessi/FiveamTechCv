using FiveamTechCv.Abstract.Services;
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
    public async Task<IActionResult> Ask([FromBody] AskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest("Question is required.");
        }

        try 
        {
            var answer = await _aiService.AskAsync(request.Question);
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
}

public class AskRequest
{
    public string? Question { get; set; }
}
