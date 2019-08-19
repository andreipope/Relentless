using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using I2.Loc;
using TMPro;

using Loom.ZombieBattleground.Localization;

[ExecuteInEditMode]
public class AddLocalizedFontSettings : MonoBehaviour
{
    public bool apply = false;
    
    void Update()
    {
        if(apply)
        {
            apply = false;

            int count = 0;
            Localize[] localizes = this.transform.GetComponentsInChildren<Localize>(true);
            foreach(Localize l in localizes)
            {
                LocalizationFontSettings f = l.transform.GetComponent<LocalizationFontSettings>();
                if( f == null )
                {
                    f = l.gameObject.AddComponent<LocalizationFontSettings>();
                    ++count;
                }
            }

            Debug.Log($"LocalizationFontSettings: {count}");
            
            DestroyImmediate(this);
        }
    }
}
