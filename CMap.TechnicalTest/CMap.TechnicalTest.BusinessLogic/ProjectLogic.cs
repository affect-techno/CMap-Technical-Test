using CMap.TechnicalTest.BusinessLogic.Interfaces;
using CMap.TechnicalTest.DataAccess.Interfaces;
using CMap.TechnicalTest.Models;

namespace CMap.TechnicalTest.BusinessLogic;

public class ProjectLogic(IProjectRepository projectRepository) : IProjectLogic
{
    private readonly IProjectRepository _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));

    public Project[] GetProjects() => _projectRepository.GetProjects()?.ToArray() ?? [];
}