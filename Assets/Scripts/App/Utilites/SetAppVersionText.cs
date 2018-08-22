// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using LoomNetwork.CZB.Common;
using UnityEngine;
using UnityEngine.UI;

namespace LoomNetwork.CZB
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Text))]
    public class SetAppVersionText : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Text>().text =
                $"{Constants.CURRENT_VERSION_DEVELOPMENT_STAGE} {Constants.CURRENT_VERSION} {Constants.CURRENT_VERSION_POSTFIX}";
        }
    }
}