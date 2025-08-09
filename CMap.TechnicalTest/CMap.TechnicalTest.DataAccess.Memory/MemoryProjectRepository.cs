using CMap.TechnicalTest.DataAccess.Interfaces;
using CMap.TechnicalTest.Models;

namespace CMap.TechnicalTest.DataAccess.Memory;

public class MemoryProjectRepository : IProjectRepository
{
    private readonly List<Project> _projects =
    [
        new Project { Id = Guid.NewGuid(), Name = "Project 1" },
        new Project { Id = Guid.NewGuid(), Name = "Project 2" }
    ];
    
    public Project? GetProjectById(Guid id)
    {
        if(id == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(id), id, $"{nameof(id)} cannot be empty");
        
        return _projects.FirstOrDefault(x => x.Id == id);
    }

    public IEnumerable<Project> GetProjects() => _projects;
}