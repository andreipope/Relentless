using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using I2.Loc;
using TMPro;

using Loom.ZombieBattleground.Localization;

[ExecuteInEditMode]
public class ExtractUIPrefabToStructure : MonoBehaviour
{
    public bool apply = false;
    
    public Transform rootParent;
    
    void Update()
    {
        if(apply)
        {
            apply = false;

            if(rootParent == null)
            {
                rootParent = this.transform;
            }

            string log = "";
            Localize[] localizes = this.transform.GetComponentsInChildren<Localize>(true);
            foreach(Localize l in localizes)
            {
                string path = FindPath(l.transform);
                string term = l.Term;
                string font = string.IsNullOrEmpty(l.SecondaryTerm) ? "{###EMPTY###}" :  l.SecondaryTerm;

                log += $"{path},{term},{font}\n";                
            }

            Debug.Log(log);
            
            DestroyImmediate(this);
        }
    }
    
    private string FindPath(Transform thisTran)
    {
        Transform findParent = thisTran.parent;

        List<string> pathList = new List<string>();
        pathList.Add(thisTran.name);

        while (findParent != rootParent && findParent != null)
        {
            pathList.Add(findParent.name);
            findParent = findParent.parent;
        }

        pathList.Reverse();

        string log = "";
        while(pathList.Count > 0)
        {
            log += pathList[0];
            pathList.RemoveAt(0);
            if(pathList.Count > 0)
            {
                log += "/";
            }
        }

        return log;
    }
}
