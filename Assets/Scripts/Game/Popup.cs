// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

public class Popup : MonoBehaviour
{
    [HideInInspector]
    public BaseScene parentScene;

    public void Close()
    {
        if (parentScene != null)
        {
            parentScene.OnPopupClosed(this);
        }
        Destroy(gameObject);
    }
}