namespace ChatUI.Models
{
    public class MessageModel
    {
        public string User { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsMine { get; set; }
    }
}
