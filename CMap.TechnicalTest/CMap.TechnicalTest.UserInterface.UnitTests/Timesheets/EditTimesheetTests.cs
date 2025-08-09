using CMap.TechnicalTest.BusinessLogic.Exceptions;
using CMap.TechnicalTest.BusinessLogic.Interfaces;
using CMap.TechnicalTest.Models;
using CMap.TechnicalTest.UserInterface.Context;
using CMap.TechnicalTest.UserInterface.Pages.Timesheets;
using CMap.TechnicalTest.UserInterface.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace CMap.TechnicalTest.UserInterface.UnitTests;

[TestClass]
public class EditTimesheetTests
{
    private IUserLogic _userLogic;
    private IProjectLogic _projectLogic;
    private ITimesheetLogic _timesheetLogic;

    private TimesheetContext _context;
    private EditTimesheet _page;

    [TestInitialize]
    public void Setup()
    {
        _userLogic = Substitute.For<IUserLogic>();
        _projectLogic = Substitute.For<IProjectLogic>();
        _timesheetLogic = Substitute.For<ITimesheetLogic>();

        // Provide default non-null results for context population
        _userLogic.GetUsers().Returns(Array.Empty<User>());
        _projectLogic.GetProjects().Returns(Array.Empty<Project>());

        _context = new TimesheetContext(_userLogic, _projectLogic, _timesheetLogic);
        _page = new EditTimesheet(_context);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Ctor_NullContext_Throws()
    {
        _ = new EditTimesheet(null!);
    }

    [TestMethod]
    public void OnGet_TimesheetExists_ReturnsPage_AndSetsModel_AndLoadsContext()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entry = new TimesheetEntry { Id = id };
        _timesheetLogic.GetTimesheetById(id).Returns(entry);

        // Act
        var result = _page.OnGet(id);

        // Assert
        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreSame(entry, _page.TimesheetEntry);

        _timesheetLogic.Received(1).GetTimesheetById(id);
        _userLogic.Received(1).GetUsers();
        _projectLogic.Received(1).GetProjects();
    }

    [TestMethod]
    public void OnGet_TimesheetMissing_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _timesheetLogic.GetTimesheetById(id).ReturnsNull();

        // Act
        var result = _page.OnGet(id);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        _timesheetLogic.Received(1).GetTimesheetById(id);
    }

    [TestMethod]
    public void OnPost_ModelStateInvalid_ReturnsPage_AndDoesNotEdit()
    {
        // Arrange
        _page.ModelState.AddModelError("x", "invalid");
        _page.TimesheetEntry = new TimesheetEntry();

        // Act
        var result = _page.OnPost(Guid.NewGuid());

        // Assert
        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsNotInstanceOfType(result, typeof(RedirectToPageResult));
        _timesheetLogic.DidNotReceive().EditTimesheetEntry(Arg.Any<TimesheetEntry>());
    }
    
    [TestMethod]
    public void OnPost_TimesheetEntryNull_ReturnsPage_DoesNotEdit()
    {
        // Arrange
        _page.TimesheetEntry = null;

        // Act
        var result = _page.OnPost(Guid.NewGuid());

        // Assert
        Assert.IsInstanceOfType(result, typeof(PageResult));
        _timesheetLogic.DidNotReceive().EditTimesheetEntry(Arg.Any<TimesheetEntry>());
    }

    [TestMethod]
    public void OnPost_UserIdEmpty_ReturnsPage_DoesNotEdit()
    {
        // Arrange
        _page.TimesheetEntry = new TimesheetEntry
        {
            UserId = Guid.Empty,
            ProjectId = Guid.NewGuid(),
            Date = DateTime.Today,
            Hours = 1m,
            Description = "Test"
        };

        // Act
        var result = _page.OnPost(Guid.NewGuid());

        // Assert
        Assert.IsInstanceOfType(result, typeof(PageResult));
        _timesheetLogic.DidNotReceive().EditTimesheetEntry(Arg.Any<TimesheetEntry>());
    }

    [TestMethod]
    public void OnPost_ProjectIdEmpty_ReturnsPage_DoesNotEdit()
    {
        // Arrange
        _page.TimesheetEntry = new TimesheetEntry
        {
            UserId = Guid.NewGuid(),
            ProjectId = Guid.Empty,
            Date = DateTime.Today,
            Hours = 1m,
            Description = "Test"
        };

        // Act
        var result = _page.OnPost(Guid.NewGuid());

        // Assert
        Assert.IsInstanceOfType(result, typeof(PageResult));
        _timesheetLogic.DidNotReceive().EditTimesheetEntry(Arg.Any<TimesheetEntry>());
    }
    
    [TestMethod]
    public void OnPost_Success_Edits_TruncatesDate_RedirectsToUsersTimesheets()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dateWithTime = new DateTime(2025, 1, 8, 15, 45, 12); // 3:45pm
        var expectedDate = dateWithTime.Date;

        var entry = new TimesheetEntry
        {
            Id = Guid.NewGuid(), // will be overwritten with id
            UserId = userId,
            ProjectId = Guid.NewGuid(),
            Date = dateWithTime,
            Hours = 5,
            Description = "Edit"
        };
        _page.TimesheetEntry = entry;

        // Act
        var result = _page.OnPost(id);

        // Assert: Edit called with date truncated and id overwritten
        _timesheetLogic.Received(1).EditTimesheetEntry(Arg.Is<TimesheetEntry>(e =>
            e.Id == id && e.UserId == userId && e.Date == expectedDate));

        // Assert: redirect to UsersTimesheets with correct route values
        var redirect = result as RedirectToPageResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("./UsersTimesheets", redirect!.PageName);
        Assert.AreEqual(userId, redirect.RouteValues!["userId"]);
        Assert.AreEqual(expectedDate.ToParameterString(), redirect.RouteValues!["date"]);
    }

    [TestMethod]
    public void OnPost_EditValidationFails_AddsModelErrors_AndRedirectsToUsersTimesheets()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var date = new DateTime(2025, 1, 9, 9, 30, 0);

        var entry = new TimesheetEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProjectId = Guid.NewGuid(),
            Date = date,
            Hours = 10
        };
        _page.TimesheetEntry = entry;

        var details = new List<BadRequestDetail>
        {
            new BadRequestDetail("User invalid", "UserId"),
            new BadRequestDetail("Hours invalid", "Hours")
        };
        var badRequest = new BadRequestException("Invalid", details);

        _timesheetLogic.EditTimesheetEntry(Arg.Any<TimesheetEntry>())
            .Returns(_ => throw badRequest);

        // Act
        var result = _page.OnPost(id);

        // Assert: model state contains validation errors with expected keys
        Assert.IsTrue(_page.ModelState.TryGetValue("TimesheetEntry.UserId", out var userEntry));
        Assert.IsTrue(userEntry!.Errors.Any(e => e.ErrorMessage == "User invalid"));

        Assert.IsTrue(_page.ModelState.TryGetValue("TimesheetEntry.Hours", out var hoursEntry));
        Assert.IsTrue(hoursEntry!.Errors.Any(e => e.ErrorMessage == "Hours invalid"));

        // Does not redirect
        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsNotInstanceOfType(result, typeof(RedirectToPageResult));

        // Edit was attempted once
        _timesheetLogic.Received(1).EditTimesheetEntry(Arg.Any<TimesheetEntry>());
    }
}