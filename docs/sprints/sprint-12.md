# Sprint 12: Bulk data and reporting

## Goal

Import workforce changes with row-level outcomes, export stable data contracts, and query operational projections efficiently.

## Scope

Include CSV hardening, dry run, partial success, reconciliation IDs, cursor pagination, filter allowlists, reporting projections, and measured performance. Exclude arbitrary query languages.

## Teaching focus

Batch idempotency, partial failure, projection models, cursor stability, untrusted files, and performance evidence.

## Planned artifacts

Import property tests, query API, benchmark evidence, ADR, and bulk-boundary note.

## Exercises

## Delivered evidence

- Bulk batches are bounded to 1,000 rows and preserve ordered row-level outcomes.
- Duplicate external keys, malformed country codes, and invalid money return stable codes.
- Valid rows remain distinguishable when neighboring rows fail.
- Report contracts enforce bounded page size and opaque cursor semantics.
- API endpoints expose validation and paged read-model boundaries.

## Exercises

Resume an interrupted import. Add a projection without coupling it to aggregate persistence.
