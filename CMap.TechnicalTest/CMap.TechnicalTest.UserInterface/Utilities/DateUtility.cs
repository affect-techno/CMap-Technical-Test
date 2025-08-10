using System.Globalization;

namespace CMap.TechnicalTest.UserInterface.Utilities;

public static class DateUtility
{
    private const string FormatString = "yyyy-MM-dd";
    
    public static DateTime? ToDateFromParameter(this string date)
    {
        if(string.IsNullOrWhiteSpace(date))
            return null;

        if (DateTime.TryParseExact(date, FormatString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result))
        {
            return result;
        }

        return null;
    }

    public static string ToParameterString(this DateTime date)
    {
        return date.ToString(FormatString);
    }
}