using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnChangeLayerHandler : MonoBehaviour
{
    public string Layer;

    public int OrderInLayer;

	void Start ()
    {
        gameObject.GetComponent<MeshRenderer>().sortingLayerName = Layer;
        gameObject.GetComponent<MeshRenderer>().sortingOrder = OrderInLayer;
    }
	
}
