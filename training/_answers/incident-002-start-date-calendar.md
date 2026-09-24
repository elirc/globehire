# SEALED: incident 002, US plans accepted with yesterday's start date

Branch `training/incidents/002-start-date-calendar` (commit `bb8077c`). BUILD: verified. The full suite passes on the branch (`dotnet test GlobeHire.slnx`, 2026-09-24).
**Reproduced live** with a throwaway table test against `ClientCalendar.TodayIn`:

| UTC instant | Country | Correct local today | `TodayIn` returned |
|---|---|---|---|
| 2026-07-15 14:00Z (10:00 New York) | US | 2026-07-15 | **2026-07-14** ✗ |
| 2026-07-16 01:30Z (21:30 New York, the reported case) | US | 2026-07-15 | 2026-07-15 ✓ |
| 2026-07-15 23:30Z (00:30 London, BST) | GB | 2026-07-16 | **2026-07-15** ✗ |
| 2026-01-15 12:00Z (winter, GMT = UTC) | GB | 2026-01-15 | 2026-01-15 ✓ |

## Root cause

```csharp
var localNow = TimeZoneInfo.ConvertTime(clock.UtcNow.UtcDateTime.Date, zone);
```

`.Date` truncates the instant to **UTC midnight** *before* converting. The code converts "00:00 UTC today", not "now":

- **US (UTC−4/−5):** UTC midnight is always 19:00/20:00 of the *previous* New York day, so `TodayIn` = UTC date − 1, all day. Between 00:00 and 20:00 New York time that is *yesterday*, and yesterday's start dates pass the guard. It is correct only in the 20:00–24:00 window, which is exactly the window the bug report and its test covered.
- **GB summer (UTC+1):** UTC midnight = 01:00 local on the same UTC date, so it returns the UTC date and is wrong only between 00:00 and 01:00 London time. In winter GB = UTC and it's always right.

That explains the metrics: US past-date 422s went from about 40 to about 3 (only dates 2+ days old are still rejected), and GB barely moved.

The activation contradiction: `ActivateEmploymentHandler` still uses `clock.Today` (the UTC date) at `EmploymentUseCases.cs:21`, so the plan and activation now disagree about what "today" is.

## Why the author's test passed

The test used the one instant from the bug report (21:30 New York), and that instant sits in the only window where "UTC midnight, converted" and "now, converted" land on the same local date. **A test that picks inputs from the bug report proves the report is closed, not that the rule is right.** Time tests need instants on *each side* of every boundary: local midnight, UTC midnight, and the DST shift, for a zone west *and* east of UTC.

## Fix

```csharp
public static DateOnly TodayIn(this IClock clock, CountryCode country)
{
    ArgumentNullException.ThrowIfNull(clock);
    if (!PayrollTimeZones.TryGetValue(country.Value, out var zoneId)) return clock.Today;
    var zone = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
    return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.UtcNow, zone).DateTime); // convert the instant, then take the date
}
```

Same PR: `ActivateEmploymentHandler` must use `clock.TodayIn(offer's country)`. Otherwise the two guards disagree, and every client that activates on its plan's start date in the US evening gets a 422. Better still, move the country's "today" into one place (an `IClock.TodayIn(country)` member, or a `ClientCalendar` service), so a third guard can't drift the same way. Longer term, the zone belongs in the country policy data, not in a hard-coded dictionary.

**Data repair:** the 39 plans with `proposed_start_date < local creation date` need a business decision (amend to the next valid date, or contact the client). Don't silently rewrite them.

## Regression test (table-driven)

```csharp
[Theory]
[InlineData("2026-07-15T14:00:00Z", "US", "2026-07-15")] // NY daytime: the incident
[InlineData("2026-07-16T01:30:00Z", "US", "2026-07-15")] // NY evening after UTC midnight: original report
[InlineData("2026-07-16T03:59:00Z", "US", "2026-07-15")] // 1 min before NY midnight (EDT)
[InlineData("2026-07-16T04:00:00Z", "US", "2026-07-16")] // NY midnight
[InlineData("2026-07-15T23:30:00Z", "GB", "2026-07-16")] // London after local midnight, before UTC midnight (BST)
[InlineData("2026-01-15T12:00:00Z", "GB", "2026-01-15")] // winter, zone = UTC
[InlineData("2026-03-29T00:30:00Z", "GB", "2026-03-29")] // DST switch day
[InlineData("2026-07-16T01:30:00Z", "DE", "2026-07-16")] // unconfigured country falls back to UTC date
public void TodayIn_ReturnsTheCountrysCalendarDate(string utc, string country, string expected) { … }
```

## Model postmortem

- **Impact:** 39 plans (37 US, 2 GB) accepted with a start date one day in the past; downstream payroll amendments; activation 422s for the same workers.
- **Trigger:** a fix for evening rejections (PR #78).
- **Root cause:** date truncation happened before time-zone conversion, so the code converted UTC midnight instead of the current instant. Activation kept a separate UTC guard.
- **Why undetected:** the only test input was the reported instant, which falls in the one window where the bug is invisible. The falling 422 count was read as success.
- **Follow-ups:** boundary-table tests for all calendar logic; a single source of "today in country X"; alert on `proposed_start_date < local creation date` as a data-quality invariant, not only on 422 volume.

## Skills

Calendar dates vs instants (time zones, DST) · verify-before-trust (a green test that tests the wrong thing) · characterization/boundary testing · telemetry correctness (a *drop* in errors can be the incident).
