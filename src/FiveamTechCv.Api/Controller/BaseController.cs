using System.Net;
using FiveamTechCv.Abstract.Filters;
using FiveamTechCv.Abstract.Services;
using FiveamTechCv.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FiveamTechCv.Api.Controller;

[ApiController]
public abstract class BaseController<TEntity, TFilter> : ControllerBase
    where TEntity : BaseNode
    where TFilter : INodeFilter
{
    internal readonly INodeService<TEntity, TFilter> _service;
    
    
    public BaseController(
        INodeService<TEntity, TFilter> service
    )
    {
        _service = service;
    }
    
    [HttpGet("{id}")]
    public virtual async Task<TEntity> GetByIdAsync(string id)
    {
        var result = await _service.GetByIdAsync(id);
        if(result == null)
        {
            throw new FiveamTechCvException(
                HttpStatusCode.NotFound,
                FiveamTechCvException.NotFound,
                $"Entity with id {id} not found"
            );
        }
        return result;
    }
    
    [HttpGet]
    public virtual async Task<IEnumerable<TEntity>> ListAsync([FromQuery]TFilter filter)
    {
        var result = await _service.ListAsync(filter);
        return result;
    }
    
    [HttpPost]
    public virtual async Task<string> CreateAsync(TEntity entity)
    {
        var result = await _service.CreateAsync(entity);
        return result;
    }
    
    [HttpPut]
    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        var result = await _service.UpdateAsync(entity);
        return result;
    }
    
    [HttpDelete("{id}")]
    public virtual async Task<bool> DeleteAsync(string id)
    {
        var result = await _service.DeleteAsync(id);
        return result;
    }
    
}