using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LogPathInParent : MonoBehaviour
{    
    public bool apply = false;

    public Transform rootParent;

    void Update()
    {
        if (apply)
        {
            apply = false;


            Transform thisTran = this.transform;
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

            Debug.Log(log);

            DestroyImmediate(this);
        }
    }
}
