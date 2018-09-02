using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonTextOffset : Button
{
    public TextMeshProUGUI buttonText;

    public float textOffset;

    private float _textStartY;

    public override void OnPointerDown(PointerEventData eventData)
    {
        Vector2 vector = buttonText.rectTransform.anchoredPosition;
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

    protected override void Awake()
    {
        _textStartY = buttonText.rectTransform.anchoredPosition.y;
        base.Awake();
    }

    private void PointerExit()
    {
        Vector2 vector = buttonText.rectTransform.anchoredPosition;
        vector.y = _textStartY;
        buttonText.rectTransform.anchoredPosition = vector;
    }
}
