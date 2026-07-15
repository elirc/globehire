# Sprint 10: Tenant authorization

## Goal

Enforce tenant, role, worker, and country scope consistently across every author and worker API.

## Scope

Include request identity abstraction, policy-based authorization, tenant-scoped repositories, resource authorization, denial Problem Details, and security regression tests. Exclude a real identity provider.

## Teaching focus

Authentication versus authorization, IDOR prevention, defense in depth, query-by-scope design, and avoiding ambient tenant state.

## Planned artifacts

Authorization matrix, cross-tenant tests, fake identity adapter, ADR, and object-authorization note.

## Exercises

## Delivered evidence

- Hire-case and offer repository ports require tenant context for every lookup.
- In-memory adapters use composite tenant/resource keys.
- Offer creation, approval, contract issue, and employment activation propagate the route tenant.
- A cross-tenant API test proves a known hire-case ID returns `404` to another client.
- The threat model distinguishes route scoping from real authentication and role authorization.

## Exercises

Add a country-scoped payroll operator. Attempt to bypass endpoint checks through a repository lookup.
