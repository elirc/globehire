namespace GlobeHire.Contracts.V1.Payroll;
public sealed record PayrollPreviewRequest(Guid EmploymentId, decimal AnnualSalary, string Currency, int PeriodsPerYear, decimal EmployeeDeductionRate, decimal EmployerContributionRate);
public sealed record PayrollPreviewResponse(decimal GrossPay, decimal EmployeeDeductions, decimal EmployerContributions, decimal NetPay, string Currency);
