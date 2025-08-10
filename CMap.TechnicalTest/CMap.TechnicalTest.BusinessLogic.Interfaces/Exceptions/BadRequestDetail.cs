namespace CMap.TechnicalTest.BusinessLogic.Interfaces.Exceptions;

public class BadRequestDetail(string description, string target)
{
    public string Description { get; } = description;
    public string Target { get; } = target;
}