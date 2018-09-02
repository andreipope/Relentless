using System;
using System.Collections.Generic;
using LoomNetwork.CZB;
using LoomNetwork.CZB.Common;
using UnityEngine;

public class BoardArrow : MonoBehaviour
{
    public List<Enumerators.SkillTargetType> TargetsType = new List<Enumerators.SkillTargetType>();

    public List<Enumerators.SetType> ElementType = new List<Enumerators.SetType>();

    protected IGameplayManager GameplayManager;

    protected BoardArrowController BoardArrowController;

    protected InputController InputController;

    protected GameObject TargetObjectsGroup, RootObjectsGroup, ArrowObject, TargetColliderObject;

    protected bool StartedDrag;

    private readonly float _defaultArrowScale = 6.25f;

    private GameObject _selfObject;

    private Vector3 _fromPosition, _targetPosition;

    private bool _isInverse = true;

    public BoardUnit SelectedCard { get; set; }

    public Player SelectedPlayer { get; set; }

    public bool IsDragging()
    {
        if (StartedDrag)
        {
            if (Vector3.Distance(_fromPosition, _targetPosition) > Constants.PointerMinDragDelta)
            {
                return true;
            }
        }

        return false;
    }

    public void SetInverse(bool isInverse = true)
    {
        int scaleX = 1;

        if (isInverse)
        {
            scaleX = -1;
        }

        _selfObject.transform.localScale = new Vector3(scaleX, 1, 1);
    }

    public void Begin(Vector2 from, bool isInverse = true)
    {
        _isInverse = isInverse;

        StartedDrag = true;
        _fromPosition = from;

        // _rootObjectsGroup.transform.position = _fromPosition;
        ArrowObject.transform.position = _fromPosition;

        SetInverse(isInverse);

        // if (this._isInverse)
        // _arrowObject.transform.localScale = new Vector3(-1, _arrowObject.transform.localScale.y, _arrowObject.transform.localScale.z);
    }

    public void UpdateLength(Vector3 target, bool isInverse = true)
    {
        TargetColliderObject.transform.position = target;
        TargetObjectsGroup.transform.position = target;

        _targetPosition = target;

        float angle = Mathf.Atan2(target.y - _fromPosition.y, target.x - _fromPosition.x) * Mathf.Rad2Deg - 90.5f;

        // float rootObjectsOffset = 21f;
        float scaleX = 1f;

        if (!isInverse)
        {
            scaleX = -1f;
        }

        ArrowObject.transform.eulerAngles = new Vector3(0, 180, -angle);

        // _rootObjectsGroup.transform.eulerAngles = new Vector3(0, 180, -angle + rootObjectsOffset);
        float scaleY = Vector3.Distance(_fromPosition, target) / _defaultArrowScale;

        ArrowObject.transform.localScale = new Vector3(scaleX, scaleY, ArrowObject.transform.localScale.z);
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

    public virtual void Dispose()
    {
        InputController.PlayerSelectingEvent -= PlayerSelectingEventHandler;
        InputController.UnitSelectingEvent -= UnitSelectingEventHandler;
        InputController.NoObjectsSelectedEvent -= NoObjectsSelectedEventHandler;

        ResetSelecting();

        Destroy(_selfObject);
    }

    protected void Init()
    {
        GameplayManager = GameClient.Get<IGameplayManager>();
        BoardArrowController = GameplayManager.GetController<BoardArrowController>();
        InputController = GameplayManager.GetController<InputController>();

        _selfObject = gameObject;

        TargetObjectsGroup = _selfObject.transform.Find("Group_TargetObjects").gameObject;
        RootObjectsGroup = _selfObject.transform.Find("Arrow/Group_RootObjects").gameObject;
        ArrowObject = _selfObject.transform.Find("Arrow").gameObject;
        TargetColliderObject = _selfObject.transform.Find("Target_Collider").gameObject;

        if (_isInverse)
        {
            _selfObject.transform.localScale = new Vector3(-1, 1, 1);
        }

        // _targetObjectsGroup.SetActive(false);
        InputController.PlayerSelectingEvent += PlayerSelectingEventHandler;
        InputController.UnitSelectingEvent += UnitSelectingEventHandler;
        InputController.NoObjectsSelectedEvent += NoObjectsSelectedEventHandler;
    }

    protected virtual void Update()
    {
        if (StartedDrag)
        {
            BoardArrowController.CurrentBoardArrow = this;
            BoardArrowController.SetStatusOfBoardArrowOnBoard(true);

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            UpdateLength(mousePosition, _isInverse);

            // CastRay(Input.mousePosition, 9);
        }
    }

    protected virtual void OnDestroy()
    {
        GameClient.Get<ITimerManager>().AddTimer(
            x =>
            {
                BoardArrowController.CurrentBoardArrow = null;
                BoardArrowController.SetStatusOfBoardArrowOnBoard(false);
            },
            null,
            0.25f);
    }

    private void Awake()
    {
        Init();
    }

    private void ResetSelecting()
    {
        if (SelectedCard != null)
        {
            if (SelectedCard.GameObject != null)
            {
                SelectedCard.SetSelectedUnit(false);
            }

            SelectedCard = null;
        }

        if (SelectedPlayer != null)
        {
            if (SelectedPlayer.AvatarObject != null)
            {
                SelectedPlayer.SetGlowStatus(false);
            }

            SelectedPlayer = null;
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
}
