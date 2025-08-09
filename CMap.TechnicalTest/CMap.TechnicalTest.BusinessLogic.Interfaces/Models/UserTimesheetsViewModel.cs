using System.ComponentModel;

namespace CMap.TechnicalTest.BusinessLogic.Interfaces.Models;

public class UserTimesheetsViewModel(TimesheetViewModel[] timesheets, ProjectHoursViewModel[] projectHours)
{
    [DisplayName("Projects")]
    public TimesheetViewModel[] Timesheets { get; } = timesheets;

    [DisplayName("Projects")]
    public ProjectHoursViewModel[] ProjectHours { get; } = projectHours;
}