using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using I2.Loc;
using TMPro;

[ExecuteInEditMode]
public class AddLocalizedComponent : MonoBehaviour
{
    public bool apply = false;

    public string term = "";

    public bool isAutoSize = true;
    
    public enum FontType
    {
        None,
        Bevan,
        Fira
    }

    public FontType fontType = FontType.Bevan;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(apply)
        {
            apply = false;

            Localize l = this.gameObject.AddComponent<Localize>();

            if (string.IsNullOrEmpty(term))
            {
                l.LocalizeOnAwake = false;
                l.AllowLocalizedParameters = false;
            }
            else
            {
                l.Term = term;
            }            
            
            if (fontType != FontType.None)
            {
                if (fontType == FontType.Bevan)
                    l.SecondaryTerm = "_HeaderFonts";
                if (fontType == FontType.Fira)
                    l.SecondaryTerm = "_BodyFonts";
            }
            
            if( isAutoSize )
            {
                TextMeshProUGUI text = gameObject.GetComponent<TextMeshProUGUI>();
                text.enableAutoSizing = true;
            }

            DestroyImmediate(this);
        }
    }
}
