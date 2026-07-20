using ReviMarket.Web.Models;

namespace ReviMarket.Web.ViewModels;

public sealed class InboxIndexViewModel
{
    public List<ConversationViewModel> Conversations { get; set; } = new();

    public List<ApplicationUser> Contacts { get; set; } = new();
}

public sealed class ConversationViewModel
{
    public ApplicationUser User { get; set; } = new();

    public ChatMessage LatestMessage { get; set; } = new();

    public int MessagesCount { get; set; }
}

public sealed class ChatThreadViewModel
{
    public ApplicationUser CurrentUser { get; set; } = new();

    public ApplicationUser OtherUser { get; set; } = new();

    public List<ChatMessage> Messages { get; set; } = new();
}
