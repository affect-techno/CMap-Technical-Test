using CMap.TechnicalTest.BusinessLogic;
using CMap.TechnicalTest.BusinessLogic.Validation;
using CMap.TechnicalTest.BusinessLogic.Interfaces;
using CMap.TechnicalTest.DataAccess.Interfaces;
using CMap.TechnicalTest.DataAccess.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace CMap.TechnicalTest.ServiceRegister;

public static class SimpleRegister
{
    public static IServiceCollection RegisterTechnicalTestServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Ideally, this would be provided by configuration, however beyond the scope of this example
        
        return services
            .AddSingleton<IProjectRepository, MemoryProjectRepository>()
            .AddSingleton<IUserRepository, MemoryUserRepository>()
            .AddSingleton<ITimesheetEntryRepository, MemoryTimesheetEntryRepository>()
            .AddTransient<IProjectLogic, ProjectLogic>()
            .AddTransient<IUserLogic, UserLogic>()
            .AddTransient<ITimesheetLogic, TimesheetLogic>()
            .AddTransient<ITimesheetEntryForUserValidation, TimesheetEntryForUserValidation>()
            .AddTransient<ITimesheetEntryValidation, TimesheetEntryValidation>();
    }
}