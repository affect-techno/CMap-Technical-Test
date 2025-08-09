using CMap.TechnicalTest.BusinessLogic.Interfaces.Models;
using CMap.TechnicalTest.Models;

namespace CMap.TechnicalTest.BusinessLogic.Interfaces;

public interface ITimesheetLogic
{
    TimesheetEntry? GetTimesheetById(Guid id);
    
    TimesheetEntry AddNewTimesheetEntry(TimesheetEntry timesheetEntry);

    TimesheetEntry EditTimesheetEntry(TimesheetEntry timesheetEntry);

    void DeleteTimesheetEntry(Guid timesheetEntryId);
    
    UserTimesheetsViewModel GetTimesheetEntriesForUser(Guid userId, DateTime weekCommencing);
}