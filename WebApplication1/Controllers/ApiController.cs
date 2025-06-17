using Microsoft.AspNetCore.Mvc;
using Poprawa1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers;


[Microsoft.AspNetCore.Components.Route("[controller]")]
[ApiController]

public class ApiController : ControllerBase
{
    private readonly IDbService _iDbService;

    public ApiController(IDbService iDbService)
    {
        _iDbService = iDbService;
    }
    
    [HttpPost("artifacts")]
    public async Task<IActionResult> AddArtifactAsync([FromBody]InputDTO artifact)
    {

        try
        {
            await _iDbService.AddArtifact(artifact);

        }
        catch (ArgumentException argEx)
        {
            return NotFound(argEx.Message);
        }

        catch (InvalidOperationException ioEx)
        {
            return Conflict(ioEx.Message);
        }

        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }

        return StatusCode(201, artifact);
    }
    [HttpGet("projects/{id}")]
    public async Task<IActionResult> GetProject(int id)
    {
        try
        {
            var project = await _iDbService.GetProjectAsync(id);
            return Ok(project);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}