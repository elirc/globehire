# Approval workflows and contract records

Approvals are business evidence, not two booleans. An offer records which role approved, which actor supplied that approval, when it happened, and which aggregate version resulted.

Finance and legal are required, and one actor cannot satisfy both roles. Contract issuance is a separate command that is legal only after approval. The first implementation stores immutable contract metadata rather than pretending generated document bytes belong inside the aggregate.

The in-memory adapter is useful for learning but not a concurrency guarantee across processes. A durable adapter must compare and update the expected version atomically; a read-check-write sequence can allow two reviewers to race. Sprint 14 will subject this boundary to concurrency tests.
