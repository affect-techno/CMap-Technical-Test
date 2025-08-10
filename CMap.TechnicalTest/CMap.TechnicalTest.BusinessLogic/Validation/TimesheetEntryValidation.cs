using CMap.TechnicalTest.BusinessLogic.Interfaces.Exceptions;
using CMap.TechnicalTest.DataAccess.Interfaces;
using CMap.TechnicalTest.Models;

namespace CMap.TechnicalTest.BusinessLogic.Validation;

public class TimesheetEntryValidation : ITimesheetEntryValidation
{
    private readonly List<BadRequestDetail> _errorDetails = new List<BadRequestDetail>();
    private readonly ITimesheetEntryRepository _timesheetEntryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;

    public TimesheetEntryValidation(ITimesheetEntryRepository timesheetEntryRepository, IUserRepository userRepository, IProjectRepository projectRepository)
    {
        _timesheetEntryRepository = timesheetEntryRepository ?? throw new ArgumentNullException(nameof(timesheetEntryRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
    }

    public BadRequestException? ValidateNew(TimesheetEntry timesheetEntry)
    {
        ArgumentNullException.ThrowIfNull(timesheetEntry);
        
        ValidateCommonRules(timesheetEntry);

        return _errorDetails.Count > 0 ? new BadRequestException("The new timesheet entry is invalid", _errorDetails) : null;
    }

    public BadRequestException? ValidateExisting(TimesheetEntry timesheetEntry)
    {
        ArgumentNullException.ThrowIfNull(timesheetEntry);
        
        ValidateCommonRules(timesheetEntry);
        
        if (_timesheetEntryRepository.GetTimesheetEntryById(timesheetEntry.Id) is null)
            _errorDetails.Add(new BadRequestDetail($"No timesheet entry found for ID {timesheetEntry.Id}", nameof(timesheetEntry.Id)));

        return _errorDetails.Count > 0 ? new BadRequestException("The updated timesheet entry is invalid", _errorDetails) : null;
    }

    private void ValidateCommonRules(TimesheetEntry timesheetEntry)
    {
        if(_userRepository.GetUserById(timesheetEntry.UserId) is null)
            _errorDetails.Add(new BadRequestDetail($"No user found for ID {timesheetEntry.Id}", nameof(timesheetEntry.UserId)));
        
        if(_projectRepository.GetProjectById(timesheetEntry.ProjectId) is null)
            _errorDetails.Add(new BadRequestDetail($"No project found for ID {timesheetEntry.ProjectId}", nameof(timesheetEntry.ProjectId)));
        
        if(timesheetEntry.Date == default)
            _errorDetails.Add(new BadRequestDetail("A date for this entry is required", nameof(timesheetEntry.Date)));
        
        if(timesheetEntry.Hours <= 0 || timesheetEntry.Hours > 24)
            _errorDetails.Add(new BadRequestDetail("The hours must be between 0 and 24", nameof(timesheetEntry.Hours)));
        
        if(_timesheetEntryRepository.GetTimesheetEntriesByUserIdAndProjectId(timesheetEntry.UserId, timesheetEntry.ProjectId, timesheetEntry.Date.Date, timesheetEntry.Date.Date.AddDays(1).AddTicks(-1))
           .Any(te => te.Id != timesheetEntry.Id))
            _errorDetails.Add(new BadRequestDetail("There is already a timesheet entry for this date, user and project", nameof(timesheetEntry.Date)));
    }
}