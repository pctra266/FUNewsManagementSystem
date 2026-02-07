namespace BussinessLogic.Services
{
    public interface INotificationService
    {
        void AddNotification(string message);
        IEnumerable<NotificationDto> GetRecentNotifications(int count = 10);
    }

    public class NotificationDto
    {
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Id { get; set; } = string.Empty;
    }
}
