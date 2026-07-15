using System.Reflection;
using GlobeHire.Application.WorkforcePlans;
using GlobeHire.Contracts.V1.WorkforcePlans;
using GlobeHire.Domain.WorkforcePlans;

namespace GlobeHire.Architecture.Tests;

public sealed class DependencyDirectionTests
{
    [Fact]
    public void Domain_DoesNotReferenceOtherGlobeHireProjects()
    {
        var references = ProjectReferences(typeof(WorkforcePlan).Assembly);

        Assert.Empty(references);
    }

    [Fact]
    public void Application_ReferencesOnlyDomainInsideSolution()
    {
        var references = ProjectReferences(typeof(CreateWorkforcePlanHandler).Assembly);

        Assert.Equal(["GlobeHire.Domain"], references);
    }

    [Fact]
    public void Contracts_RemainIndependentFromDomainAndApplication()
    {
        var references = ProjectReferences(typeof(WorkforcePlanResponse).Assembly);

        Assert.Empty(references);
    }

    private static string[] ProjectReferences(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name?.StartsWith("GlobeHire.", StringComparison.Ordinal) is true)
            .Order(StringComparer.Ordinal)
            .ToArray()!;
}
