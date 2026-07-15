# Deliberate flaw harvest

The course planted five realistic flaws, reviewed their symptoms, and repaired them at the boundary that owned the guarantee.

| Introduced | Flaw | Harvest | Regression evidence |
| --- | --- | --- | --- |
| Sprint 2 | Rounding equal shares independently can lose or create cents | Sprint 14 largest-remainder allocation | allocations sum exactly to the source and differ by at most one cent |
| Sprint 2 | Inclusive policy end dates overlap adjacent versions | Sprint 13 half-open `[from, to)` convention | boundary day selects exactly the new policy |
| Sprint 3 | Hire-case lookup by GUID omitted tenant ownership | Sprint 10 tenant-required ports and composite keys | known foreign ID returns `404` |
| Sprint 4 | Approval mutations occurred after an unlocked repository read | Sprint 14 atomic repository mutation | two writers with one version produce one commit and one conflict |
| Sprint 11 | Retry used a new delivery identity, weakening deduplication | Sprint 14 stable delivery identity with incremented attempt | retry retains delivery/message IDs and increments attempt |

## Lesson

The fixes do not add general-purpose frameworks. Each strengthens one explicit contract: exact money totals, temporal composition, tenant ownership, compare-and-swap mutation, or retry identity. Regression tests explain the former failure better than comments alone.
