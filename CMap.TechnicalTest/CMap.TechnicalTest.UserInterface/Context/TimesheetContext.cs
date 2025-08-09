using CMap.TechnicalTest.BusinessLogic.Interfaces;
using CMap.TechnicalTest.Models;

namespace CMap.TechnicalTest.UserInterface.Context;

[AutoRegisterContext]
public class TimesheetContext
{
    public TimesheetContext(IUserLogic userLogic, IProjectLogic projectLogic, ITimesheetLogic timesheetLogic)
    {
        if(userLogic == null)
            throw new ArgumentNullException(nameof(userLogic));
        if(projectLogic == null)
            throw new ArgumentNullException(nameof(projectLogic));
        TimesheetLogic = timesheetLogic ?? throw new ArgumentNullException(nameof(timesheetLogic));
        
        Users = userLogic.GetUsers();
        Projects = projectLogic.GetProjects();
    }

    /// <summary>
    /// Available Users
    /// </summary>
    public User[] Users { get; }

    /// <summary>
    /// Available Projects
    /// </summary>
    public Project[] Projects { get; }
    
    /// <summary>
    /// Timesheet Logic
    /// </summary>
    public ITimesheetLogic TimesheetLogic { get; }
}