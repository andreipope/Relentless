using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SideMenuButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Image _image;

    private void Awake()
    {
        _image = transform.Find("Image").GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _image.color = new Color(0.8f, 0.8f, 0.8f, 1f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _image.color = Color.white;
    }
}
