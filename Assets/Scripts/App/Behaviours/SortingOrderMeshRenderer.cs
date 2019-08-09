using System;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SortingOrderMeshRenderer : MonoBehaviour
{
    [SerializeField]
    [SortingLayer]
    private int _sortingLayer;

    [SerializeField]

    private int _orderInLayer;
    private Renderer _renderer;

    private void OnEnable()
    {
        UpdateSorting();
    }

    private void OnValidate()
    {
        UpdateSorting();
    }

    private void UpdateSorting()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
        }

        _renderer.sortingLayerID = _sortingLayer;
        _renderer.sortingOrder = _orderInLayer;
    }
}
