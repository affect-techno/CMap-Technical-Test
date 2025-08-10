using CMap.TechnicalTest.BusinessLogic.Interfaces.Exceptions;

namespace CMap.TechnicalTest.BusinessLogic.Validation;

public interface ITimesheetEntryForUserValidation
{
    BadRequestException? Validate(Guid userId);
}