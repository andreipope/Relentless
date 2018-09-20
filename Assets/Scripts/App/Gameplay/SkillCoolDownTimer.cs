using System.Collections.Generic;
using Loom.ZombieBattleground;
using UnityEngine;

public class SkillCoolDownTimer
{
    private readonly GameObject SelfObject;
    private int _coolDown;
    private float _angleSegment;
    private const float HalfCircleAngle = 180;

    private readonly ILoadObjectsManager _loadObjectsManager;
    private float _angleSegmentOffset = 5f;

    private List<GameObject> _gapList = new List<GameObject>();

    public SkillCoolDownTimer(GameObject obj, int coolDown)
    {
        _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
        SelfObject = obj.transform.Find("CoolDown_Timer/CoolDown_Timer_On").gameObject;
        Reset(coolDown);
    }

    public void SetAngle(int turn, bool isSetInitialCoolDown = false)
    {
        if (isSetInitialCoolDown)
            Reset(turn);

        int current = _coolDown - turn;
        float angle = current != 0 ? _angleSegment * current : 0;
        angle = angle < HalfCircleAngle ? angle + _angleSegmentOffset : angle;
        SelfObject.transform.eulerAngles = new Vector3(0, 0, angle);
    }

    private void Reset(int coolDown)
    {
        SelfObject.transform.eulerAngles = Vector3.zero;
        _coolDown = coolDown;
        _angleSegment = HalfCircleAngle / _coolDown;

        //Create gaps
        DestroyGaps();
        float gapAngle = _angleSegment;
        _gapList = new List<GameObject>();
        for (int i = 0; i < _coolDown - 1; i++)
        {
            GameObject gapObj = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/SkillCoolDown_Gap"));
            gapObj.transform.eulerAngles = new Vector3(0, 0, gapAngle);
            gapAngle += _angleSegment;
            gapObj.transform.SetParent(SelfObject.transform);
            gapObj.transform.localPosition = Vector3.zero;
            _gapList.Add(gapObj);
        }
    }

    private void DestroyGaps()
    {
        for (int i = _gapList.Count-1; i >= 0; i--)
        {
            Object.Destroy(_gapList[i]);
        }
    }


}
