using System.Reflection;

namespace CMap.TechnicalTest.UserInterface.Context;

public static class ContextRegistration
{
    public static IServiceCollection RegisterContexts(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        IEnumerable<Type> contextTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass
                        && t.Namespace == "CMap.TechnicalTest.UserInterface.Context"
                        && t.CustomAttributes.Any(a => a.AttributeType == typeof(AutoRegisterContextAttribute)));

        foreach (Type contextType in contextTypes)
        {
            services.AddTransient(contextType);
        }

        return services;
    }
}