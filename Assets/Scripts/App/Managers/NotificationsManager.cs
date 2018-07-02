// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
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