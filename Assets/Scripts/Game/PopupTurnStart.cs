// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;

using UnityEngine;

using DG.Tweening;

public class PopupTurnStart : Popup
{
    private void Start()
    {
        gameObject.transform.DOScale(1.0f, 0.4f).SetEase(Ease.InOutBack);
        StartCoroutine(AutoClose());
    }

    private IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(2.0f);
        var sequence = DOTween.Sequence();
        sequence.Append(gameObject.transform.DOScale(0.0f, 0.2f).SetEase(Ease.OutCubic));
        sequence.OnComplete(() => { Close(); });
    }
}