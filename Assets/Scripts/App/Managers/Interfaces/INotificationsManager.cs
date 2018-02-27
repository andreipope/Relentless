using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GrandDevs.CZB.Common;

namespace GrandDevs.CZB
{
    public interface INotificationManager
    {
        event Action<Notification> DrawNotificationEvent;

        void DrawNotification(Enumerators.NotificationType type, string message);

    }
}