using System;
using GrandDevs.CZB.Common;

namespace GrandDevs.CZB
{
    public class NotificationManager : IService, INotificationManager
    {
        public event Action<Notification> DrawNotificationEvent;

        public void Dispose()
        {

        }

        public void Init()
        {
        }

        public void Update()
        {

        }


        public void DrawNotification(Enumerators.NotificationType type, string message)
        {
            Notification notification = new Notification()
            {
                type = type,
                message = message
            };

            if (DrawNotificationEvent != null)
                DrawNotificationEvent(notification);
        }
    }

    public class Notification
    {
        public string message;

        public Enumerators.NotificationType type;
    }
}