using Microsoft.AspNetCore.Mvc;
using Poprawa1.Models;
using WebApplication1.Exceptions;
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
        catch (Exception e) when (e is ArtifactExistsException || e is ProjectExistsException)
        {
            return BadRequest(e.Message);
        }

        catch (InstitutionDoesntExistException e)
        {
            return NotFound(e.Message);
        }

        catch (Exception e)
        {
            return StatusCode(500, e.Message);
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
        catch (ProjectDoesntExistException e)
        {
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
}