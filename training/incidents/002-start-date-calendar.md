# Incident 002: US plans accepted with yesterday's start date

Branch: `training/incidents/002-start-date-calendar` · Severity: SEV-3 (bad data reaching payroll)

## Alert / symptom

> **Payroll ops ticket PAY-2291:** "Northwind's new engineer has a start date of **14 July**. The workforce plan was created
> **15 July at 10:12 New York time**. We can't run a retroactive start without an amendment. How did the system accept a start date in the past?"
>
> **Follow-up from support:** "Same client, same worker: activating the employment with start date 14 July fails with
> `employment.start_date_in_past`. So the plan says the date is fine and activation says it isn't."

## Data pulled by the on-call engineer

```text
plans created since deploy where proposed_start_date < DATE(created_at in client's country):
  US: 37 plans (all created 00:00–20:00 New York time)
  GB: 2 plans (both created 00:00–01:00 London time)
  DE: 0
```

## Metrics

| Metric (per day) | Week before deploy | Since deploy |
|---|---|---|
| `workforce_plan.start_date_in_past` 422s, US | ~40 | **~3** |
| `workforce_plan.start_date_in_past` 422s, GB | ~6 | ~6 |
| Support tickets "my start date was rejected in the evening" | 5 | 0 |

The team celebrated the drop in 422s and in tickets.

## What changed

```diff
# PR #78  fix(workforce-plans): judge start dates against the hiring country's calendar
# "A US client entering a plan at 21:30 New York time on 15 July was told a 15 July start was in the past…"
-            _clock.Today,
+            _clock.TodayIn(country),

+public static DateOnly TodayIn(this IClock clock, CountryCode country)
+{
+    ... PayrollTimeZones: US → America/New_York, GB → Europe/London; others fall back to clock.Today
+    var zone = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
+    var localNow = TimeZoneInfo.ConvertTime(clock.UtcNow.UtcDateTime.Date, zone);
+    return DateOnly.FromDateTime(localNow);
+}
```

It shipped with a regression test for the reported 21:30 New York case, and that test passes. CI was green.

## Your task

1. **Explain before touching:** for 10:12 New York time on 15 July, work out by hand what `TodayIn` returns. Then do the same for 00:30 London time on 16 July (BST) and for 12:00 London time in January.
2. Write a table-driven test that would have caught this. Choose the instants deliberately and justify each one.
3. Fix it. Then decide whether employment activation (`EmploymentUseCases.cs`) must change in the same PR, and say why.
4. Answer: why did the author's regression test pass? What does that teach about choosing test inputs for time code?

Sealed answer: `training/_answers/incident-002-start-date-calendar.md`
