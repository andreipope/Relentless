using System.Collections.Generic;
using UnityEngine;

public class GeneralColor : MonoBehaviour
{
    [SerializeField]
    private Color _color = Color.white;
    public Color Color
    {
        get => _color;
        set => _color = value;
    }

    public bool isUpdated = false;

    private List<Renderer> _rendererList;


    public void Init()
    {
        _rendererList = new List<Renderer>();
        GetRenderers();
    }
	

	void Update ()
    {
        if (isUpdated)
        {
            ChangeColor();
            if (_color.a == 0)
            {
                isUpdated = false;
                Hide();
            }
        }
    }

    private void GetRenderers()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            foreach (Renderer objectRenderer in transform.GetChild(i).GetComponentsInChildren<Renderer>())
            {
                _rendererList.Add(objectRenderer);
            }
        }

    }

    private void ChangeColor()
    {
        foreach (var item in _rendererList)
        {
            if (item is SpriteRenderer)
            {
                if ((item as SpriteRenderer).color.a >= _color.a)
                    (item as SpriteRenderer).color = _color;
            }
            else if (item is MeshRenderer)
            {
                (item as MeshRenderer).material.SetColor("_Color", _color);
            }
            else if (item is SkinnedMeshRenderer)
            {
                (item as SkinnedMeshRenderer).material.SetColor("_Color", _color);
            }

        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
