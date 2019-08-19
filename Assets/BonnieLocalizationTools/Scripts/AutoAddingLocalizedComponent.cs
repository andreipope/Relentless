using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using I2.Loc;
using TMPro;
using Loom.ZombieBattleground.Localization;

[ExecuteInEditMode]
public class AutoAddingLocalizedComponent : MonoBehaviour
{
    public bool apply = false;

    public TextAsset textData;

    public GameObject UIObject;
    
    void Update()
    {
        if(apply)
        {
            apply = false;

            if (UIObject == null)
                UIObject = this.gameObject;

            string[] lines = textData.ToString().Split('\n');
            
            foreach(string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;
                    
                string[] words = line.Split(',');
                if (words.Length > 0)
                {
                    GameObject obj; 
                    try
                    {
                        obj = UIObject.transform.Find(words[0]).gameObject;
                    }
                    catch
                    {
                        Debug.LogError($"Not found {words[0]}");
                        continue;
                    }
                    AddLocalizeComponent
                    (
                        obj,
                        words.Length > 1 ? words[1] : "",
                        words.Length > 2 ? words[2] : ""
                    );
                }
            }
            
            DestroyImmediate(this);
        }
    }
    
    private void AddLocalizeComponent(GameObject obj, string term = "", string font = "")
    {
        if (obj == null)
            return;
            
        Localize l = obj.GetComponent<Localize>();
        if (l == null)
        {
            l = obj.AddComponent<Localize>();
        }

        if (string.IsNullOrEmpty(term))
        {
            l.LocalizeOnAwake = false;
            l.AllowLocalizedParameters = false;
        }
        else
        {
            l.Term = term;
        }

        l.SecondaryTerm = font;
        
        if(obj.GetComponent<LocalizationFontSettings>() == null)
            obj.AddComponent<LocalizationFontSettings>();
    }
}
