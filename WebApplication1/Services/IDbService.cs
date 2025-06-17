using Poprawa1.Models;

namespace WebApplication1.Services;



public interface IDbService
{
    Task<ProjectDTO> GetProjectAsync(int id);
    Task AddArtifact(InputDTO input);
}