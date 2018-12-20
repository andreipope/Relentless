using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public interface IFacebookManager
    {
        event Action<bool> OnFacebookInitEvent;

        FBUser FacebookUser { get; }
        bool IsLoggined { get; }
        bool IsInitialized { get; }


        void InitFacebook();

        void FeedShare(string title, string caption, string description, string uri, string imageUri);
    }
}
