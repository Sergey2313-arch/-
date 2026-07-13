using ReviMarket.Web.Models;

namespace ReviMarket.Web.ViewModels;

public sealed class ModerationDashboardViewModel
{
    public List<MarketItem> Orders { get; set; } = new();

    public List<ChatMessage> Messages { get; set; } = new();

    public List<SupportMessage> SupportMessages { get; set; } = new();

    public List<ApplicationUser> Users { get; set; } = new();

    public List<UserCase> UserCases { get; set; } = new();

    public List<OrderCase> OrderCases { get; set; } = new();

    public List<SupportRequest> SupportRequests { get; set; } = new();
}

public sealed class CaseActionViewModel
{
    public int Id { get; set; }

    public string Action { get; set; } = string.Empty;

    public string Status { get; set; } = CaseStatuses.Open;
}
