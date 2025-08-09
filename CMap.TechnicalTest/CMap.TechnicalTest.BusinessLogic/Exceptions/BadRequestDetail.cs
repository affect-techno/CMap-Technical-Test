namespace CMap.TechnicalTest.BusinessLogic.Exceptions;

public class BadRequestDetail(string description, string target)
{
    public string Description { get; } = description;
    public string Target { get; } = target;
}