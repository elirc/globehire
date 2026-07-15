using GlobeHire.Contracts.V1.Payroll;
using GlobeHire.Domain.Common;
using GlobeHire.Domain.Payroll;

namespace GlobeHire.Api.Payroll;
internal static class PayrollEndpoints
{
    public static IEndpointRouteBuilder MapPayrollEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/clients/{clientId}/payroll/preview", (PayrollPreviewRequest request) =>
        {
            var statement = PayrollStatement.Calculate(request.EmploymentId, Money.Of(request.AnnualSalary, request.Currency), request.PeriodsPerYear, request.EmployeeDeductionRate, request.EmployerContributionRate);
            return TypedResults.Ok(new PayrollPreviewResponse(statement.GrossPay.Amount, statement.EmployeeDeductions.Amount, statement.EmployerContributions.Amount, statement.NetPay.Amount, statement.GrossPay.Currency));
        }).WithTags("Payroll").Produces<PayrollPreviewResponse>();
        return endpoints;
    }
}
