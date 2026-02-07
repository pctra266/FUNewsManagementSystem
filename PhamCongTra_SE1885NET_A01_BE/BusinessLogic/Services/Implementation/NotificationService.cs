using System.Collections.Concurrent;

namespace BussinessLogic.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ConcurrentQueue<NotificationDto> _notifications = new();
        private readonly int _maxNotifications = 10;

        public void AddNotification(string message)
        {
            var notification = new NotificationDto
            {
                Id = Guid.NewGuid().ToString(),
                Message = message,
                Timestamp = DateTime.Now
            };

            _notifications.Enqueue(notification);

            // Keep only the last 10 notifications
            while (_notifications.Count > _maxNotifications)
            {
                _notifications.TryDequeue(out _);
            }
        }

        public IEnumerable<NotificationDto> GetRecentNotifications(int count = 10)
        {
            return _notifications
                .OrderByDescending(n => n.Timestamp)
                .Take(count)
                .ToList();
        }
    }
}
