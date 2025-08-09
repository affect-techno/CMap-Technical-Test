using CMap.TechnicalTest.DataAccess.Interfaces;
using CMap.TechnicalTest.Models;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace CMap.TechnicalTest.BusinessLogic.UnitTests;

[TestClass]
public class ProjectLogicTests
{
    private IProjectRepository _projectRepository;
    private ProjectLogic _projectLogic;

    [TestInitialize]
    public void Setup()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _projectLogic = new ProjectLogic(_projectRepository);
    }

    // Constructor guard clause
    [TestMethod, ExpectedException(typeof(ArgumentNullException))]
    public void Ctor_NullRepository_Throws()
    {
        _ = new ProjectLogic(null!);
    }

    // Success scenarios
    [TestMethod]
    public void GetProjects_EmptyRepositoryResult_ReturnsEmptyArray()
    {
        // Arrange
        _projectRepository.GetProjects().Returns(Enumerable.Empty<Project>());

        // Act
        var result = _projectLogic.GetProjects();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Length);
        _projectRepository.Received(1).GetProjects();
    }

    [TestMethod]
    public void GetProjects_RepositoryReturnsItems_ReturnsArrayPreservingOrder()
    {
        // Arrange
        var p1 = new Project();
        var p2 = new Project();
        _projectRepository.GetProjects().Returns(new[] { p1, p2 });

        // Act
        var result = _projectLogic.GetProjects();

        // Assert
        Assert.AreEqual(2, result.Length);
        Assert.AreSame(p1, result[0]);
        Assert.AreSame(p2, result[1]);
        _projectRepository.Received(1).GetProjects();
    }

    // Edge/failure scenarios
    [TestMethod]
    public void GetProjects_RepositoryReturnsNull_ReturnsEmptyArray()
    {
        // Arrange
        _projectRepository.GetProjects().ReturnsNull();

        // Act
        Project[] result = _projectLogic.GetProjects();
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Length);
        _projectRepository.Received(1).GetProjects();
    }

    [TestMethod]
    public void GetProjects_RepositoryThrows_PropagatesException()
    {
        // Arrange
        _projectRepository.GetProjects().Returns(_ => throw new InvalidOperationException("repo failed"));

        // Act + Assert
        Assert.ThrowsException<InvalidOperationException>(() => _projectLogic.GetProjects());
        _projectRepository.Received(1).GetProjects();
    }
}