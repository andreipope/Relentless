using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralColor : MonoBehaviour {


    [SerializeField]
    private Color _color = Color.white;
    public Color Color
    {
        get { return _color; }
        set
        {
            _color = value;
            ChangeColor();
        }
    }

    private List<Renderer> _rendererList;


    public void Init()
    {
        _rendererList = new List<Renderer>();
        GetRenderers();
        //_customMaterial = new Material(Shader.Find("Specular"));
    }
	

	void Update ()
    {
		
	}

    private void GetRenderers()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            foreach (Renderer objectRenderer in transform.GetChild(i).GetComponentsInChildren<Renderer>())
            {
                Debug.LogError(objectRenderer);
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
}
