# Sprint 07: Leave, expenses, and benefits

## Goal

Let workers request leave and expenses and enroll in benefits under snapshotted country/client policies.

## Scope

Include local-date overlap rules, balances, expense currencies, approval limits, benefit coverage dates, and payroll-output ports. Exclude real FX providers and benefit carriers.

## Teaching focus

Policy composition, time zones versus local dates, money conversion boundaries, approvals, and eventual handoff to payroll.

## Planned artifacts

Rule matrices, APIs, fake FX/carrier adapters, ADR, and policy-composition note.

## Exercises

## Delivered evidence

- Leave approvals cannot exceed the available entitlement ledger.
- Expense claims use explicit submitted/approved/rejected states.
- Rejections require a decision reason and repeated decisions are rejected.
- Focused domain tests protect balance and evidence rules.

## Exercises

Add a half-day leave unit. Model a late expense that belongs to an adjustment period.
