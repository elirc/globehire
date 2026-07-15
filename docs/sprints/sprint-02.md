# Sprint 02: Country policies and cost estimates

## Goal

Calculate transparent employer costs from immutable, effective-dated country policies and issue expiring estimates with explicit assumptions.

## Scope

Include `Money`, percentage/rate types, policy date ranges, employer-cost lines, currency invariants, estimate versioning, and boundary/property tests. Exclude production tax formulas and live FX.

## Teaching focus

Value objects, temporal modeling, decimal rounding, policy snapshots, and adding country behavior without switch-statement sprawl or premature plugin frameworks.

## Planned artifacts

Policy repository contract, calculator strategies, estimate API, generated examples, ADR, and money/date concept note.

## Exercises

Add a fictional country policy. Prove that estimates created before a policy change remain reproducible.
