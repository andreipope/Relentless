// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

public class BoardSpell : MonoBehaviour
{
    public TargetingArrow targetingArrow;

    private void OnDestroy()
    {
        if (targetingArrow != null)
        {
            Destroy(targetingArrow.gameObject);
            targetingArrow = null;
        }
    }
}