using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DragableObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Action<GameObject> OnItemEndDrag;

    public bool DragOnSurfaces = true;

    public bool Locked;

    private GameObject _mDraggingIcon;

    private RectTransform _mDraggingPlane;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Locked)

            return;

        Canvas canvas = FindInParents<Canvas>(gameObject);
        if (canvas == null)

            return;

        _mDraggingIcon = Instantiate(gameObject);
        _mDraggingIcon.transform.Find("Amount").gameObject.SetActive(false);
        Destroy(_mDraggingIcon.GetComponent<DragableObject>());
        _mDraggingIcon.transform.position = gameObject.transform.position;
        _mDraggingIcon.transform.localScale = Vector3.one;
        _mDraggingIcon.transform.SetParent(canvas.transform, false);
        _mDraggingIcon.transform.SetAsLastSibling();

        if (DragOnSurfaces)
        {
            _mDraggingPlane = transform as RectTransform;
        }
        else
        {
            _mDraggingPlane = canvas.transform as RectTransform;
        }

        SetDraggedPosition(eventData);
    }

    public void OnDrag(PointerEventData data)
    {
        if (Locked)

            return;

        if (_mDraggingIcon != null)
        {
            SetDraggedPosition(data);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (Locked)

            return;

        if (OnItemEndDrag != null)
        {
            OnItemEndDrag(_mDraggingIcon);
        }

        // if (m_DraggingIcon != null)
        // Destroy(m_DraggingIcon);
    }

    public static T FindInParents<T>(GameObject go)
        where T : Component
    {
        if (go == null)
        {
            return null;
        }

        T comp = go.GetComponent<T>();

        if (comp != null)
        {
            return comp;
        }

        Transform t = go.transform.parent;
        while ((t != null) && (comp == null))
        {
            comp = t.gameObject.GetComponent<T>();
            t = t.parent;
        }

        return comp;
    }

    private void SetDraggedPosition(PointerEventData data)
    {
        if (Locked)

            return;

        if (DragOnSurfaces && (data.pointerEnter != null) && (data.pointerEnter.transform as RectTransform != null))
        {
            _mDraggingPlane = data.pointerEnter.transform as RectTransform;
        }

        RectTransform rt = _mDraggingIcon.GetComponent<RectTransform>();
        Vector3 globalMousePos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_mDraggingPlane, data.position, data.pressEventCamera, out globalMousePos))
        {
            rt.position = globalMousePos;
            rt.rotation = _mDraggingPlane.rotation;
        }
    }
}
