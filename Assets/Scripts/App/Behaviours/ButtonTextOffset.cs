using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ButtonTextOffset : Button {

    public TextMeshProUGUI buttonText;
    public float textOffset;

    private float _textStartY;

    protected override void Awake()
    {
        _textStartY = buttonText.rectTransform.anchoredPosition.y;
        base.Awake();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        var vector = buttonText.rectTransform.anchoredPosition;
        vector.y += textOffset;
        buttonText.rectTransform.anchoredPosition = vector;

        base.OnPointerDown(eventData);
    }



    public override void OnPointerUp(PointerEventData eventData)
    {
        PointerExit();
        base.OnPointerUp(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        PointerExit();
        base.OnPointerExit(eventData);
    }

    private void PointerExit()
    {
        var vector = buttonText.rectTransform.anchoredPosition;
        vector.y = _textStartY;
        buttonText.rectTransform.anchoredPosition = vector;
    }
}
