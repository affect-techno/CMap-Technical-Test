using CMap.TechnicalTest.Models;

namespace CMap.TechnicalTest.DataAccess.Interfaces;

public interface ITimesheetEntryRepository
{
    TimesheetEntry? GetTimesheetEntryById(Guid id);
    
    IEnumerable<TimesheetEntry> GetTimesheetEntriesByUserId(Guid userId, DateTime? startDate, DateTime? endDate);
    
    IEnumerable<TimesheetEntry> GetTimesheetEntriesByUserIdAndProjectId(Guid userId, Guid projectId, DateTime? startDate, DateTime? endDate);
    
    TimesheetEntry CreateTimesheetEntry(TimesheetEntry entry);
    
    TimesheetEntry UpdateTimesheetEntry(TimesheetEntry entry);
    
    void DeleteTimesheetEntry(Guid entryId);
}