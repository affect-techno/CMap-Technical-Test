using CMap.TechnicalTest.BusinessLogic.Exceptions;
using CMap.TechnicalTest.Models;

namespace CMap.TechnicalTest.BusinessLogic.Validation;

public interface ITimesheetEntryValidation
{
    BadRequestException? ValidateNew(TimesheetEntry  timesheetEntry);
    BadRequestException? ValidateExisting(TimesheetEntry  timesheetEntry);
}