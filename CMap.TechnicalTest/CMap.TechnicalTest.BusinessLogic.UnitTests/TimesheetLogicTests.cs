using CMap.TechnicalTest.BusinessLogic.Exceptions;
using CMap.TechnicalTest.BusinessLogic.Validation;
using CMap.TechnicalTest.DataAccess.Interfaces;
using CMap.TechnicalTest.Models;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace CMap.TechnicalTest.BusinessLogic.UnitTests;

[TestClass]
public class TimesheetLogicTests
{
    private ITimesheetEntryValidation _timesheetEntryValidation;
    private ITimesheetEntryRepository _timesheetEntryRepository;
    private IProjectRepository _projectRepository;
    private ITimesheetEntryForUserValidation _timesheetEntryForUserValidation;
    private IUserRepository _userRepository;

    private TimesheetLogic _timesheetLogic;

    [TestInitialize]
    public void Setup()
    {
        _timesheetEntryValidation = Substitute.For<ITimesheetEntryValidation>();
        _timesheetEntryRepository = Substitute.For<ITimesheetEntryRepository>();
        _projectRepository = Substitute.For<IProjectRepository>();
        _timesheetEntryForUserValidation = Substitute.For<ITimesheetEntryForUserValidation>();
        _userRepository = Substitute.For<IUserRepository>();

        _timesheetLogic = new TimesheetLogic(
            _timesheetEntryValidation,
            _timesheetEntryRepository,
            _projectRepository,
            _timesheetEntryForUserValidation,
            _userRepository);
    }

    // Constructor guard clauses
    [TestMethod, ExpectedException(typeof(ArgumentNullException))]
    public void Ctor_NullTimesheetEntryValidation_Throws() =>
        _ = new TimesheetLogic(null!, _timesheetEntryRepository, _projectRepository, _timesheetEntryForUserValidation, _userRepository);

    [TestMethod, ExpectedException(typeof(ArgumentNullException))]
    public void Ctor_NullTimesheetEntryRepository_Throws() =>
        _ = new TimesheetLogic(_timesheetEntryValidation, null!, _projectRepository, _timesheetEntryForUserValidation, _userRepository);

    [TestMethod, ExpectedException(typeof(ArgumentNullException))]
    public void Ctor_NullProjectRepository_Throws() =>
        _ = new TimesheetLogic(_timesheetEntryValidation, _timesheetEntryRepository, null!, _timesheetEntryForUserValidation, _userRepository);

    [TestMethod, ExpectedException(typeof(ArgumentNullException))]
    public void Ctor_NullTimesheetEntryForUserValidation_Throws() =>
        _ = new TimesheetLogic(_timesheetEntryValidation, _timesheetEntryRepository, _projectRepository, null!, _userRepository);

    [TestMethod, ExpectedException(typeof(ArgumentNullException))]
    public void Ctor_NullUserRepository_Throws() =>
        _ = new TimesheetLogic(_timesheetEntryValidation, _timesheetEntryRepository, _projectRepository, _timesheetEntryForUserValidation, null!);

    // GetTimesheetById
    [TestMethod]
    public void GetTimesheetById_RepositoryReturnsNull_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _timesheetEntryRepository.GetTimesheetEntryById(id).ReturnsNull();

        var result = _timesheetLogic.GetTimesheetById(id);

        Assert.IsNull(result);
        _timesheetEntryRepository.Received(1).GetTimesheetEntryById(id);
    }

    [TestMethod]
    public void GetTimesheetById_RepositoryReturnsEntry_ReturnsEntry()
    {
        var id = Guid.NewGuid();
        var entry = new TimesheetEntry { Id = id };
        _timesheetEntryRepository.GetTimesheetEntryById(id).Returns(entry);

        var result = _timesheetLogic.GetTimesheetById(id);

        Assert.AreSame(entry, result);
        _timesheetEntryRepository.Received(1).GetTimesheetEntryById(id);
    }

    // AddNewTimesheetEntry
    [TestMethod, ExpectedException(typeof(ArgumentNullException))]
    public void AddNewTimesheetEntry_Null_ThrowsArgumentNullException() =>
        _timesheetLogic.AddNewTimesheetEntry(null!);

    [TestMethod]
    public void AddNewTimesheetEntry_ValidationFails_ThrowsBadRequestException_AndDoesNotCreate()
    {
        var entry = CreateEntry();
        var validationEx = new BadRequestException("invalid", new List<BadRequestDetail> { new BadRequestDetail("d", "t") });
        _timesheetEntryValidation.ValidateNew(entry).Returns(validationEx);

        var ex = Assert.ThrowsException<BadRequestException>(() => _timesheetLogic.AddNewTimesheetEntry(entry));
        Assert.AreEqual("invalid", ex.Message);
        _timesheetEntryRepository.DidNotReceive().CreateTimesheetEntry(Arg.Any<TimesheetEntry>());
    }

    [TestMethod]
    public void AddNewTimesheetEntry_ValidationPasses_CreatesAndReturnsEntry()
    {
        var entry = CreateEntry();
        var created = new TimesheetEntry { Id = entry.Id, UserId = entry.UserId, ProjectId = entry.ProjectId, Date = entry.Date, Hours = entry.Hours, Description = entry.Description };
        _timesheetEntryValidation.ValidateNew(entry).Returns((BadRequestException?)null);
        _timesheetEntryRepository.CreateTimesheetEntry(entry).Returns(created);

        var result = _timesheetLogic.AddNewTimesheetEntry(entry);

        Assert.AreSame(created, result);
        _timesheetEntryRepository.Received(1).CreateTimesheetEntry(entry);
        _timesheetEntryValidation.Received(1).ValidateNew(entry);
    }

    // EditTimesheetEntry
    [TestMethod, ExpectedException(typeof(ArgumentNullException))]
    public void EditTimesheetEntry_Null_ThrowsArgumentNullException() =>
        _timesheetLogic.EditTimesheetEntry(null!);

    [TestMethod]
    public void EditTimesheetEntry_ValidationFails_ThrowsBadRequestException_AndDoesNotUpdate()
    {
        var entry = CreateEntry();
        var validationEx = new BadRequestException("invalid", new List<BadRequestDetail> { new BadRequestDetail("d", "t") });
        _timesheetEntryValidation.ValidateExisting(entry).Returns(validationEx);

        var ex = Assert.ThrowsException<BadRequestException>(() => _timesheetLogic.EditTimesheetEntry(entry));
        Assert.AreEqual("invalid", ex.Message);
        _timesheetEntryRepository.DidNotReceive().UpdateTimesheetEntry(Arg.Any<TimesheetEntry>());
    }

    [TestMethod]
    public void EditTimesheetEntry_ValidationPasses_UpdatesAndReturnsEntry()
    {
        var entry = CreateEntry();
        var updated = new TimesheetEntry { Id = entry.Id, UserId = entry.UserId, ProjectId = entry.ProjectId, Date = entry.Date, Hours = entry.Hours + 1, Description = entry.Description };
        _timesheetEntryValidation.ValidateExisting(entry).Returns((BadRequestException?)null);
        _timesheetEntryRepository.UpdateTimesheetEntry(entry).Returns(updated);

        var result = _timesheetLogic.EditTimesheetEntry(entry);

        Assert.AreSame(updated, result);
        _timesheetEntryRepository.Received(1).UpdateTimesheetEntry(entry);
        _timesheetEntryValidation.Received(1).ValidateExisting(entry);
    }

    // DeleteTimesheetEntry
    [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void DeleteTimesheetEntry_DefaultId_Throws()
    {
        _timesheetLogic.DeleteTimesheetEntry(Guid.Empty);
    }

    [TestMethod]
    public void DeleteTimesheetEntry_ValidId_Deletes()
    {
        var id = Guid.NewGuid();

        _timesheetLogic.DeleteTimesheetEntry(id);

        _timesheetEntryRepository.Received(1).DeleteTimesheetEntry(id);
    }

    // GetTimesheetEntriesForUser - guards
    [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetTimesheetEntriesForUser_DefaultUserId_Throws()
    {
        _timesheetLogic.GetTimesheetEntriesForUser(Guid.Empty, new DateTime(2025, 1, 6));
    }

    [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetTimesheetEntriesForUser_MinWeekCommencing_Throws()
    {
        _timesheetLogic.GetTimesheetEntriesForUser(Guid.NewGuid(), DateTime.MinValue);
    }

    [TestMethod]
    public void GetTimesheetEntriesForUser_ValidationFails_ThrowsBadRequestException()
    {
        var userId = Guid.NewGuid();
        var week = new DateTime(2025, 1, 6);
        var validationEx = new BadRequestException("invalid", new List<BadRequestDetail> { new BadRequestDetail("d", "t") });
        _timesheetEntryForUserValidation.Validate(userId).Returns(validationEx);

        Assert.ThrowsException<BadRequestException>(() => _timesheetLogic.GetTimesheetEntriesForUser(userId, week));
        _timesheetEntryRepository.DidNotReceive().GetTimesheetEntriesByUserId(Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<DateTime>());
    }

    [TestMethod]
    public void GetTimesheetEntriesForUser_NoEntries_ReturnsEmptyCollections_AndQueriesExpectedWindow()
    {
        var userId = Guid.NewGuid();
        var week = new DateTime(2025, 1, 6); // Monday
        _timesheetEntryForUserValidation.Validate(userId).Returns((BadRequestException?)null);
        _timesheetEntryRepository.GetTimesheetEntriesByUserId(userId, week, week.AddDays(7)).Returns(Enumerable.Empty<TimesheetEntry>());

        var result = _timesheetLogic.GetTimesheetEntriesForUser(userId, week);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Timesheets.Length);
        Assert.AreEqual(0, result.ProjectHours.Length);
        _timesheetEntryRepository.Received(1).GetTimesheetEntriesByUserId(userId, week, week.AddDays(7));
    }

    [TestMethod]
    public void GetTimesheetEntriesForUser_Success_ReturnsMappedTimesheets_AndProjectTotals()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var week = new DateTime(2025, 1, 6);
        var projectA = Guid.NewGuid();
        var projectB = Guid.NewGuid();

        var e1 = CreateEntry(userId: userId, projectId: projectA, date: week.AddDays(1), hours: 4, description: "e1");
        var e2 = CreateEntry(userId: userId, projectId: projectA, date: week.AddDays(2), hours: 3, description: "e2");
        var e3 = CreateEntry(userId: userId, projectId: projectB, date: week.AddDays(3), hours: 2, description: "e3");

        _timesheetEntryForUserValidation.Validate(userId).Returns((BadRequestException?)null);
        _timesheetEntryRepository.GetTimesheetEntriesByUserId(userId, week, week.AddDays(7)).Returns(new[] { e1, e2, e3 });

        _userRepository.GetUserById(userId).Returns(new User { Id = userId, Name = "Alice" });
        _projectRepository.GetProjectById(projectA).Returns(new Project { Id = projectA, Name = "Project A" });
        _projectRepository.GetProjectById(projectB).Returns(new Project { Id = projectB, Name = "Project B" });

        // Act
        var result = _timesheetLogic.GetTimesheetEntriesForUser(userId, week);

        // Assert
        Assert.IsNotNull(result);

        // Timesheets mapped
        Assert.AreEqual(3, result.Timesheets.Length);
        Assert.IsTrue(result.Timesheets.Any(t => t.Id == e1.Id && t.UserName == "Alice" && t.ProjectName == "Project A" && t.Hours == 4 && t.Description == "e1"));
        Assert.IsTrue(result.Timesheets.Any(t => t.Id == e2.Id && t.ProjectName == "Project A" && t.Hours == 3));
        Assert.IsTrue(result.Timesheets.Any(t => t.Id == e3.Id && t.ProjectName == "Project B" && t.Hours == 2));

        // Project totals aggregated
        Assert.AreEqual(2, result.ProjectHours.Length);
        var totalA = result.ProjectHours.Single(pt => pt.ProjectName == "Project A");
        var totalB = result.ProjectHours.Single(pt => pt.ProjectName == "Project B");
        Assert.AreEqual(7, totalA.TotalHours);
        Assert.AreEqual(2, totalB.TotalHours);

        _timesheetEntryRepository.Received(1).GetTimesheetEntriesByUserId(userId, week, week.AddDays(7));
    }

    [TestMethod]
    public void GetTimesheetEntriesForUser_UserMissingForAnEntry_ThrowsKeyNotFound()
    {
        var userId = Guid.NewGuid();
        var week = new DateTime(2025, 1, 6);
        var projectId = Guid.NewGuid();
        var e1 = CreateEntry(userId: userId, projectId: projectId, date: week.AddDays(1));

        _timesheetEntryForUserValidation.Validate(userId).Returns((BadRequestException?)null);
        _timesheetEntryRepository.GetTimesheetEntriesByUserId(userId, week, week.AddDays(7)).Returns(new[] { e1 });

        _userRepository.GetUserById(userId).ReturnsNull();
        _projectRepository.GetProjectById(projectId).Returns(new Project { Id = projectId, Name = "Project X" });

        var ex = Assert.ThrowsException<KeyNotFoundException>(() => _timesheetLogic.GetTimesheetEntriesForUser(userId, week));
        Assert.AreEqual($"No user with Id: {userId}", ex.Message);
    }

    [TestMethod]
    public void GetTimesheetEntriesForUser_ProjectMissingWhenMappingTimesheets_ThrowsKeyNotFound()
    {
        var userId = Guid.NewGuid();
        var week = new DateTime(2025, 1, 6);
        var projectId = Guid.NewGuid();
        var e1 = CreateEntry(userId: userId, projectId: projectId, date: week.AddDays(1));

        _timesheetEntryForUserValidation.Validate(userId).Returns((BadRequestException?)null);
        _timesheetEntryRepository.GetTimesheetEntriesByUserId(userId, week, week.AddDays(7)).Returns(new[] { e1 });

        _userRepository.GetUserById(userId).Returns(new User { Id = userId, Name = "Alice" });
        _projectRepository.GetProjectById(projectId).ReturnsNull();

        var ex = Assert.ThrowsException<KeyNotFoundException>(() => _timesheetLogic.GetTimesheetEntriesForUser(userId, week));
        Assert.AreEqual($"No project with Id: {projectId}", ex.Message);
    }

    [TestMethod]
    public void GetTimesheetEntriesForUser_ProjectMissingWhenComputingTotals_ThrowsKeyNotFound()
    {
        var userId = Guid.NewGuid();
        var week = new DateTime(2025, 1, 6);
        var projectId = Guid.NewGuid();
        var e1 = CreateEntry(userId: userId, projectId: projectId, date: week.AddDays(1));
        var e2 = CreateEntry(userId: userId, projectId: projectId, date: week.AddDays(2));

        _timesheetEntryForUserValidation.Validate(userId).Returns((BadRequestException?)null);
        _timesheetEntryRepository.GetTimesheetEntriesByUserId(userId, week, week.AddDays(7)).Returns(new[] { e1, e2 });

        _userRepository.GetUserById(userId).Returns(new User { Id = userId, Name = "Alice" });

        // For mapping timesheets (2 entries): return a project twice, then for totals (group key) return null
        _projectRepository.GetProjectById(projectId).Returns(
            new Project { Id = projectId, Name = "Project X" },
            new Project { Id = projectId, Name = "Project X" },
            (Project)null!
        );

        var ex = Assert.ThrowsException<KeyNotFoundException>(() => _timesheetLogic.GetTimesheetEntriesForUser(userId, week));
        Assert.AreEqual($"No Project with ProjectId: {projectId}", ex.Message);
    }

    // Helpers
    private static TimesheetEntry CreateEntry(Guid? id = null, Guid? userId = null, Guid? projectId = null, DateTime? date = null, int hours = 8, string description = "desc")
    {
        return new TimesheetEntry
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            ProjectId = projectId ?? Guid.NewGuid(),
            Date = date ?? new DateTime(2025, 1, 7, 10, 0, 0),
            Hours = hours,
            Description = description
        };
    }
}