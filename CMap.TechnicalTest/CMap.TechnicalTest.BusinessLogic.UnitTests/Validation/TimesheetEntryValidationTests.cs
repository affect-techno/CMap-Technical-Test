using CMap.TechnicalTest.BusinessLogic.Validation;
using CMap.TechnicalTest.DataAccess.Interfaces;
using CMap.TechnicalTest.Models;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace CMap.TechnicalTest.BusinessLogic.UnitTests.Validation;

[TestClass]
public class TimesheetEntryValidationTests
{
    private ITimesheetEntryRepository _timesheetEntryRepository;
    private IUserRepository _userRepository;
    private IProjectRepository _projectRepository;
    
    private TimesheetEntryValidation _validation;

    [TestInitialize]
    public void TestInitialize()
    {
        _timesheetEntryRepository = Substitute.For<ITimesheetEntryRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _projectRepository = Substitute.For<IProjectRepository>();

        // By default, no duplicate entries
        _timesheetEntryRepository
            .GetTimesheetEntriesByUserIdAndProjectId(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(Enumerable.Empty<TimesheetEntry>());

        _validation = new TimesheetEntryValidation(_timesheetEntryRepository, _userRepository, _projectRepository);
    }

    [TestMethod, ExpectedException(typeof(ArgumentNullException))]
    public void ValidateNew_NullTimesheetEntry_ThrowsArgumentNullException()
    {
        _validation.ValidateNew(null!);
    }

    [TestMethod, ExpectedException(typeof(ArgumentNullException))]
    public void ValidateExisting_NullTimesheetEntry_ThrowsArgumentNullException()
    {
        _validation.ValidateExisting(null!);
    }

    // Success cases
    [TestMethod]
    public void ValidateNew_AllValid_NoDuplicate_ReturnsNull_And_QueriesExpectedDayWindow()
    {
        // Arrange
        var entry = CreateValidEntry();
        SetupUserExists(entry.UserId);
        SetupProjectExists(entry.ProjectId);

        var expectedFrom = entry.Date.Date;
        var expectedTo = entry.Date.Date.AddDays(1).AddTicks(-1);

        // Act
        var result = _validation.ValidateNew(entry);

        // Assert
        Assert.IsNull(result);
        _timesheetEntryRepository.Received(1)
            .GetTimesheetEntriesByUserIdAndProjectId(entry.UserId, entry.ProjectId, expectedFrom, expectedTo);
    }

    [TestMethod]
    public void ValidateExisting_AllValid_EntryExists_NoDuplicate_ReturnsNull()
    {
        // Arrange
        var entry = CreateValidEntry();
        SetupUserExists(entry.UserId);
        SetupProjectExists(entry.ProjectId);
        _timesheetEntryRepository.GetTimesheetEntryById(entry.Id).Returns(entry);

        // Act
        var result = _validation.ValidateExisting(entry);

        // Assert
        Assert.IsNull(result);
        _timesheetEntryRepository.Received(1).GetTimesheetEntryById(entry.Id);
    }

    [TestMethod]
    public void ValidateNew_HoursExactly24_IsValid()
    {
        // Arrange
        var entry = CreateValidEntry(hours: 24);
        SetupUserExists(entry.UserId);
        SetupProjectExists(entry.ProjectId);

        // Act
        var result = _validation.ValidateNew(entry);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void ValidateNew_UserDoesNotExist_ReturnsBadRequestWithUserDetail()
    {
        // Arrange
        var entry = CreateValidEntry();
        // User is missing
        SetupProjectExists(entry.ProjectId);

        // Act
        var result = _validation.ValidateNew(entry);

        // Assert
        Assert.IsNotNull(result);
        var details = result!.Details.ToList();
        Assert.AreEqual(1, details.Count);
        Assert.AreEqual($"No user found for ID {entry.Id}", details[0].Description); // note: message uses entry.Id
        Assert.AreEqual(nameof(entry.UserId), details[0].Target);
    }

    [TestMethod]
    public void ValidateNew_ProjectDoesNotExist_ReturnsBadRequestWithProjectDetail()
    {
        // Arrange
        var entry = CreateValidEntry();
        SetupUserExists(entry.UserId);
        // Project is missing

        // Act
        var result = _validation.ValidateNew(entry);

        // Assert
        Assert.IsNotNull(result);
        var details = result!.Details.ToList();
        Assert.AreEqual(1, details.Count);
        Assert.AreEqual($"No project found for ID {entry.ProjectId}", details[0].Description);
        Assert.AreEqual(nameof(entry.ProjectId), details[0].Target);
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(25)]
    public void ValidateNew_InvalidHours_ReturnsBadRequestWithHoursDetail(int invalidHours)
    {
        // Arrange
        var entry = CreateValidEntry(hours: invalidHours);
        SetupUserExists(entry.UserId);
        SetupProjectExists(entry.ProjectId);

        // Act
        var result = _validation.ValidateNew(entry);

        // Assert
        Assert.IsNotNull(result);
        var detail = result!.Details.Single();
        Assert.AreEqual("The hours must be between 0 and 24", detail.Description);
        Assert.AreEqual(nameof(entry.Hours), detail.Target);
    }

    [TestMethod]
    public void ValidateNew_DateIsDefault_ReturnsBadRequestWithDateDetail()
    {
        // Arrange
        var entry = CreateValidEntry(date: default(DateTime));
        SetupUserExists(entry.UserId);
        SetupProjectExists(entry.ProjectId);

        // Act
        var result = _validation.ValidateNew(entry);

        // Assert
        Assert.IsNotNull(result);
        var detail = result!.Details.Single();
        Assert.AreEqual("A date for this entry is required", detail.Description);
        Assert.AreEqual(nameof(entry.Date), detail.Target);
    }

    [TestMethod]
    public void ValidateNew_DuplicateEntrySameDay_ReturnsBadRequestWithDuplicateDetail()
    {
        // Arrange
        var entry = CreateValidEntry();
        SetupUserExists(entry.UserId);
        SetupProjectExists(entry.ProjectId);

        var expectedFrom = entry.Date.Date;
        var expectedTo = entry.Date.Date.AddDays(1).AddTicks(-1);

        _timesheetEntryRepository
            .GetTimesheetEntriesByUserIdAndProjectId(entry.UserId, entry.ProjectId, expectedFrom, expectedTo)
            .Returns(new[] { new TimesheetEntry() });

        // Act
        var result = _validation.ValidateNew(entry);

        // Assert
        Assert.IsNotNull(result);
        var detail = result!.Details.Single();
        Assert.AreEqual("There is already a timesheet entry for this date, user and project", detail.Description);
        Assert.AreEqual(nameof(entry.Date), detail.Target);
    }

    [TestMethod]
    public void ValidateNew_MultipleFailures_AggregatesAllErrors()
    {
        // Arrange
        var entry = new TimesheetEntry
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),      // user missing
            ProjectId = Guid.NewGuid(),   // project missing
            Date = default,               // invalid date
            Hours = 0                     // invalid hours
        };
        // No user, no project; duplicates already set to return empty

        // Act
        var result = _validation.ValidateNew(entry);

        // Assert
        Assert.IsNotNull(result);
        var details = result!.Details.ToList();
        // Expect 4 distinct errors: user, project, date, hours
        Assert.AreEqual(4, details.Count);

        Assert.IsTrue(details.Any(d => d.Target == nameof(entry.UserId) && d.Description == $"No user found for ID {entry.Id}"));
        Assert.IsTrue(details.Any(d => d.Target == nameof(entry.ProjectId) && d.Description == $"No project found for ID {entry.ProjectId}"));
        Assert.IsTrue(details.Any(d => d.Target == nameof(entry.Date) && d.Description == "A date for this entry is required"));
        Assert.IsTrue(details.Any(d => d.Target == nameof(entry.Hours) && d.Description == "The hours must be between 0 and 24"));
    }

    [TestMethod]
    public void ValidateExisting_HoursExactly24_IsValid()
    {
        // Arrange
        var entry = CreateValidEntry(hours: 24);
        SetupTimesheetExists(entry.Id);
        SetupUserExists(entry.UserId);
        SetupProjectExists(entry.ProjectId);

        // Act
        var result = _validation.ValidateExisting(entry);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void ValidateExisting_UserDoesNotExist_ReturnsBadRequestWithUserDetail()
    {
        // Arrange
        var entry = CreateValidEntry();
        SetupTimesheetExists(entry.Id);
        // User is missing
        SetupProjectExists(entry.ProjectId);

        // Act
        var result = _validation.ValidateExisting(entry);

        // Assert
        Assert.IsNotNull(result);
        var details = result!.Details.ToList();
        Assert.AreEqual(1, details.Count);
        Assert.AreEqual($"No user found for ID {entry.Id}", details[0].Description); // note: message uses entry.Id
        Assert.AreEqual(nameof(entry.UserId), details[0].Target);
    }

    [TestMethod]
    public void ValidateExisting_ProjectDoesNotExist_ReturnsBadRequestWithProjectDetail()
    {
        // Arrange
        var entry = CreateValidEntry();
        SetupTimesheetExists(entry.Id);
        SetupUserExists(entry.UserId);
        // Project is missing

        // Act
        var result = _validation.ValidateExisting(entry);

        // Assert
        Assert.IsNotNull(result);
        var details = result!.Details.ToList();
        Assert.AreEqual(1, details.Count);
        Assert.AreEqual($"No project found for ID {entry.ProjectId}", details[0].Description);
        Assert.AreEqual(nameof(entry.ProjectId), details[0].Target);
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(25)]
    public void ValidateExisting_InvalidHours_ReturnsBadRequestWithHoursDetail(int invalidHours)
    {
        // Arrange
        var entry = CreateValidEntry(hours: invalidHours);
        SetupTimesheetExists(entry.Id);
        SetupUserExists(entry.UserId);
        SetupProjectExists(entry.ProjectId);

        // Act
        var result = _validation.ValidateExisting(entry);

        // Assert
        Assert.IsNotNull(result);
        var detail = result!.Details.Single();
        Assert.AreEqual("The hours must be between 0 and 24", detail.Description);
        Assert.AreEqual(nameof(entry.Hours), detail.Target);
    }

    [TestMethod]
    public void ValidateExisting_DateIsDefault_ReturnsBadRequestWithDateDetail()
    {
        // Arrange
        var entry = CreateValidEntry(date: default(DateTime));
        SetupTimesheetExists(entry.Id);
        SetupUserExists(entry.UserId);
        SetupProjectExists(entry.ProjectId);

        // Act
        var result = _validation.ValidateExisting(entry);

        // Assert
        Assert.IsNotNull(result);
        var detail = result!.Details.Single();
        Assert.AreEqual("A date for this entry is required", detail.Description);
        Assert.AreEqual(nameof(entry.Date), detail.Target);
    }

    [TestMethod]
    public void ValidateExisting_DuplicateEntrySameDay_ReturnsBadRequestWithDuplicateDetail()
    {
        // Arrange
        var entry = CreateValidEntry();
        SetupTimesheetExists(entry.Id);
        SetupUserExists(entry.UserId);
        SetupProjectExists(entry.ProjectId);

        var expectedFrom = entry.Date.Date;
        var expectedTo = entry.Date.Date.AddDays(1).AddTicks(-1);

        _timesheetEntryRepository
            .GetTimesheetEntriesByUserIdAndProjectId(entry.UserId, entry.ProjectId, expectedFrom, expectedTo)
            .Returns(new[] { new TimesheetEntry() });

        // Act
        var result = _validation.ValidateExisting(entry);

        // Assert
        Assert.IsNotNull(result);
        var detail = result!.Details.Single();
        Assert.AreEqual("There is already a timesheet entry for this date, user and project", detail.Description);
        Assert.AreEqual(nameof(entry.Date), detail.Target);
    }

    [TestMethod]
    public void ValidateExisting_DuplicateEntrySameDayIsSameEntryAsUpdate_ReturnsNull()
    {
        // Arrange
        var entry = CreateValidEntry();
        SetupTimesheetExists(entry.Id);
        SetupUserExists(entry.UserId);
        SetupProjectExists(entry.ProjectId);

        var expectedFrom = entry.Date.Date;
        var expectedTo = entry.Date.Date.AddDays(1).AddTicks(-1);

        _timesheetEntryRepository
            .GetTimesheetEntriesByUserIdAndProjectId(entry.UserId, entry.ProjectId, expectedFrom, expectedTo)
            .Returns(new[] { new TimesheetEntry {Id = entry.Id } });

        // Act
        var result = _validation.ValidateExisting(entry);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void ValidateExisting_MultipleFailures_AggregatesAllErrors()
    {
        // Arrange
        var entry = new TimesheetEntry
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),      // user missing
            ProjectId = Guid.NewGuid(),   // project missing
            Date = default,               // invalid date
            Hours = 0                     // invalid hours
        };
        SetupTimesheetExists(entry.Id);
        // No user, no project; duplicates already set to return empty

        // Act
        var result = _validation.ValidateExisting(entry);

        // Assert
        Assert.IsNotNull(result);
        var details = result!.Details.ToList();
        // Expect 4 distinct errors: user, project, date, hours
        Assert.AreEqual(4, details.Count);

        Assert.IsTrue(details.Any(d => d.Target == nameof(entry.UserId) && d.Description == $"No user found for ID {entry.Id}"));
        Assert.IsTrue(details.Any(d => d.Target == nameof(entry.ProjectId) && d.Description == $"No project found for ID {entry.ProjectId}"));
        Assert.IsTrue(details.Any(d => d.Target == nameof(entry.Date) && d.Description == "A date for this entry is required"));
        Assert.IsTrue(details.Any(d => d.Target == nameof(entry.Hours) && d.Description == "The hours must be between 0 and 24"));
    }

    // ValidateExisting-specific failure
    [TestMethod]
    public void ValidateExisting_EntryDoesNotExist_AddsIdDetail()
    {
        // Arrange
        var entry = CreateValidEntry();
        SetupUserExists(entry.UserId);
        SetupProjectExists(entry.ProjectId);
        _timesheetEntryRepository.GetTimesheetEntryById(entry.Id).ReturnsNull();

        // Act
        var result = _validation.ValidateExisting(entry);

        // Assert
        Assert.IsNotNull(result);
        var detail = result!.Details.Single(d => d.Target == nameof(entry.Id));
        Assert.AreEqual($"No timesheet entry found for ID {entry.Id}", detail.Description);
    }

    private static TimesheetEntry CreateValidEntry(int hours = 8, DateTime? date = null)
    {
        return new TimesheetEntry
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            Date = date ?? new DateTime(2025, 1, 15, 10, 30, 0),
            Hours = hours
        };
    }

    private void SetupTimesheetExists(Guid timesheetId)
    {
        _timesheetEntryRepository.GetTimesheetEntryById(timesheetId).Returns(new TimesheetEntry { Id = timesheetId });
    }

    private void SetupUserExists(Guid userId)
    {
        _userRepository.GetUserById(userId).Returns(new User { Id = userId });
    }

    private void SetupProjectExists(Guid projectId)
    {
        _projectRepository.GetProjectById(projectId).Returns(new Project { Id = projectId });
    }
}