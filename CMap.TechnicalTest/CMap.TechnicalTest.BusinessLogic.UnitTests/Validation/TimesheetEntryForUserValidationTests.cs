using CMap.TechnicalTest.BusinessLogic.Exceptions;
using CMap.TechnicalTest.BusinessLogic.Validation;
using CMap.TechnicalTest.DataAccess.Interfaces;
using CMap.TechnicalTest.Models;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace CMap.TechnicalTest.BusinessLogic.UnitTests.Validation;

[TestClass]
public class TimesheetEntryForUserValidationTests
{
    private IUserRepository _userRepository;
    private TimesheetEntryForUserValidation _validation;

    [TestInitialize]
    public void TestInitialize()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _validation = new TimesheetEntryForUserValidation(_userRepository);
    }
    
    [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Validate_UserIdIsDefault_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        Guid userId = Guid.Empty;
        
        // Act
        _validation.Validate(userId);
        
        // Assert
        // See attribute
    }

    [TestMethod]
    public void Validate_UserDoesNotExist_ReturnsBadRequestException()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        _userRepository.GetUserById(userId).ReturnsNull();
        
        // Act
        BadRequestException? result = _validation.Validate(userId);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Details.Count());
        Assert.AreEqual("There is no user with the specified id", result.Details.First().Description);
        Assert.AreEqual("userId", result.Details.First().Target);
    }

    [TestMethod]
    public void Validate_UserExists_ReturnsNull()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        _userRepository.GetUserById(userId).Returns(new User());
        
        // Act
        BadRequestException? result = _validation.Validate(userId);
        
        // Assert
        Assert.IsNull(result);
    }
}