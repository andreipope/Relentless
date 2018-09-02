using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SortingOrderMeshRenderer : MonoBehaviour
{
    [SerializeField]
    private readonly string _sortingLayer = "";

    [SerializeField]
    private readonly int _orderInLayer = 0;

    private void Start()
    {
        gameObject.GetComponent<Renderer>().sortingLayerName = _sortingLayer;
        gameObject.GetComponent<Renderer>().sortingOrder = _orderInLayer;
    }

    private void Update()
    {
        gameObject.GetComponent<Renderer>().sortingLayerName = _sortingLayer;
        gameObject.GetComponent<Renderer>().sortingOrder = _orderInLayer;
    }
}
