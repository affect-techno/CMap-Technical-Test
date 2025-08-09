using System.Web;
using CMap.TechnicalTest.BusinessLogic.Exceptions;
using CMap.TechnicalTest.Models;
using CMap.TechnicalTest.UserInterface.Context;
using CMap.TechnicalTest.UserInterface.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMap.TechnicalTest.UserInterface.Pages.Timesheets;

public class NewTimesheet : PageModel
{
    private readonly TimesheetContext _timesheetContext;

    public NewTimesheet(TimesheetContext timesheetContext)
    {
        _timesheetContext = timesheetContext ?? throw new ArgumentNullException(nameof(timesheetContext));
    }

    public SelectList Users => new(_timesheetContext.Users, nameof(Models.User.Id), nameof(Models.User.Name));
    
    public SelectList Projects => new(_timesheetContext.Projects, nameof(Project.Id), nameof(Project.Name));


    [BindProperty]
    public TimesheetEntry? TimesheetEntry { get; set; }

    public IActionResult OnGet()
    {
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return OnGet();
        }
        
        if(TimesheetEntry != null)
        {
            TimesheetEntry.Date = TimesheetEntry.Date.Date;

            try
            {
                _timesheetContext.TimesheetLogic.AddNewTimesheetEntry(TimesheetEntry);
            }
            catch (BadRequestException ex)
            {
                foreach (BadRequestDetail badRequestDetail in ex.Details)
                {
                    ModelState.AddModelError($"{nameof(TimesheetEntry)}.{badRequestDetail.Target}", badRequestDetail.Description);
                }
                
                return Page();
            }

            return RedirectToPage("./UsersTimesheets",
                new { userId = TimesheetEntry.UserId, date = TimesheetEntry.Date.ToParameterString() });
        }

        return OnGet();
    }
}