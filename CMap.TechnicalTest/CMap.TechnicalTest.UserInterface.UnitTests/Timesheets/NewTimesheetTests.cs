using CMap.TechnicalTest.BusinessLogic.Interfaces;
using CMap.TechnicalTest.BusinessLogic.Interfaces.Exceptions;
using CMap.TechnicalTest.Models;
using CMap.TechnicalTest.UserInterface.Context;
using CMap.TechnicalTest.UserInterface.Pages.Timesheets;
using CMap.TechnicalTest.UserInterface.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NSubstitute;

namespace CMap.TechnicalTest.UserInterface.UnitTests;

[TestClass]
public class NewTimesheetTests
{
    private IUserLogic _userLogic;
    private IProjectLogic _projectLogic;
    private ITimesheetLogic _timesheetLogic;

    private TimesheetContext _context;
    private NewTimesheet _page;

    [TestInitialize]
    public void Setup()
    {
        _userLogic = Substitute.For<IUserLogic>();
        _projectLogic = Substitute.For<IProjectLogic>();
        _timesheetLogic = Substitute.For<ITimesheetLogic>();

        // Default non-null results for context population
        _userLogic.GetUsers().Returns(Array.Empty<User>());
        _projectLogic.GetProjects().Returns(Array.Empty<Project>());

        _context = new TimesheetContext(_userLogic, _projectLogic, _timesheetLogic);
        _page = new NewTimesheet(_context);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Ctor_NullContext_Throws()
    {
        _ = new NewTimesheet(null!);
    }

    [TestMethod]
    public void OnGet_ReturnsPage_AndLoadsContext()
    {
        // Act
        var result = _page.OnGet();

        // Assert
        Assert.IsInstanceOfType(result, typeof(PageResult));
        _userLogic.Received(1).GetUsers();
        _projectLogic.Received(1).GetProjects();
    }

    [TestMethod]
    public void OnPost_ModelStateInvalid_ReturnsPage_AndDoesNotCreate_ButLoadsContext()
    {
        // Arrange
        _page.ModelState.AddModelError("x", "invalid");
        _page.TimesheetEntry = new TimesheetEntry();

        // Act
        var result = _page.OnPost();

        // Assert
        Assert.IsInstanceOfType(result, typeof(PageResult));
        _timesheetLogic.DidNotReceive().AddNewTimesheetEntry(Arg.Any<TimesheetEntry>());
        _userLogic.Received(1).GetUsers();
        _projectLogic.Received(1).GetProjects();
    }

    [TestMethod]
    public void OnPost_TimesheetEntryNull_ReturnsPage_AndLoadsContext()
    {
        // Arrange
        _page.TimesheetEntry = null;

        // Act
        var result = _page.OnPost();

        // Assert
        Assert.IsInstanceOfType(result, typeof(PageResult));
        _timesheetLogic.DidNotReceive().AddNewTimesheetEntry(Arg.Any<TimesheetEntry>());
        _userLogic.Received(1).GetUsers();
        _projectLogic.Received(1).GetProjects();
    }

    [TestMethod]
    public void OnPost_Success_Creates_TruncatesDate_AndRedirectsToUsersTimesheets()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dateWithTime = new DateTime(2025, 1, 8, 15, 45, 12);
        var expectedDate = dateWithTime.Date;

        var entry = new TimesheetEntry
        {
            UserId = userId,
            ProjectId = Guid.NewGuid(),
            Date = dateWithTime,
            Hours = 5,
            Description = "New"
        };
        _page.TimesheetEntry = entry;

        // Act
        var result = _page.OnPost();

        // Assert: creation called with truncated date
        _timesheetLogic.Received(1).AddNewTimesheetEntry(Arg.Is<TimesheetEntry>(e =>
            e.UserId == userId && e.Date == expectedDate));

        // Assert: redirect to UsersTimesheets with correct route values
        var redirect = result as RedirectToPageResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("./UsersTimesheets", redirect!.PageName);
        Assert.AreEqual(userId, redirect.RouteValues!["userId"]);
        Assert.AreEqual(expectedDate.ToParameterString(), redirect.RouteValues!["date"]);
    }

    [TestMethod]
    public void OnPost_ValidationFails_AddsModelErrors_AndDoesNotRedirectToUsersTimesheets()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = new DateTime(2025, 1, 9, 9, 30, 0);
        var entry = new TimesheetEntry
        {
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

        _timesheetLogic.AddNewTimesheetEntry(Arg.Any<TimesheetEntry>())
            .Returns(_ => throw badRequest);

        // Act
        var result = _page.OnPost();

        // Assert: model state contains validation errors with expected keys
        Assert.IsTrue(_page.ModelState.TryGetValue("TimesheetEntry.UserId", out var userEntry));
        Assert.IsTrue(userEntry!.Errors.Any(e => e.ErrorMessage == "User invalid"));

        Assert.IsTrue(_page.ModelState.TryGetValue("TimesheetEntry.Hours", out var hoursEntry));
        Assert.IsTrue(hoursEntry!.Errors.Any(e => e.ErrorMessage == "Hours invalid"));

        // Does not redirect
        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsNotInstanceOfType(result, typeof(RedirectToPageResult));

        _timesheetLogic.Received(1).AddNewTimesheetEntry(Arg.Any<TimesheetEntry>());
    }
}