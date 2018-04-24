// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

public class OpponentTargetingArrow : FightTargetingArrow
{
    private Vector3 _target = Vector3.zero;

    protected override void Update()
    {
        UpdateLength(_target);
    }

    protected override void LateUpdate()
    {
    }

    public void SetTarget(GameObject go)
    {
        _target = go.transform.position;
        _target.z = 0;

        UpdateLength(_target);
        CreateTarget(_target);
    }
}