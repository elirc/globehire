# Sprint 02: Country policies and cost estimates

## Goal

Calculate transparent employer costs from immutable, effective-dated country policies and issue expiring estimates with explicit assumptions.

## Scope

Include `Money`, percentage/rate types, policy date ranges, employer-cost lines, currency invariants, estimate versioning, and boundary/property tests. Exclude production tax formulas and live FX.

## Teaching focus

Value objects, temporal modeling, decimal rounding, policy snapshots, and adding country behavior without switch-statement sprawl or premature plugin frameworks.

## Planned artifacts

Policy repository contract, calculator strategies, estimate API, generated examples, ADR, and money/date concept note.

## Delivered evidence

- Versioned `US-2026.1` and `GB-2026.1` teaching fixtures are selected using the proposed employment start date.
- Zero applicable policies and overlapping policies fail with different stable error codes.
- Currency support is a policy rule; mismatched compensation cannot silently enter a quote.
- The API returns policy identity, monthly salary, employer contributions, service fee, total, and explicit assumptions.
- Domain tests cover start boundaries, missing ranges, overlap, contribution calculation, and currency-safe arithmetic.

## Review prompt

Effective-range endpoints are deceptively easy to misunderstand. Review the exact meaning of `EffectiveTo`, add an adjacent pair of policy versions, and prove there is neither a gap nor a date on which both versions apply.

## Exercises

Add a fictional country policy. Prove that estimates created before a policy change remain reproducible.
