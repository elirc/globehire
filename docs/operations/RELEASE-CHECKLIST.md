# Release checklist

## Curriculum release (`v1.0.0`)

- [x] Fifteen sprint notes and retrospective are present.
- [x] Five deliberate flaws are revealed with regression evidence.
- [x] Locked restore, formatting, Release build, and all test suites run in CI.
- [x] Dependency vulnerability and secret-history checks run in CI.
- [x] Integration, operations, testing, and over-engineering guidance is linked from the root README.
- [x] Health/readiness and trace response behavior have integration tests.
- [x] No real credentials, PII, proprietary source, or legal/payroll claims are included.

## Required before any real deployment

- [ ] Durable transactional storage, migrations, atomic audit/outbox, and tested restore.
- [ ] Authenticated identity, tenant claims, role/scope enforcement, and security testing.
- [ ] Qualified ownership and traceable sources for country, tax, payroll, and legal policy.
- [ ] PII inventory, encryption, retention/deletion, access audit, and incident obligations.
- [ ] Managed secrets, webhook HMAC/replay/SSRF controls, and real provider contract tests.
- [ ] Telemetry backend, dashboards, alerts, capacity evidence, on-call, SLOs, and disaster-recovery rehearsal.

