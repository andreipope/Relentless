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
    protected BoardArrowController _boardArrowController;
    protected InputController _inputController;

    public Action onTargetSelected;

    private GameObject _selfObject;

    protected GameObject _targetObjectsGroup,
                         _rootObjectsGroup,
                         _arrowObject,
                         _targetColliderObject;

    protected ParticleSystem _upBubbles;

    private Vector3 _fromPosition,
                    _targetPosition;

    private float _defaultArrowScale = 6.25f;

    private bool _isInverse = true;

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
        _boardArrowController = _gameplayManager.GetController<BoardArrowController>();
        _inputController = _gameplayManager.GetController<InputController>();

        _selfObject = gameObject;

        _targetObjectsGroup = _selfObject.transform.Find("Group_TargetObjects").gameObject;
        _rootObjectsGroup = _selfObject.transform.Find("Arrow/Group_RootObjects").gameObject;
        _arrowObject = _selfObject.transform.Find("Arrow").gameObject;
        _targetColliderObject = _selfObject.transform.Find("Target_Collider").gameObject;

        _upBubbles = _rootObjectsGroup.transform.Find("UpBubbles").GetComponent<ParticleSystem>();


        if (_isInverse)
            _selfObject.transform.localScale = new Vector3(-1, 1, 1);
        //  _targetObjectsGroup.SetActive(false);

        _boardArrowController.CurrentBoardArrow = this;
        _boardArrowController.SetStatusOfBoardArrowOnBoard(true);

        _inputController.PlayerSelectingEvent += PlayerSelectingEventHandler;
        _inputController.UnitSelectingEvent += UnitSelectingEventHandler;
        _inputController.NoObjectsSelectedEvent += NoObjectsSelectedEventHandler;
    }

    protected virtual void Update()
    {
        if (startedDrag)
        {
            var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            UpdateLength(mousePosition, _isInverse);


            //  CastRay(Input.mousePosition, 9);
        }
    }

    protected virtual void OnDestroy()
    {
        GameClient.Get<ITimerManager>().AddTimer((x) =>
        {
            _boardArrowController.CurrentBoardArrow = null;
            _boardArrowController.SetStatusOfBoardArrowOnBoard(false);
        }, null, 0.25f);
    }


    public bool IsDragging()
    {
        if (startedDrag)
        {
            if (Vector3.Distance(_fromPosition, _targetPosition) > Constants.POINTER_MIN_DRAG_DELTA)
                return true;
        }

        return false;
    }

    public void SetInverse(bool isInverse = true)
    {
        var scaleX = 1;

        if (isInverse)
            scaleX = -1;

        _selfObject.transform.localScale = new Vector3(scaleX, 1, 1);
    }

    public void Begin(Vector2 from, bool isInverse = true)
    {
        _isInverse = isInverse;

        startedDrag = true;
        _fromPosition = from;
        //   _rootObjectsGroup.transform.position = _fromPosition;
        _arrowObject.transform.position = _fromPosition;

        SetInverse(isInverse);

        //  if (this._isInverse)
        //     _arrowObject.transform.localScale = new Vector3(-1, _arrowObject.transform.localScale.y, _arrowObject.transform.localScale.z);
    }

    public void UpdateLength(Vector3 target, bool isInverse = true)
    {
        _targetColliderObject.transform.position = target;
        _targetObjectsGroup.transform.position = target;

        _targetPosition = target;

        float angle = Mathf.Atan2(target.y - _fromPosition.y, target.x - _fromPosition.x) * Mathf.Rad2Deg - 90.5f;
        // float rootObjectsOffset = 21f;

        var scaleX = 1f;

        if (!isInverse)
            scaleX = -1f;

        _arrowObject.transform.eulerAngles = new Vector3(0, 180, -angle);
        //  _rootObjectsGroup.transform.eulerAngles = new Vector3(0, 180, -angle + rootObjectsOffset);

        var scaleY = Vector3.Distance(_fromPosition, target) / _defaultArrowScale;


        _arrowObject.transform.localScale = new Vector3(scaleX, scaleY, _arrowObject.transform.localScale.z);
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
        //  _targetObjectsGroup.SetActive(true);
    }

    public void Dispose()
    {
        _inputController.PlayerSelectingEvent -= PlayerSelectingEventHandler;
        _inputController.UnitSelectingEvent -= UnitSelectingEventHandler;
        _inputController.NoObjectsSelectedEvent -= NoObjectsSelectedEventHandler;

        ResetSelecting();

        MonoBehaviour.Destroy(_selfObject);
    }


    private void ResetSelecting()
    {
        if (selectedCard != null)
        {
            if(selectedCard.gameObject != null)
                selectedCard.SetSelectedUnit(false);

            selectedCard = null;
        }

        if (selectedPlayer != null)
        {
            if (selectedPlayer.AvatarObject != null)
                selectedPlayer.SetGlowStatus(false);

            selectedPlayer = null;
        }
    }

    private void PlayerSelectingEventHandler(Player player)
    {
        OnPlayerSelected(player);
    }

    private void UnitSelectingEventHandler(BoardUnit unit)
    {
        OnCardSelected(unit);
    }

    private void NoObjectsSelectedEventHandler()
    {
        ResetSelecting();
    }

    private void CastRay(Vector3 origin, int layerMask)
    {
        Vector3 point = Camera.main.ScreenToWorldPoint(origin);

        Debug.DrawRay(point, Vector3.forward, Color.red);

        var hits = Physics2D.RaycastAll(point, Vector3.forward, Mathf.Infinity, 1 << layerMask);

        if (hits.Length > 0)
        {
            foreach (var hit in hits)
                CheckColliders(hit.collider);
        }

    }

    private void CheckColliders(Collider2D collider)
    {
        // check on units

        if (selectedCard != null)
        {
            selectedCard.SetSelectedUnit(false);
            selectedCard = null;
        }

        if (selectedPlayer != null)
        {
            selectedPlayer.SetGlowStatus(false);
            selectedPlayer = null;
        }

        foreach (var unit in _gameplayManager.CurrentPlayer.BoardCards)
        {
            if (unit.gameObject == collider.gameObject)
            {
                OnCardSelected(unit);
                break;
            }
        }


        foreach (var unit in _gameplayManager.OpponentPlayer.BoardCards)
        {
            if (unit.gameObject == collider.gameObject)
            {
                OnCardSelected(unit);
                break;
            }
        }

        // check on players

        if (_gameplayManager.CurrentPlayer.AvatarObject == collider.gameObject)
            OnPlayerSelected(_gameplayManager.CurrentPlayer);

        if (_gameplayManager.OpponentPlayer.AvatarObject == collider.gameObject)
            OnPlayerSelected(_gameplayManager.OpponentPlayer);
    }
}