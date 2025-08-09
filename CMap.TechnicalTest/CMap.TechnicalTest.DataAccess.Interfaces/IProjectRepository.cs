using CMap.TechnicalTest.Models;

namespace CMap.TechnicalTest.DataAccess.Interfaces;

public interface IProjectRepository
{
    Project? GetProjectById(Guid id);
    
    IEnumerable<Project> GetProjects();
}