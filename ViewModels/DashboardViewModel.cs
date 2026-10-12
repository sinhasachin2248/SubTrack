
using SubTrack.Models;

namespace SubTrack.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalActiveSubscriptions { get; set; }

        public int TotalCancelledSubscriptions { get; set; }

        public decimal MonthlyRecurringCost { get; set; }

        public decimal AnnualRecurringCost { get; set; }

        public int DueTodayCount { get; set; }

        public int OverdueCount { get; set; }

        public int RenewalsThisMonthCount { get; set; }

        public DateTime? NextRenewalDate { get; set; }

        public string? NextRenewalName { get; set; }

        public CategorySpendingViewModel? TopCategory { get; set; }

        public decimal RenewalExposure { get; set; }

        public List<Subscription> UpcomingRenewals { get; set; }
            = new();

        public List<CategorySpendingViewModel> CategorySpending { get; set; }
            = new();
    }

    public class CategorySpendingViewModel
    {
        public string Category { get; set; } = string.Empty;

        public decimal MonthlyCost { get; set; }
    }
}
