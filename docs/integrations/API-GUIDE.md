# Integration guide

All teaching endpoints are under `/api/v1/clients/{clientId}`. Treat `clientId` as a security boundary, not a filter. Real authorization must derive and validate tenant access from the authenticated principal; a path value alone is not proof of access.

## Contract rules

- Send ISO country/currency codes, `DateOnly` values as `YYYY-MM-DD`, and timestamps in UTC.
- Use `Idempotency-Key` for retryable creates. Replaying the same logical operation with the same key must not create a second resource.
- Preserve `ETag` and send the expected version through `If-Match` for mutable resources. On conflict, fetch current state and ask the user to reconcile rather than silently overwrite.
- Parse Problem Details by HTTP status and stable `code`; display `detail` to a human but do not branch on its wording.
- Record `X-Trace-Id` in integration logs and support requests. Do not put secrets or PII in correlation values.
- Treat `404` for a resource outside the current tenant as intentional information minimization.

## Webhooks and bulk work

Webhook message and delivery IDs are stable across retry. Consumers must make processing idempotent before acknowledging. Signatures in this model demonstrate deterministic input only; production needs HMAC verification, timestamps, replay windows, secret rotation, endpoint validation, and SSRF defenses.

Bulk validation returns bounded, row-level outcomes so one invalid row need not erase valid findings. External row keys should be stable across resubmission. A real importer must define whether valid rows commit independently or the file is atomic, then expose reconciliation evidence.

## Compatibility

Changes within v1 should be additive: add optional fields or new endpoints without changing existing meanings. Removing/renaming fields, changing requiredness, altering enum meaning, or changing idempotency/version semantics requires a new API version and a consumer migration window. OpenAPI describes transport shape; examples and contract tests should protect behavior.

