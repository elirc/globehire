# Hardening with regression tests

A flaw harvest starts from a violated guarantee, not a desire to rewrite. First reproduce the failure at the lowest useful boundary. Then repair the owning abstraction and keep the test.

Sprint 14 uses this pattern for cents, dates, tenants, concurrent approvals, and webhook retries. The fixes are intentionally different because the guarantees live in different places. A single resilience or validation framework would obscure rather than unify them.
