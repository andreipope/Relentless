using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ButtonTextOffset : Button
{
    [FormerlySerializedAs("buttonText")]
    public TextMeshProUGUI ButtonText;

    [FormerlySerializedAs("textOffset")]
    public float TextOffset;

    private float _textStartY;

    public override void OnPointerDown(PointerEventData eventData)
    {
        Vector2 vector = ButtonText.rectTransform.anchoredPosition;
        vector.y += TextOffset;
        ButtonText.rectTransform.anchoredPosition = vector;

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
        _textStartY = ButtonText.rectTransform.anchoredPosition.y;
        base.Awake();
    }

    private void PointerExit()
    {
        Vector2 vector = ButtonText.rectTransform.anchoredPosition;
        vector.y = _textStartY;
        ButtonText.rectTransform.anchoredPosition = vector;
    }
}
