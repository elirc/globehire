# GlobeHire technical questions (10)

Answer in writing, 3–6 sentences each, naming files. Then check against `training/_answers/technical-questions.md`.

1. **DI lifetimes.** `OfferHandler` is registered `AddScoped` on `main` (`Program.cs:44`). A teammate wants `AddSingleton` "to save allocations". What must be true of every constructor dependency, today *and in the future*, for that to be safe, and how would you make the build enforce it?
2. **Captive dependency.** Why does a scoped service resolved by a singleton behave like a singleton? Where does ASP.NET Core catch this by default, and why don't GlobeHire's API tests (`UseEnvironment("Testing")`) get that protection?
3. **Instants vs dates.** `SystemClock.Today` is `DateOnly.FromDateTime(UtcNow.UtcDateTime)` (`SystemClock.cs:9`). `WorkforcePlan.Create` compares it to `ProposedStartDate`. Explain what a start date *is* (an instant or a calendar date, and whose calendar) and design the smallest change that makes the guard correct for US and GB clients.
4. **Testing time.** You're writing tests for "today in country X". List the instants you would choose and why each one earns its place. What does `IClock` give you here that `DateTime.UtcNow` wouldn't?
5. **Optimistic concurrency.** Offers use `MutateAsync` under a lock (`InMemoryOfferRepository.cs:29-47`). Hire cases don't (`HireCaseUseCases.cs:58-59`). Describe the lost update that's possible on hire cases, and what the SQL Server equivalent of `EnsureVersion` + write would look like (one statement).
6. **Moving to SQL Server.** You replace `InMemoryOfferRepository` with EF Core on SQL Server. What happens to the `lock (_gate)` guarantee? Name the EF Core feature and the column type you'd use for the version, and what exception you'd map to which HTTP status.
7. **Uniqueness belongs in the database.** Two `POST /offers` for the same hire case both succeed today. Where do you enforce "one live offer per hire case" once there's a database, and why is a check in `OfferHandler.CreateAsync` alone not enough?
8. **Idempotency replay.** `Idempotency-Key` replay returns the first plan even when the body differs (`InMemoryWorkforcePlanRepository.cs:33-37`). What should the server store with the key, and what status should a mismatched replay get?
9. **Error contract.** Every business error is `422` (`ApiExceptionHandler.cs:33`). Which GlobeHire errors should be `412` and `428` instead, and what breaks for a client (or a retrying gateway) when they're all `422`?
10. **Legacy rescue order.** You inherit GlobeHire and must fix finding 4 (hire-case race) *and* incident 002 (calendar) in one sprint. In what order do you write characterization tests, refactor and fix, and what's the smallest safe diff for each?
