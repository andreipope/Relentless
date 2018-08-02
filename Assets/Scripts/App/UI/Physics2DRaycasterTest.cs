using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Physics2DRaycasterTest : Physics2DRaycaster {

    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList) {
        base.Raycast(eventData, resultAppendList);
    }
}
