using CMap.TechnicalTest.BusinessLogic.Exceptions;
using CMap.TechnicalTest.DataAccess.Interfaces;

namespace CMap.TechnicalTest.BusinessLogic.Validation;

public class TimesheetEntryForUserValidation(IUserRepository userRepository) : ITimesheetEntryForUserValidation
{
    public BadRequestException? Validate(Guid userId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty, nameof(userId));
        
        List<BadRequestDetail> validationErrors = new List<BadRequestDetail>();

        if (userRepository.GetUserById(userId) == null)
            validationErrors.Add(new BadRequestDetail("There is no user with the specified id", nameof(userId)));

        return validationErrors.Count > 0 ? new BadRequestException("Invalid parameters provided", validationErrors) : null;
    }
}