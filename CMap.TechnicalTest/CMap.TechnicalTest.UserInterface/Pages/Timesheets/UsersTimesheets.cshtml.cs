using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CMap.TechnicalTest.BusinessLogic.Interfaces;
using CMap.TechnicalTest.BusinessLogic.Interfaces.Exceptions;
using CMap.TechnicalTest.BusinessLogic.Interfaces.Models;
using CMap.TechnicalTest.UserInterface.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMap.TechnicalTest.UserInterface.Pages.Timesheets;

public class UsersTimesheets : PageModel
{
    private readonly IUserLogic _userLogic;
    private readonly ITimesheetLogic _timesheetLogic;

    public UsersTimesheets(IUserLogic userLogic, ITimesheetLogic timesheetLogic)
    {
        _userLogic = userLogic ?? throw new ArgumentNullException(nameof(userLogic));
        _timesheetLogic = timesheetLogic ?? throw new ArgumentNullException(nameof(timesheetLogic));
    }
    
    public SelectList Users => new(_userLogic.GetUsers(), nameof(Models.User.Id), nameof(Models.User.Name));
    
    [BindProperty, Required, DisplayName("User")]
    public Guid? UserId { get; set; }
    
    public UserTimesheetsViewModel? ViewModel { get; private set; }

    [BindProperty, Required, DisplayName("Week Commencing"), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime? WeekCommencing { get; set; }

    public IActionResult OnGet(Guid? userId = null, string? date = null)
    {
        if (userId != null)
        {
            UserId = userId;
            
            ModelState.ClearValidationState(nameof(UserId));
        }
        if (date != null)
        {
            WeekCommencing = date.ToDateFromParameter();
            
            ModelState.ClearValidationState(nameof(WeekCommencing));
        }

        if (UserId != null && UserId.Value != Guid.Empty && WeekCommencing != null && WeekCommencing.Value != DateTime.MinValue)
        {
            try
            {
                ViewModel = _timesheetLogic.GetTimesheetEntriesForUser(UserId.Value, WeekCommencing.Value.Date);
            }
            catch (BadRequestException ex)
            {
                foreach (BadRequestDetail badRequestDetail in ex.Details)
                {
                    ModelState.AddModelError($"{badRequestDetail.Target}", badRequestDetail.Description);
                }
            }
        }
        
        return Page();
    }

    public IActionResult OnPostSearch()
    {
        if (!ModelState.IsValid)
            return Page();

        return RedirectToPage("./UsersTimesheets", new { userId = UserId, date = WeekCommencing?.Date.ToParameterString() } );
    }

    public IActionResult OnPostEdit(Guid? id = null)
    {
        if (id.HasValue)
        {
            return RedirectToPage("./EditTimesheet", new { id = id.Value });
        }

        return Page();
    }

    public IActionResult OnPostDelete(Guid? id = null, Guid? userId = null, string? date = null)
    {
        if (id.HasValue)
        {
            _timesheetLogic.DeleteTimesheetEntry(id.Value);
        }

        return OnGet(userId, date);
    }
}