using ReviMarket.Web.Models;

namespace ReviMarket.Web.ViewModels;

public sealed class ModerationDashboardViewModel
{
    public List<MarketItem> Orders { get; set; } = new();

    public List<ChatMessage> Messages { get; set; } = new();

    public List<ApplicationUser> Users { get; set; } = new();

    public List<SupportRequest> SupportRequests { get; set; } = new();
}
