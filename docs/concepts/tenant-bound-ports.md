# Make unsafe lookup difficult to express

Checking tenant ownership in each endpoint is fragile because a new caller can forget. Sprint 10 moves the boundary into the application port: `FindAsync` requires both `ClientId` and resource ID.

The in-memory adapters use composite keys, and a foreign tenant receives the same not-found response as an unknown ID. This is object-level authorization by construction at the persistence seam. Authentication and role authorization still belong at the API boundary; tenant-bound ports are defense in depth, not a replacement.
