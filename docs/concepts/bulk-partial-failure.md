# Bulk work, partial failure, and reports

Bulk input is not one giant request that either hides every error or partially writes without explanation. GlobeHire validates bounded batches and returns a stable result for every row: external key, validity, and error code.

External keys provide caller-controlled idempotency within a batch. Durable imports should persist job and row outcomes so retries resume safely rather than replaying successful effects.

Reports use bounded page sizes and opaque cursors. Public report DTOs should remain purpose-built read models; exporting domain aggregates or database rows would couple integrations to internal storage and sensitive fields.
