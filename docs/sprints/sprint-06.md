# Sprint 06: Payroll periods and statements

## Goal

Open a payroll period, accept typed inputs, validate cutoff rules, calculate fictional statements, and close the run immutably.

## Scope

Include payroll calendar, earnings/deductions/reimbursements, calculation version, balanced totals, statement lines, and close/reopen policy. Exclude real taxes, payments, and bank details.

## Teaching focus

Financial invariants, decimal allocation, cutoff state machines, immutable results, and reconciliation-friendly APIs.

## Planned artifacts

Calculation property tests, payroll endpoints, statement contract, ADR, and financial-systems note.

## Exercises

## Delivered evidence

- Payroll previews calculate gross, deductions, employer contributions, and net with currency-safe money.
- Statements reconcile gross to deductions plus net.
- Periods reject duplicate statements, empty finalization, and changes after finalization.
- The API exposes calculation inputs explicitly instead of hiding fictional rates.

## Exercises

Allocate a total across multiple lines without losing a cent. Add an adjustment run without mutating the closed statement.
