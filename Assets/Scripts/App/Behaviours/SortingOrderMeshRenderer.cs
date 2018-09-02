using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SortingOrderMeshRenderer : MonoBehaviour
{
    [SerializeField]
    private string _sortingLayer = "";

    [SerializeField]
    private int _orderInLayer;

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
