// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

public class OpponentTargetingArrow : TargetingArrow
{
    protected override void Update()
    {
    }

    protected override void LateUpdate()
    {
        uvOffset += (uvAnimationRate * Time.deltaTime);
        lineRenderer.material.SetTextureOffset("_MainTex", uvOffset);
    }

    public void SetTarget(GameObject go)
    {
        var pos = go.transform.position;
        UpdateLength(pos);
        CreateTarget(pos);
    }
}