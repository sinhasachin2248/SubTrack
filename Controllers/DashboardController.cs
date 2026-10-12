
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SubTrack.Data;
using SubTrack.Models;
using SubTrack.ViewModels;

namespace SubTrack.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var nextSevenDays = today.AddDays(7);
            var monthEnd = new DateTime(today.Year, today.Month,
                DateTime.DaysInMonth(today.Year, today.Month));

            var activeSubscriptions = await _context.Subscriptions
                .AsNoTracking()
                .Where(s => s.IsActive && s.UserId == userId)
                .ToListAsync();

            var cancelledCount = await _context.Subscriptions
                .CountAsync(s => !s.IsActive && s.UserId == userId);

            decimal monthlyCost = 0;
            decimal annualCost = 0;

            foreach (var subscription in activeSubscriptions)
            {
                if (subscription.BillingCycle == "Monthly")
                {
                    monthlyCost += subscription.Price;
                    annualCost += subscription.Price * 12;
                }
                else if (subscription.BillingCycle == "Yearly")
                {
                    monthlyCost += subscription.Price / 12;
                    annualCost += subscription.Price;
                }
            }

            var upcomingRenewals = activeSubscriptions
                .Where(s =>
                    s.NextRenewalDate.Date >= today &&
                    s.NextRenewalDate.Date <= nextSevenDays)
                .OrderBy(s => s.NextRenewalDate)
                .ToList();

            var categorySpending = activeSubscriptions
                .GroupBy(s => s.Category)
                .Select(group => new CategorySpendingViewModel
                {
                    Category = group.Key,
                    MonthlyCost = group.Sum(s =>
                        s.BillingCycle == "Monthly"
                            ? s.Price
                            : s.BillingCycle == "Yearly"
                                ? s.Price / 12
                                : 0)
                })
                .OrderByDescending(c => c.MonthlyCost)
                .ToList();

            var nextRenewal = activeSubscriptions
                .Where(s => s.NextRenewalDate.Date >= today)
                .OrderBy(s => s.NextRenewalDate)
                .FirstOrDefault();

            var viewModel = new DashboardViewModel
            {
                TotalActiveSubscriptions = activeSubscriptions.Count,
                TotalCancelledSubscriptions = cancelledCount,
                MonthlyRecurringCost = monthlyCost,
                AnnualRecurringCost = annualCost,
                DueTodayCount = activeSubscriptions.Count(s =>
                    s.NextRenewalDate.Date == today),
                OverdueCount = activeSubscriptions.Count(s =>
                    s.NextRenewalDate.Date < today),
                RenewalsThisMonthCount = activeSubscriptions.Count(s =>
                    s.NextRenewalDate.Date >= today &&
                    s.NextRenewalDate.Date <= monthEnd),
                NextRenewalDate = nextRenewal?.NextRenewalDate,
                NextRenewalName = nextRenewal?.Name,
                TopCategory = categorySpending.FirstOrDefault(),
                RenewalExposure = activeSubscriptions
                    .Where(s => s.NextRenewalDate.Date >= today &&
                        s.NextRenewalDate.Date <= nextSevenDays)
                    .Sum(s => s.Price),
                UpcomingRenewals = upcomingRenewals,
                CategorySpending = categorySpending
            };

            return View(viewModel);
        }
    }
}
