using CMap.TechnicalTest.BusinessLogic.Exceptions;

namespace CMap.TechnicalTest.BusinessLogic.Validation;

public interface ITimesheetEntryForUserValidation
{
    BadRequestException? Validate(Guid userId);
}