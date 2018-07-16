// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using LoomNetwork.CZB;
using LoomNetwork.CZB.Common;

public class BoardArrow : MonoBehaviour
{
    protected IGameplayManager _gameplayManager;

    public Action onTargetSelected;

    private GameObject _selfObject;

    protected GameObject _targetObjectsGroup,
                         _rootObjectsGroup,
                         _arrowObject,
                         _targetColliderObject;

    private Vector3 _fromPosition;

    private float _defaultArrowScale = 6.25f;

    protected bool startedDrag;

    protected BoardUnit boardCreature;

    public List<Enumerators.SkillTargetType> targetsType = new List<Enumerators.SkillTargetType>();

    public BoardUnit selectedCard { get; set; }
    public Player selectedPlayer { get; set; }


    private void Awake()
    {
        Init();
    }

    protected void Init()
    {
        _gameplayManager = GameClient.Get<IGameplayManager>();

        _selfObject = gameObject;

        _targetObjectsGroup = _selfObject.transform.Find("Group_TargetObjects").gameObject;
        _rootObjectsGroup = _selfObject.transform.Find("Group_RootObjects").gameObject;
        _arrowObject = _selfObject.transform.Find("Arrow").gameObject;
        _targetColliderObject = _selfObject.transform.Find("Target_Collider").gameObject;

        _targetObjectsGroup.SetActive(false);
    }

    protected virtual void Update()
    {
        if (startedDrag)
        {
            var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            UpdateLength(mousePosition);
        }
    }

    public void Begin(Vector2 from)
    {
        startedDrag = true;
        _fromPosition = from;
        _rootObjectsGroup.transform.position = _fromPosition;
        _arrowObject.transform.position = _fromPosition;
    }

    public void UpdateLength(Vector3 target)
    {
        _targetColliderObject.transform.position = target;

        float angle = Mathf.Atan2(target.y - _fromPosition.y, target.x - _fromPosition.x) * Mathf.Rad2Deg - 90;

        _arrowObject.transform.eulerAngles = new Vector3(0, 180, -angle);
        _rootObjectsGroup.transform.eulerAngles = new Vector3(0, 180, -angle + 21f);

        var scale = Vector3.Distance(_fromPosition, target) / _defaultArrowScale;

        _arrowObject.transform.localScale = new Vector3(1, scale, 1);
    }


    public virtual void OnCardSelected(BoardUnit creature)
    {
    }

    public virtual void OnCardUnselected(BoardUnit creature)
    {
    }

    public virtual void OnPlayerSelected(Player player)
    {
    }

    public virtual void OnPlayerUnselected(Player player)
    {
    }

    protected void CreateTarget(Vector3 target)
    {
        _targetObjectsGroup.transform.position = target;
        _targetObjectsGroup.SetActive(true);
    }
}