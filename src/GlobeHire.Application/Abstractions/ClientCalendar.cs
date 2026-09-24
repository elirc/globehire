using GlobeHire.Domain.Common;

namespace GlobeHire.Application.Abstractions;

/// <summary>
/// Start dates are calendar dates in the hiring country, so "today" must be the country's today, not UTC's.
/// </summary>
public static class ClientCalendar
{
    private static readonly Dictionary<string, string> PayrollTimeZones = new(StringComparer.Ordinal)
    {
        ["US"] = "America/New_York",
        ["GB"] = "Europe/London",
    };

    public static DateOnly TodayIn(this IClock clock, CountryCode country)
    {
        ArgumentNullException.ThrowIfNull(clock);
        if (!PayrollTimeZones.TryGetValue(country.Value, out var zoneId))
        {
            return clock.Today;
        }

        var zone = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
        var localNow = TimeZoneInfo.ConvertTime(clock.UtcNow.UtcDateTime.Date, zone);
        return DateOnly.FromDateTime(localNow);
    }
}
