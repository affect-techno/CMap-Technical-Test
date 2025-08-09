using CMap.TechnicalTest.BusinessLogic.Exceptions;
using CMap.TechnicalTest.BusinessLogic.Interfaces;
using CMap.TechnicalTest.BusinessLogic.Interfaces.Models;
using CMap.TechnicalTest.UserInterface.Pages.Timesheets;
using CMap.TechnicalTest.UserInterface.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NSubstitute;

namespace CMap.TechnicalTest.UserInterface.UnitTests;

[TestClass]
public class UserTimesheetsTests
{
    private IUserLogic _userLogic;
    private ITimesheetLogic _timesheetLogic;

    private UsersTimesheets _page;

    [TestInitialize]
    public void Setup()
    {
        _userLogic = Substitute.For<IUserLogic>();
        _timesheetLogic = Substitute.For<ITimesheetLogic>();

        // Default: Users list is available for the Users SelectList
        _userLogic.GetUsers().Returns([]);

        _page = new UsersTimesheets(_userLogic, _timesheetLogic);
    }

    // Constructor guard clauses
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Ctor_NullUserLogic_Throws()
    {
        _ = new UsersTimesheets(null!, _timesheetLogic);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Ctor_NullTimesheetLogic_Throws()
    {
        _ = new UsersTimesheets(_userLogic, null!);
    }

    // OnGet edge cases
    [TestMethod]
    public void OnGet_NoParams_ReturnsPage_DoesNotQueryTimesheets()
    {
        var result = _page.OnGet();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsNull(_page.ViewModel);
        _timesheetLogic.DidNotReceive().GetTimesheetEntriesForUser(Arg.Any<Guid>(), Arg.Any<DateTime>());
    }

    [TestMethod]
    public void OnGet_UserOnly_SetsUserId_ReturnsPage_NoQuery()
    {
        var userId = Guid.NewGuid();

        var result = _page.OnGet(userId: userId);

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreEqual(userId, _page.UserId);
        Assert.IsNull(_page.ViewModel);
        _timesheetLogic.DidNotReceive().GetTimesheetEntriesForUser(Arg.Any<Guid>(), Arg.Any<DateTime>());
    }

    [TestMethod]
    public void OnGet_DateOnly_SetsWeekCommencing_ReturnsPage_NoQuery()
    {
        var week = new DateTime(2025, 1, 6);
        var dateParam = week.ToParameterString();

        var result = _page.OnGet(date: dateParam);

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreEqual(week, _page.WeekCommencing);
        Assert.IsNull(_page.ViewModel);
        _timesheetLogic.DidNotReceive().GetTimesheetEntriesForUser(Arg.Any<Guid>(), Arg.Any<DateTime>());
    }

    [TestMethod]
    public void OnGet_UserAndDate_Success_SetsViewModel_QueriesTimesheets_WithDateTruncated()
    {
        var userId = Guid.NewGuid();
        var week = new DateTime(2025, 1, 6); // Monday
        var dateParam = week.ToParameterString();

        var vm = new UserTimesheetsViewModel(
            timesheets:
            [
                new TimesheetViewModel(Guid.NewGuid(), "Alice", "Project X", week.AddDays(1), 4, "desc")
            ],
            projectHours: []);

        _timesheetLogic.GetTimesheetEntriesForUser(userId, week).Returns(vm);

        var result = _page.OnGet(userId, dateParam);

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreSame(vm, _page.ViewModel);
        _timesheetLogic.Received(1).GetTimesheetEntriesForUser(userId, week); // .Date applied in page model
    }

    [TestMethod]
    public void OnGet_UserAndDate_ValidationFails_AddsModelErrors_AndDoesNotThrow()
    {
        var userId = Guid.NewGuid();
        var week = new DateTime(2025, 1, 6);
        var dateParam = week.ToParameterString();

        var details = new List<BadRequestDetail>
        {
            new BadRequestDetail("User missing", "UserId"),
            new BadRequestDetail("Date invalid", "WeekCommencing")
        };
        var badRequest = new BadRequestException("Invalid", details);

        _timesheetLogic.GetTimesheetEntriesForUser(userId, week).Returns(_ => throw badRequest);

        var result = _page.OnGet(userId, dateParam);

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.IsNull(_page.ViewModel);

        Assert.IsTrue(_page.ModelState.TryGetValue("UserId", out var userState));
        Assert.IsTrue(userState!.Errors.Any(e => e.ErrorMessage == "User missing"));

        Assert.IsTrue(_page.ModelState.TryGetValue("WeekCommencing", out var weekState));
        Assert.IsTrue(weekState!.Errors.Any(e => e.ErrorMessage == "Date invalid"));

        _timesheetLogic.Received(1).GetTimesheetEntriesForUser(userId, week);
    }

    // OnPostSearch
    [TestMethod]
    public void OnPostSearch_ModelInvalid_ReturnsPage_AndDoesNotQuery()
    {
        _page.ModelState.AddModelError("x", "invalid");

        var result = _page.OnPostSearch();

        Assert.IsInstanceOfType(result, typeof(PageResult));
        _timesheetLogic.DidNotReceive().GetTimesheetEntriesForUser(Arg.Any<Guid>(), Arg.Any<DateTime>());
    }

    [TestMethod]
    public void OnPostSearch_ModelValid_WithBoundProperties_RedirectsWithParameters_AndDoesNotQuery()
    {
        var userId = Guid.NewGuid();
        var weekWithTime = new DateTime(2025, 1, 8, 14, 22, 0);
        var expectedDate = weekWithTime.Date; // Truncated to Monday

        _page.UserId = userId;
        _page.WeekCommencing = weekWithTime;

        var result = _page.OnPostSearch();

        var redirect = result as RedirectToPageResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("./UsersTimesheets", redirect!.PageName);

        Assert.IsNotNull(redirect.RouteValues);
        Assert.AreEqual(userId, redirect.RouteValues!["userId"]);
        Assert.AreEqual(expectedDate.ToParameterString(), redirect.RouteValues!["date"]);

        _timesheetLogic.DidNotReceive().GetTimesheetEntriesForUser(Arg.Any<Guid>(), Arg.Any<DateTime>());
        Assert.IsNull(_page.ViewModel);
    }


    // OnPostEdit
    [TestMethod]
    public void OnPostEdit_WithId_RedirectsToEditTimesheet()
    {
        var id = Guid.NewGuid();

        var result = _page.OnPostEdit(id);

        var redirect = result as RedirectToPageResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("./EditTimesheet", redirect!.PageName);
        Assert.AreEqual(id, redirect.RouteValues!["id"]);
    }

    [TestMethod]
    public void OnPostEdit_NoId_ReturnsPage()
    {
        var result = _page.OnPostEdit(null);

        Assert.IsInstanceOfType(result, typeof(PageResult));
    }

    // OnPostDelete
    [TestMethod]
    public void OnPostDelete_WithId_DeletesAndRefreshesList()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var week = new DateTime(2025, 1, 6);
        var weekParam = week.ToParameterString();

        var vm = new UserTimesheetsViewModel([], []);
        _timesheetLogic.GetTimesheetEntriesForUser(userId, week).Returns(vm);

        var result = _page.OnPostDelete(id, userId, weekParam);

        Assert.IsInstanceOfType(result, typeof(PageResult));
        Assert.AreSame(vm, _page.ViewModel);

        _timesheetLogic.Received(1).DeleteTimesheetEntry(id);
        _timesheetLogic.Received(1).GetTimesheetEntriesForUser(userId, week);
    }

    [TestMethod]
    public void OnPostDelete_NoId_OnlyRefreshesList()
    {
        var userId = Guid.NewGuid();
        var week = new DateTime(2025, 1, 6);
        var weekParam = week.ToParameterString();

        _timesheetLogic.GetTimesheetEntriesForUser(userId, week)
            .Returns(new UserTimesheetsViewModel([], []));

        var result = _page.OnPostDelete(null, userId, weekParam);

        Assert.IsInstanceOfType(result, typeof(PageResult));
        _timesheetLogic.DidNotReceive().DeleteTimesheetEntry(Arg.Any<Guid>());
        _timesheetLogic.Received(1).GetTimesheetEntriesForUser(userId, week);
    }
}