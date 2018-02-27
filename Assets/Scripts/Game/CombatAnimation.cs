// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

using DG.Tweening;

public static class CombatAnimation
{
    public static void PlayFightAnimation(GameObject source, GameObject target, float shakeStrength, Action onHitCallback, Action onCompleteCallback = null)
    {
        var originalPos = source.transform.position;
        var sortingGroup = source.GetComponent<SortingGroup>();
        var oldSortingOrder = sortingGroup.sortingOrder;
        sortingGroup.sortingOrder = 1000;
        source.transform.DOMove(target.transform.position, 0.35f).SetEase(Ease.InSine).OnComplete(() =>
        {
            DOTween.Sequence()
                .Append(target.GetComponent<Image>().DOColor(Color.red, 0.25f))
                .Append(target.GetComponent<Image>().DOColor(Color.white, 0.25f))
                .Play();
            target.transform.DOShakePosition(1, new Vector3(shakeStrength, shakeStrength, 0));
            //target.transform.DOPunchPosition(new Vector3(0, 1, 0), 0.5f);
            source.transform.DOMove(originalPos, 0.5f).SetEase(Ease.OutSine).OnComplete(() =>
            {
                if (onCompleteCallback != null)
                {
                    onCompleteCallback();
                }

                sortingGroup.sortingOrder = oldSortingOrder;
            });

            if (onHitCallback != null)
            {
                onHitCallback();
            }
        });
    }
}