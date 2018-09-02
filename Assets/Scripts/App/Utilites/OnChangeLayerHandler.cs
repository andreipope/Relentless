// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using UnityEngine;

public class OnChangeLayerHandler : MonoBehaviour
{
    public string Layer;

    public int OrderInLayer;

    private void Start()
    {
        gameObject.GetComponent<MeshRenderer>().sortingLayerName = Layer;
        gameObject.GetComponent<MeshRenderer>().sortingOrder = OrderInLayer;
    }
}
