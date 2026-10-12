
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SubTrack.Data;
using SubTrack.Models;

namespace SubTrack.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "Cookies,Bearer")]
    [Route("api/subscriptions")]
    public class SubscriptionsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/subscriptions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Subscription>>> GetAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var subscriptions = await _context.Subscriptions
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return Ok(subscriptions);
        }

        // GET: api/subscriptions/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Subscription>> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var subscription = await _context.Subscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (subscription == null)
            {
                return NotFound(new
                {
                    message = $"Subscription with ID {id} was not found."
                });
            }

            return Ok(subscription);
        }

        // GET: api/subscriptions/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var activeSubscriptions = await _context.Subscriptions
                .AsNoTracking()
                .Where(s => s.IsActive && s.UserId == userId)
                .ToListAsync();

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

            var today = DateTime.Today;
            var nextSevenDays = today.AddDays(7);

            var upcomingRenewals = activeSubscriptions
                .Where(s =>
                    s.NextRenewalDate.Date >= today &&
                    s.NextRenewalDate.Date <= nextSevenDays)
                .OrderBy(s => s.NextRenewalDate)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Price,
                    s.BillingCycle,
                    s.NextRenewalDate,
                    DaysRemaining = (s.NextRenewalDate.Date - today).Days
                })
                .ToList();

            var cancelledCount = await _context.Subscriptions
                .CountAsync(s => !s.IsActive && s.UserId == userId);

            return Ok(new
            {
                totalActiveSubscriptions = activeSubscriptions.Count,
                totalCancelledSubscriptions = cancelledCount,
                monthlyRecurringCost = Math.Round(monthlyCost, 2),
                annualRecurringCost = Math.Round(annualCost, 2),
                upcomingRenewals
            });
        }
    }
}
