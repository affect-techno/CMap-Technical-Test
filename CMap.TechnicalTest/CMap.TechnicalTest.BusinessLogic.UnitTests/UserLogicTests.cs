using CMap.TechnicalTest.DataAccess.Interfaces;
using CMap.TechnicalTest.Models;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace CMap.TechnicalTest.BusinessLogic.UnitTests;

[TestClass]
public class UserLogicTests
{
    private IUserRepository _userRepository;
    private UserLogic _userLogic;

    [TestInitialize]
    public void Setup()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _userLogic = new UserLogic(_userRepository);
    }

    // Constructor guard clause
    [TestMethod, ExpectedException(typeof(ArgumentNullException))]
    public void Ctor_NullRepository_Throws()
    {
        _ = new UserLogic(null!);
    }

    // GetUsers - success scenarios
    [TestMethod]
    public void GetUsers_EmptyRepositoryResult_ReturnsEmptyArray()
    {
        // Arrange
        _userRepository.GetUsers().Returns(Enumerable.Empty<User>());

        // Act
        var result = _userLogic.GetUsers();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Length);
        _userRepository.Received(1).GetUsers();
    }

    [TestMethod]
    public void GetUsers_RepositoryReturnsItems_ReturnsArrayPreservingOrder()
    {
        // Arrange
        var u1 = new User { Id = Guid.NewGuid(), Name = "User 1" };
        var u2 = new User { Id = Guid.NewGuid(), Name = "User 2" };
        _userRepository.GetUsers().Returns(new[] { u1, u2 });

        // Act
        var result = _userLogic.GetUsers();

        // Assert
        Assert.AreEqual(2, result.Length);
        Assert.AreSame(u1, result[0]);
        Assert.AreSame(u2, result[1]);
        _userRepository.Received(1).GetUsers();
    }

    // GetUsers - edge/failure scenarios
    [TestMethod]
    public void GetUsers_RepositoryReturnsNull_ReturnsEmptyArray()
    {
        // Arrange
        _userRepository.GetUsers().ReturnsNull();

        // Act
        User[] result = _userLogic.GetUsers();
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Length);
        _userRepository.Received(1).GetUsers();
    }

    [TestMethod]
    public void GetUsers_RepositoryThrows_PropagatesException()
    {
        // Arrange
        _userRepository.GetUsers().Returns(_ => throw new InvalidOperationException("repo failed"));

        // Act + Assert
        Assert.ThrowsException<InvalidOperationException>(() => _userLogic.GetUsers());
        _userRepository.Received(1).GetUsers();
    }

    // GetUserById - success scenarios
    [TestMethod]
    public void GetUserById_UserExists_ReturnsUser()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = new User { Id = id, Name = "Alice" };
        _userRepository.GetUserById(id).Returns(user);

        // Act
        var result = _userLogic.GetUserById(id);

        // Assert
        Assert.AreSame(user, result);
        _userRepository.Received(1).GetUserById(id);
    }

    [TestMethod]
    public void GetUserById_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _userRepository.GetUserById(id).ReturnsNull();

        // Act
        var result = _userLogic.GetUserById(id);

        // Assert
        Assert.IsNull(result);
        _userRepository.Received(1).GetUserById(id);
    }

    // GetUserById - edge/failure scenarios
    [TestMethod]
    public void GetUserById_DefaultGuid_PassesThroughToRepository()
    {
        // Arrange
        var id = Guid.Empty;
        _userRepository.GetUserById(id).ReturnsNull();

        // Act
        var result = _userLogic.GetUserById(id);

        // Assert
        Assert.IsNull(result);
        _userRepository.Received(1).GetUserById(id);
    }

    [TestMethod]
    public void GetUserById_RepositoryThrows_PropagatesException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _userRepository.GetUserById(id).Returns(_ => throw new InvalidOperationException("repo failed"));

        // Act + Assert
        Assert.ThrowsException<InvalidOperationException>(() => _userLogic.GetUserById(id));
        _userRepository.Received(1).GetUserById(id);
    }
}