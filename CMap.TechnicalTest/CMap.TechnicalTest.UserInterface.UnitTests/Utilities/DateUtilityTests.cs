using CMap.TechnicalTest.UserInterface.Utilities;

namespace CMap.TechnicalTest.UserInterface.UnitTests.Utilities;

[TestClass]
public class DateUtilityTests
{
    // ToDateFromParameter - null/whitespace and invalid formats
    [TestMethod]
    public void ToDateFromParameter_NullOrWhitespace_ReturnsNull()
    {
        string?[] inputs = { null, "", " ", "   \t  " };

        foreach (var input in inputs)
        {
            var result = DateUtility.ToDateFromParameter(input!);
            Assert.IsNull(result, $"Expected null for '{input ?? "<null>"}'");
        }
    }

    [DataTestMethod]
    [DataRow("2025/01/06")]
    [DataRow("06-01-2025")]
    [DataRow("2025-1-06")]
    [DataRow("2025-01-6")]
    [DataRow("2025-13-01")] // invalid month
    [DataRow("2025-00-10")] // invalid month
    [DataRow("2025-01-32")] // invalid day
    [DataRow("2023-02-29")] // non-leap year
    [DataRow("2025-01-06T12:00:00")]
    [DataRow(" 2025-01-06")]
    [DataRow("2025-01-06 ")]
    [DataRow("abc")]
    public void ToDateFromParameter_InvalidFormat_ReturnsNull(string input)
    {
        var result = input.ToDateFromParameter();
        Assert.IsNull(result);
    }

    // ToDateFromParameter - valid cases
    [TestMethod]
    public void ToDateFromParameter_ValidFormat_ReturnsExpectedDate_TruncatesToMidnight()
    {
        var input = "2025-01-06";

        var result = input.ToDateFromParameter();

        Assert.IsNotNull(result);
        Assert.AreEqual(2025, result!.Value.Year);
        Assert.AreEqual(1, result.Value.Month);
        Assert.AreEqual(6, result.Value.Day);
        Assert.AreEqual(0, result.Value.Hour);
        Assert.AreEqual(0, result.Value.Minute);
        Assert.AreEqual(0, result.Value.Second);
    }

    [DataTestMethod]
    [DataRow("0001-01-01", 1, 1, 1)]
    [DataRow("9999-12-31", 9999, 12, 31)]
    [DataRow("2024-02-29", 2024, 2, 29)] // leap day valid
    public void ToDateFromParameter_Boundaries_AndLeapYear_Success(string input, int y, int m, int d)
    {
        var result = input.ToDateFromParameter();

        Assert.IsNotNull(result);
        Assert.AreEqual(y, result!.Value.Year);
        Assert.AreEqual(m, result.Value.Month);
        Assert.AreEqual(d, result.Value.Day);
    }

    // ToParameterString - formatting
    [TestMethod]
    public void ToParameterString_FormatsWithZeroPadding()
    {
        var dt = new DateTime(2025, 1, 6);

        var s = dt.ToParameterString();

        Assert.AreEqual("2025-01-06", s);
    }

    [TestMethod]
    public void ToParameterString_IgnoresTimeComponent()
    {
        var dt = new DateTime(2025, 1, 6, 13, 45, 23);

        var s = dt.ToParameterString();

        Assert.AreEqual("2025-01-06", s);
    }

    [DataTestMethod]
    [DataRow(1, 1, 1, "0001-01-01")]
    [DataRow(9999, 12, 31, "9999-12-31")]
    public void ToParameterString_MinAndMaxDates(int y, int m, int d, string expected)
    {
        var dt = new DateTime(y, m, d);

        var s = dt.ToParameterString();

        Assert.AreEqual(expected, s);
    }

    // Round-trip scenarios
    [DataTestMethod]
    [DataRow(DateTimeKind.Local)]
    [DataRow(DateTimeKind.Utc)]
    [DataRow(DateTimeKind.Unspecified)]
    public void RoundTrip_DateToStringToDate_PreservesCalendarDate(DateTimeKind kind)
    {
        var dt = new DateTime(2025, 1, 6, 22, 15, 30, kind);

        var s = dt.ToParameterString();
        var parsed = s.ToDateFromParameter();

        Assert.IsNotNull(parsed);
        Assert.AreEqual(dt.Date, parsed!.Value.Date);
    }

    [TestMethod]
    public void RoundTrip_BoundaryValues()
    {
        var dates = new[]
        {
            new DateTime(1,1,1),
            new DateTime(9999,12,31),
            new DateTime(2024,2,29)
        };

        foreach (var dt in dates)
        {
            var s = dt.ToParameterString();
            var parsed = s.ToDateFromParameter();

            Assert.IsNotNull(parsed, $"Parse failed for {s}");
            Assert.AreEqual(dt, parsed!.Value, $"Round-trip mismatch for {s}");
        }
    }
}