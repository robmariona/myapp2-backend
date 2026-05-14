using Microsoft.AspNetCore.Mvc;

namespace myapp2.Services;

public interface IInsuranceService
{
    decimal CalculatePremium(decimal productPrice);
    decimal CalculateCoverage(decimal productPrice, decimal requestedCoverage);
}

public class InsuranceService : IInsuranceService
{
    public decimal CalculatePremium(decimal productPrice)
    {

        if (productPrice < 0) return 0;
        // Business Rule: Insurance costs 2% of the item price per year
        return productPrice * 0.02m;
    }

    public decimal CalculateCoverage(decimal productPrice, decimal requestedCoverage)
    {
        // Rule: If coverage is 0, default to full product price
        return requestedCoverage > 0 ? requestedCoverage : productPrice;
    }
}
