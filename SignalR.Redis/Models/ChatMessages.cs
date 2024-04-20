namespace SignalR.Redis.Models
{
    public class ChatMessages
    {
        public string UserName { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
