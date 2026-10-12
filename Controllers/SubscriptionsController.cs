
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SubTrack.Data;
using SubTrack.Models;

namespace SubTrack.Controllers
{
    [Authorize]
    public class SubscriptionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? search,
            string? category,
            string? status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var query = _context.Subscriptions.Where(s => s.UserId == userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.Name.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(s => s.Category == category);
            }

            if (status == "Active")
            {
                query = query.Where(s => s.IsActive);
            }
            else if (status == "Cancelled")
            {
                query = query.Where(s => !s.IsActive);
            }

            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Status = status;

            ViewBag.Categories = await _context.Subscriptions.Where(s => s.UserId == userId)
                .Select(s => s.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var subscriptions = await query
                .OrderBy(s => s.NextRenewalDate)
                .ToListAsync();

            return View(subscriptions);
        }

        // Display the create form
        public IActionResult Create()
        {
            return View(new Subscription
            {
                StartDate = DateTime.Today,
                NextRenewalDate = DateTime.Today.AddMonths(1)
            });
        }

        // Save a new subscription
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subscription subscription)
        {
            if (!ModelState.IsValid)
            {
                return View(subscription);
            }

            _context.Subscriptions.Add(subscription);
            subscription.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Subscription added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Display subscription details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (subscription == null)
                return NotFound();

            return View(subscription);
        }

        // Display the edit form
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (subscription == null)
                return NotFound();

            return View(subscription);
        }

        // Save subscription changes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Subscription subscription)
        {
            if (id != subscription.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(subscription);

            var existing = await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == id && s.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (existing == null)
                return NotFound();

            existing.Name = subscription.Name;
            existing.Category = subscription.Category;
            existing.Price = subscription.Price;
            existing.BillingCycle = subscription.BillingCycle;
            existing.StartDate = subscription.StartDate;
            existing.NextRenewalDate = subscription.NextRenewalDate;
            existing.IsActive = subscription.IsActive;
            existing.Notes = subscription.Notes;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Subscription updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Delete a subscription
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (subscription == null)
                return NotFound();

            return View(subscription);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subscription = await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == id && s.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (subscription != null)
            {
                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Subscription deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
