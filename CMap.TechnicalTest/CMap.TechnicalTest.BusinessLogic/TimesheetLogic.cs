using CMap.TechnicalTest.BusinessLogic.Exceptions;
using CMap.TechnicalTest.BusinessLogic.Validation;
using CMap.TechnicalTest.BusinessLogic.Interfaces;
using CMap.TechnicalTest.BusinessLogic.Interfaces.Models;
using CMap.TechnicalTest.DataAccess.Interfaces;
using CMap.TechnicalTest.Models;

namespace CMap.TechnicalTest.BusinessLogic;

public class TimesheetLogic(
    ITimesheetEntryValidation timesheetEntryValidation,
    ITimesheetEntryRepository timesheetEntryRepository,
    IProjectRepository projectRepository,
    ITimesheetEntryForUserValidation timesheetEntryForUserValidation,
    IUserRepository userRepository)
    : ITimesheetLogic
{
    private readonly ITimesheetEntryValidation _timesheetEntryValidation = timesheetEntryValidation
        ?? throw new ArgumentNullException(nameof(timesheetEntryValidation));
    private readonly ITimesheetEntryRepository _timesheetEntryRepository = timesheetEntryRepository
        ?? throw new ArgumentNullException(nameof(timesheetEntryRepository));
    private readonly IProjectRepository _projectRepository = projectRepository
        ?? throw new ArgumentNullException(nameof(projectRepository));
    private readonly ITimesheetEntryForUserValidation _timesheetEntryForUserValidation = timesheetEntryForUserValidation
        ?? throw new ArgumentNullException(nameof(timesheetEntryForUserValidation));
    private readonly IUserRepository _userRepository = userRepository
        ?? throw new ArgumentNullException(nameof(userRepository));

    public TimesheetEntry? GetTimesheetById(Guid id)
    {
        return _timesheetEntryRepository.GetTimesheetEntryById(id);
    }
    
    public TimesheetEntry AddNewTimesheetEntry(TimesheetEntry timesheetEntry)
    {
        ArgumentNullException.ThrowIfNull(timesheetEntry, nameof(timesheetEntry));
        
        _timesheetEntryValidation.ValidateNew(timesheetEntry).ThrowIfNotNull();

        return _timesheetEntryRepository.CreateTimesheetEntry(timesheetEntry);
    }

    public TimesheetEntry EditTimesheetEntry(TimesheetEntry timesheetEntry)
    {
        ArgumentNullException.ThrowIfNull(timesheetEntry, nameof(timesheetEntry));
        
        _timesheetEntryValidation.ValidateExisting(timesheetEntry).ThrowIfNotNull();

        return _timesheetEntryRepository.UpdateTimesheetEntry(timesheetEntry);
    }
    
    public void DeleteTimesheetEntry(Guid timesheetEntryId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(timesheetEntryId, Guid.Empty);
        
        _timesheetEntryRepository.DeleteTimesheetEntry(timesheetEntryId);
    }
    
    public UserTimesheetsViewModel GetTimesheetEntriesForUser(Guid userId, DateTime weekCommencing)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(weekCommencing, DateTime.MinValue);

        _timesheetEntryForUserValidation.Validate(userId).ThrowIfNotNull();

        TimesheetEntry[] userTimesheetEntries = _timesheetEntryRepository.GetTimesheetEntriesByUserId(userId, weekCommencing, weekCommencing.AddDays(7)).ToArray();
        
        TimesheetViewModel[] timesheets = userTimesheetEntries.Select(ToTimesheetViewModel).ToArray();

        ProjectHoursViewModel[] projectTotals = userTimesheetEntries.ToLookup(te => te.ProjectId, te => te.Hours)
            .Select(group => new ProjectHoursViewModel(
                _projectRepository.GetProjectById(group.Key)?.Name ?? throw new KeyNotFoundException($"No Project with ProjectId: {group.Key}"),
                group.Sum()))
            .ToArray();

        return new UserTimesheetsViewModel(timesheets, projectTotals);
    }

    private TimesheetViewModel ToTimesheetViewModel(TimesheetEntry timesheetEntry)
    {
        string userName = _userRepository.GetUserById(timesheetEntry.UserId)?.Name ?? throw new KeyNotFoundException($"No user with Id: {timesheetEntry.UserId}");
        string projectName = _projectRepository.GetProjectById(timesheetEntry.ProjectId)?.Name ??  throw new KeyNotFoundException($"No project with Id: {timesheetEntry.ProjectId}");
        
        return new TimesheetViewModel(timesheetEntry.Id, userName, projectName, timesheetEntry.Date, timesheetEntry.Hours, timesheetEntry.Description);
    }
}