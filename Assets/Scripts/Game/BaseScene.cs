// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

public class BaseScene : MonoBehaviour
{
    public GameObject currentPopup { get; protected set; }

    [SerializeField]
    protected Canvas canvas;

    [SerializeField]
    protected CanvasGroup panelCanvasGroup;

    public void OpenPopup<T>(string name, Action<T> onOpened = null, bool darkenBackground = true) where T : Popup
    {
        StartCoroutine(OpenPopupAsync<T>(name, onOpened, darkenBackground));
    }

    public void ClosePopup()
    {
        if (currentPopup != null)
        {
            Destroy(currentPopup);
            currentPopup = null;
            panelCanvasGroup.blocksRaycasts = false;
            panelCanvasGroup.GetComponent<Image>().DOKill();
            panelCanvasGroup.GetComponent<Image>().DOFade(0.0f, 0.2f);
        }
    }

    protected IEnumerator OpenPopupAsync<T>(string name, Action<T> onOpened, bool darkenBackground) where T : Popup
    {
        var request = Resources.LoadAsync<GameObject>(name);
        while (!request.isDone)
        {
            yield return null;
        }

        currentPopup = Instantiate(request.asset) as GameObject;
        currentPopup.transform.SetParent(canvas.transform, false);
        currentPopup.GetComponent<Popup>().parentScene = this;
        if (darkenBackground)
        {
            panelCanvasGroup.blocksRaycasts = true;
            panelCanvasGroup.GetComponent<Image>().DOKill();
            panelCanvasGroup.GetComponent<Image>().DOFade(0.5f, 0.5f);
        }

        if (onOpened != null)
        {
            onOpened(currentPopup.GetComponent<T>());
        }
    }

    public void OnPopupClosed(Popup popup)
    {
        panelCanvasGroup.blocksRaycasts = false;
        panelCanvasGroup.GetComponent<Image>().DOKill();
        panelCanvasGroup.GetComponent<Image>().DOFade(0.0f, 0.25f);
    }
}
