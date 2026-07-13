using ReviMarket.Web.Models;

namespace ReviMarket.Web.ViewModels;

public sealed class SupportIndexViewModel
{
    public List<SupportRequest> Requests { get; set; } = new();
}

public sealed class SupportQueueViewModel
{
    public List<SupportRequest> Requests { get; set; } = new();
}

public sealed class SupportDetailsViewModel
{
    public SupportRequest Request { get; set; } = new();
    public List<SupportMessage> Messages { get; set; } = new();
}
