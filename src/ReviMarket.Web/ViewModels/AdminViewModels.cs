using ReviMarket.Web.Models;

namespace ReviMarket.Web.ViewModels;

public sealed class AdminDashboardViewModel
{
    public int UsersCount { get; set; }
    public int OrdersCount { get; set; }
    public int ProductsCount { get; set; }
    public int MessagesCount { get; set; }
    public int SupportCount { get; set; }
    public int PendingOrdersCount { get; set; }
    public int OpenReportsCount { get; set; }
    public int LockedUsersCount { get; set; }
    public int ActiveDealsCount { get; set; }
    public int DisputesCount { get; set; }

    public string[] Roles { get; set; } = UserRoles.All;
    public List<ApplicationUser> Users { get; set; } = new();
    public List<MarketItem> PendingOrders { get; set; } = new();
    public List<UserCase> UserCases { get; set; } = new();
    public List<OrderCase> OrderCases { get; set; } = new();
    public List<SupportRequest> SupportRequests { get; set; } = new();
}
