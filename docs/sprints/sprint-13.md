# Sprint 13: Immigration and compliance cases

## Goal

Track long-running immigration/compliance cases with policy evidence, milestones, deadlines, and document metadata.

## Scope

Include case types, milestone dependencies, deadline calculation, immutable policy evidence, reminders, reviewer decisions, and escalation. Exclude real filings and legal determinations.

## Teaching focus

Long-running workflows, effective-date edges, explicit clocks, deadline semantics, and evidence trails.

## Planned artifacts

Timeline/deadline tests, case API, notification port, ADR, and effective-policy note.

## Exercises

## Delivered evidence

- Compliance cases require versionable evidence types and reject missing or expired evidence.
- Review readiness and expert approval remain separate transitions.
- Domain records references and expiry metadata, not sensitive document bytes.
- Country policies now use half-open `[from, to)` ranges.
- An adjacent-boundary regression test proves one and only one policy applies.

## Exercises

Handle a policy change while a case is open. Define whether a deadline uses local calendar days or elapsed duration.
