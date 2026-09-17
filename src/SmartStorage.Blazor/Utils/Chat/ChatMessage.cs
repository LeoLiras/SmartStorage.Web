using MudBlazor;

namespace SmartStorage.Blazor.Utils.Chat
{
    public class ChatMessage
    {
        public string Text { get; set; } = "";
        public bool IsUser { get; set; }
        public ChatBubblePosition Position => IsUser ? ChatBubblePosition.End : ChatBubblePosition.Start;
    }
}
