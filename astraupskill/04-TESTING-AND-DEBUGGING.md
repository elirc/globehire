# Testing and debugging the boundary

Run the smallest test project that owns the composition: `dotnet test tests/GlobeHire.Application.Tests/GlobeHire.Application.Tests.csproj --disable-build-servers -m:1`. The repository targets .NET 10 through global.json. The recorded run used SDK 10.0.302, a single MSBuild worker, and disabled node reuse to fit the constrained machine. Eight application tests passed in the recorded scope.

When a tenant-isolation test fails, classify the symptom before editing. If the wrong-client result is non-null, inspect the arguments passed by ChangeHireCaseHandler and the key used by InMemoryHireCaseRepository. If the result is null but state changed, a lookup or shared fixture may have exposed the same aggregate through another reference. If the valid completion returns but the timestamp differs, check that the handler passes IClock.UtcNow and that the test injected FixedClock. If stale mutation changes state, inspect the ordering inside CompleteTask: EnsureVersion must run before task replacement.

Avoid testing only HTTP status. A 404 can coexist with an earlier mutation if code is ordered badly. These regressions assert both outward outcome and owner state. Also avoid using DateTimeOffset.UtcNow in the fixture, because a broad time assertion can hide local-offset or delayed-write bugs. Exact deterministic time makes the audit claim reviewable.

The test uses the in-memory repository on purpose. It verifies application composition without a database or external service. It does not exercise persistence isolation levels or multiple processes. If a durable repository is added later, reuse this behavior matrix against that implementation and add transaction/concurrency tests appropriate to its storage engine. Do not erase this suite; it remains the fast contract for every implementation.

## Source excerpt

From [tests/GlobeHire.Application.Tests/GlobeHire.Application.Tests.csproj](../tests/GlobeHire.Application.Tests/GlobeHire.Application.Tests.csproj).

```csproj
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\GlobeHire.Application\GlobeHire.Application.csproj" />
    <ProjectReference Include="..\..\src\GlobeHire.Domain\GlobeHire.Domain.csproj" />
    <ProjectReference Include="..\..\src\GlobeHire.Infrastructure\GlobeHire.Infrastructure.csproj" />
  </ItemGroup>

```

## Course navigation

[README](README.md) / [01-CODEBASE-MAP](01-CODEBASE-MAP.md) / [02-CONCEPTS](02-CONCEPTS.md) / [03-WORKED-CHANGE](03-WORKED-CHANGE.md) / [04-TESTING-AND-DEBUGGING](04-TESTING-AND-DEBUGGING.md) / [05-PRACTICE](05-PRACTICE.md) / [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md) / [07-TRACE-LAB](07-TRACE-LAB.md) / [VERIFICATION](VERIFICATION.md)
