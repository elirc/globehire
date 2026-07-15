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

Add a country-scoped payroll operator. Attempt to bypass endpoint checks through a repository lookup.
