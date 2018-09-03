using DG.Tweening;
using LoomNetwork.CZB;
using LoomNetwork.CZB.Common;
using UnityEngine;
using UnityEngine.Rendering;

public class HandBoardCard
{
    public Player OwnerPlayer;

    public GameObject BoardZone;

    public bool Enabled = true;

    protected BoardCard CardView;

    protected bool StartedDrag;

    protected Vector3 InitialPos;

    protected Vector3 InitialRotation;

    private readonly IGameplayManager _gameplayManager;

    private readonly ISoundManager _soundManager;

    private readonly ITutorialManager _tutorialManager;

    private readonly PlayerController _playerController;

    private readonly CardsController _cardsController;

    private readonly OnBehaviourHandler _behaviourHandler;

    private bool _isHandCard = true;

    private bool _isReturnToHand;

    private bool _alreadySelected;

    private bool _canceledPlay;

    public HandBoardCard(GameObject selfObject, BoardCard boardCard)
    {
        GameObject = selfObject;

        CardView = boardCard;

        _gameplayManager = GameClient.Get<IGameplayManager>();
        _soundManager = GameClient.Get<ISoundManager>();
        _tutorialManager = GameClient.Get<ITutorialManager>();

        _playerController = _gameplayManager.GetController<PlayerController>();
        _cardsController = _gameplayManager.GetController<CardsController>();

        _behaviourHandler = GameObject.GetComponent<OnBehaviourHandler>();

        _behaviourHandler.MouseUpTriggered += MouseUp;
        _behaviourHandler.Updating += UpdatingHandler;
    }

    public Transform Transform => GameObject.transform;

    public GameObject GameObject { get; }

    public void UpdatingHandler(GameObject obj)
    {
        if (!Enabled)
            return;

        if (StartedDrag)
        {
            Transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 newPos = Transform.position;
            newPos.z = 0;
            Transform.position = newPos;

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                _canceledPlay = true;
                MouseUp(null);
            }

            if (BoardZone.GetComponent<BoxCollider2D>().bounds.Contains(Transform.position) && _isHandCard)
            {
                _cardsController.HoverPlayerCardOnBattleground(OwnerPlayer, CardView, this);
            }
            else
            {
                _cardsController.ResetPlayerCardsOnBattlegroundPosition();
            }
        }
    }

    public void OnSelected()
    {
        if (!Enabled)
            return;

        if (_playerController.IsActive && CardView.CanBePlayed(OwnerPlayer) && !_isReturnToHand && !_alreadySelected &&
            Enabled)
        {
            StartedDrag = true;
            InitialPos = Transform.position;
            InitialRotation = Transform.eulerAngles;

            Transform.eulerAngles = Vector3.zero;

            _playerController.IsCardSelected = true;
            _alreadySelected = true;
        }
    }

    public void CheckStatusOfHighlight()
    {
        if (CardView.CanBePlayed(OwnerPlayer) && CardView.CanBeBuyed(OwnerPlayer))
        {
            CardView.SetHighlightingEnabled(true);
        }
        else
        {
            CardView.SetHighlightingEnabled(false);
        }
    }

    public void MouseUp(GameObject obj)
    {
        if (!Enabled)
            return;

        if (!StartedDrag)
            return;

        _cardsController.ResetPlayerCardsOnBattlegroundPosition();

        _alreadySelected = false;
        StartedDrag = false;
        _playerController.IsCardSelected = false;

        bool playable = !_canceledPlay &&
            CardView.CanBeBuyed(OwnerPlayer) &&
            (CardView.WorkingCard.LibraryCard.CardKind != Enumerators.CardKind.CREATURE ||
                OwnerPlayer.BoardCards.Count < Constants.MaxBoardUnits);

        if (playable)
        {
            if (BoardZone.GetComponent<BoxCollider2D>().bounds.Contains(Transform.position) && _isHandCard)
            {
                _isHandCard = false;
                _cardsController.PlayPlayerCard(OwnerPlayer, CardView, this);
                CardView.SetHighlightingEnabled(false);
            }
            else
            {
                Transform.position = InitialPos;
                Transform.eulerAngles = InitialRotation;
                if (_tutorialManager.IsTutorial)
                {
                    _tutorialManager.ActivateSelectTarget();
                }
            }
        }
        else
        {
            _isReturnToHand = true;

            _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND, Constants.CardsMoveSoundVolume);

            Transform.DOMove(InitialPos, 0.5f).OnComplete(
                () =>
                {
                    Transform.position = InitialPos;
                    Transform.eulerAngles = InitialRotation;
                    _isReturnToHand = false;
                });
        }
    }

    public void ResetToInitialPosition()
    {
        Transform.position = InitialPos;
        Transform.eulerAngles = InitialRotation;
    }

    public void ResetToHandAnimation()
    {
        _canceledPlay = false;
        _alreadySelected = false;
        StartedDrag = false;
        _isReturnToHand = true;
        _isHandCard = true;
        Enabled = true;
        GameObject.GetComponent<SortingGroup>().sortingLayerName = Constants.LayerHandCards;
        GameObject.GetComponent<SortingGroup>().sortingOrder = 0;

        _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND, Constants.CardsMoveSoundVolume);

        Transform.DOMove(InitialPos, 0.5f).OnComplete(
            () =>
            {
                Transform.position = InitialPos;
                _isReturnToHand = false;
            });
    }
}
