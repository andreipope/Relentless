// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public interface INotificationManager
    {
        event Action<Notification> DrawNotificationEvent;

        void DrawNotification(Enumerators.NotificationType type, string message);

    }
}