namespace CMap.TechnicalTest.BusinessLogic.Interfaces.Exceptions;

public class BadRequestException : Exception
{
    private readonly List<BadRequestDetail> _details = new List<BadRequestDetail>();
    
    public BadRequestException(string message) : base(message)
    { }

    public BadRequestException(string message, IEnumerable<BadRequestDetail> details) : base(message)
    {
        _details.AddRange(details);
    }

    internal void AddDetail(BadRequestDetail detail)
    {
        ArgumentNullException.ThrowIfNull(detail);
        _details.Add(detail);
    }
    
    public IEnumerable<BadRequestDetail> Details => _details;
}

public static class BadRequestExceptionExtensions
{
    public static void ThrowIfNotNull(this BadRequestException? exception)
    {
        if (exception != null)
            throw exception;
    }
}