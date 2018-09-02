// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DragableObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Action<GameObject> OnItemEndDrag;

    public bool dragOnSurfaces = true;

    public bool locked;

    private GameObject m_DraggingIcon;

    private RectTransform m_DraggingPlane;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (locked)
        
return;

        Canvas canvas = FindInParents<Canvas>(gameObject);
        if (canvas == null)
        
return;

        m_DraggingIcon = Instantiate(gameObject);
        m_DraggingIcon.transform.Find("Amount").gameObject.SetActive(false);
        Destroy(m_DraggingIcon.GetComponent<DragableObject>());
        m_DraggingIcon.transform.position = gameObject.transform.position;
        m_DraggingIcon.transform.localScale = Vector3.one;
        m_DraggingIcon.transform.SetParent(canvas.transform, false);
        m_DraggingIcon.transform.SetAsLastSibling();

        if (dragOnSurfaces)
        {
            m_DraggingPlane = transform as RectTransform;
        } else
        {
            m_DraggingPlane = canvas.transform as RectTransform;
        }

        SetDraggedPosition(eventData);
    }

    public void OnDrag(PointerEventData data)
    {
        if (locked)
        
return;

        if (m_DraggingIcon != null)
        {
            SetDraggedPosition(data);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (locked)
        
return;

        if (OnItemEndDrag != null)
        {
            OnItemEndDrag(m_DraggingIcon);
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
        if (locked)
        
return;

        if (dragOnSurfaces && (data.pointerEnter != null) && (data.pointerEnter.transform as RectTransform != null))
        {
            m_DraggingPlane = data.pointerEnter.transform as RectTransform;
        }

        RectTransform rt = m_DraggingIcon.GetComponent<RectTransform>();
        Vector3 globalMousePos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane, data.position, data.pressEventCamera, out globalMousePos))
        {
            rt.position = globalMousePos;
            rt.rotation = m_DraggingPlane.rotation;
        }
    }
}
