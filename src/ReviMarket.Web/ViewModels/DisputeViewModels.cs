using ReviMarket.Web.Models;

namespace ReviMarket.Web.ViewModels;

public sealed class DisputeIndexViewModel
{
    public List<DisputeQueueItemViewModel> Items { get; set; } = new();
    public int OpenCount { get; set; }
    public int InProgressCount { get; set; }
}

public sealed class DisputeQueueItemViewModel
{
    public Deal Deal { get; set; } = new();
    public OrderCase? Case { get; set; }
}
