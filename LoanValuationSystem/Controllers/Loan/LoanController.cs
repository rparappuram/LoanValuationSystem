using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanValuationSystem.Controllers
{
    public class LoanController : Controller
    {
        private readonly LoanDbContext _context;

        public LoanController(LoanDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch loan data
            var loans = await _context.Loans.ToListAsync();
            var loanPerformance = await _context.LoanPerformance
                .OrderBy(lp => lp.ReportingPeriod)
                .ToListAsync();

            // Compute summary statistics
            var totalLoans = loans.Count;
            var avgInterestRate = loans.Any() ? loans.Average(l => l.InterestRate) : 0;

            var delinquentLoanIds = loanPerformance
                .Where(lp => lp.DelinquencyStatus > 0)
                .Select(lp => lp.LoanId)
                .Distinct()
                .ToList();
            var delinquentLoans = delinquentLoanIds.Count;
            var delinquencyRate = totalLoans > 0 ? (double)delinquentLoans / totalLoans * 100 : 0;

            var latestBalances = loanPerformance
                .GroupBy(lp => lp.LoanId)
                .Select(g => g.OrderByDescending(lp => lp.ReportingPeriod).First())
                .Sum(lp => lp.CurrentBalance);
            var totalOutstandingBalance = latestBalances;

            var balanceDates = loanPerformance
                .Select(lp => lp.ReportingPeriod.ToString("yyyy-MM-dd"))
                .Distinct()
                .ToList();

            var balances = balanceDates
                .Select(date => loanPerformance
                    .Where(lp => lp.ReportingPeriod.ToString("yyyy-MM-dd") == date)
                    .GroupBy(lp => lp.LoanId)
                    .Select(g => g.OrderByDescending(lp => lp.ReportingPeriod).First())
                    .Sum(lp => lp.CurrentBalance))
                .ToList();

            // Prepare data for view
            var summaryData = new
            {
                TotalLoans = totalLoans,
                AvgInterestRate = avgInterestRate,
                TotalOutstandingBalance = totalOutstandingBalance,
                DelinquencyRate = delinquencyRate,
                BalanceDates = balanceDates,
                Balances = balances
            };

            return View("~/Views/Loan/Index.cshtml", summaryData);
        }
    }
}