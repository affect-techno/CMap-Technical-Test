using CMap.TechnicalTest.BusinessLogic.Exceptions;
using CMap.TechnicalTest.Models;
using CMap.TechnicalTest.UserInterface.Context;
using CMap.TechnicalTest.UserInterface.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMap.TechnicalTest.UserInterface.Pages.Timesheets;

public class EditTimesheet : PageModel
{
    private readonly TimesheetContext _timesheetContext;

    public EditTimesheet(TimesheetContext timesheetContext)
    {
        _timesheetContext = timesheetContext ?? throw new ArgumentNullException(nameof(timesheetContext));
    }

    public SelectList Users => new(_timesheetContext.Users, nameof(Models.User.Id), nameof(Models.User.Name));
    
    public SelectList Projects => new(_timesheetContext.Projects, nameof(Project.Id), nameof(Project.Name));

    [BindProperty]
    public TimesheetEntry? TimesheetEntry { get; set; }

    public IActionResult OnGet(Guid id)
    {
        TimesheetEntry = _timesheetContext.TimesheetLogic.GetTimesheetById(id);

        if (TimesheetEntry == null)
            return NotFound();

        return Page();
    }

    public IActionResult OnPost(Guid id)
    {
        if (!ModelState.IsValid || TimesheetEntry == null || TimesheetEntry.UserId == Guid.Empty || TimesheetEntry.ProjectId == Guid.Empty)
        {
            if(TimesheetEntry?.UserId == Guid.Empty)
                ModelState.AddModelError($"{nameof(TimesheetEntry)}.{nameof(TimesheetEntry.UserId)}", "User is required");
            
            if(TimesheetEntry?.ProjectId == Guid.Empty)
                ModelState.AddModelError($"{nameof(TimesheetEntry)}.{nameof(TimesheetEntry.ProjectId)}", "Project is required");
            
            return Page();
        }

        TimesheetEntry.Id = id;
        TimesheetEntry.Date = TimesheetEntry.Date.Date;

        try
        {
            _timesheetContext.TimesheetLogic.EditTimesheetEntry(TimesheetEntry);
        }
        catch (BadRequestException ex)
        {
            foreach (BadRequestDetail badRequestDetail in ex.Details)
            {
                ModelState.AddModelError($"{nameof(TimesheetEntry)}.{badRequestDetail.Target}", badRequestDetail.Description);
            }

            return Page();
        }
            
        return RedirectToPage("./UsersTimesheets", new { userId = TimesheetEntry.UserId, date = TimesheetEntry.Date.ToParameterString()} );
    }
}