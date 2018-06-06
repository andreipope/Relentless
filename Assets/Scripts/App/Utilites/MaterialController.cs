using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialController : MonoBehaviour {

    private Material _instance;
    public MeshRenderer[] renderers;
    public Material sourceMaterial;


    private void Start()
    {
        _instance = Instantiate(sourceMaterial);

        foreach (var item in renderers)
            item.material = _instance;
    }

}
