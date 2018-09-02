using DG.Tweening;
using LoomNetwork.CZB;
using LoomNetwork.CZB.Common;
using UnityEngine;
using UnityEngine.Rendering;

public class HandBoardCard
{
    public Player ownerPlayer;

    public GameObject boardZone;

    public bool enabled = true;

    protected BoardCard cardView;

    protected bool startedDrag;

    protected Vector3 initialPos;

    protected Vector3 initialRotation;

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

    private int _handInd;

    public HandBoardCard(GameObject selfObject, BoardCard boardCard)
    {
        gameObject = selfObject;

        cardView = boardCard;

        _handInd = GetHashCode();

        _gameplayManager = GameClient.Get<IGameplayManager>();
        _soundManager = GameClient.Get<ISoundManager>();
        _tutorialManager = GameClient.Get<ITutorialManager>();

        _playerController = _gameplayManager.GetController<PlayerController>();
        _cardsController = _gameplayManager.GetController<CardsController>();

        _behaviourHandler = gameObject.GetComponent<OnBehaviourHandler>();

        _behaviourHandler.OnMouseUpEvent += OnMouseUp;
        _behaviourHandler.OnUpdateEvent += OnUpdateEventHandler;
    }

    public Transform transform => gameObject.transform;

    public GameObject gameObject { get; }

    public void OnUpdateEventHandler(GameObject obj)
    {
        if (!enabled)

            return;

        if (startedDrag)
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 newPos = transform.position;
            newPos.z = 0;
            transform.position = newPos;

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                _canceledPlay = true;
                OnMouseUp(null);
            }

            if (boardZone.GetComponent<BoxCollider2D>().bounds.Contains(transform.position) && _isHandCard)
            {
                _cardsController.HoverPlayerCardOnBattleground(ownerPlayer, cardView, this);
            } else
            {
                _cardsController.ResetPlayerCardsOnBattlegroundPosition();
            }
        }
    }

    public void OnSelected()
    {
        if (!enabled)

            return;

        if (_playerController.IsActive && cardView.CanBePlayed(ownerPlayer) && !_isReturnToHand && !_alreadySelected && enabled)
        {
            startedDrag = true;
            initialPos = transform.position;
            initialRotation = transform.eulerAngles;

            transform.eulerAngles = Vector3.zero;

            _playerController.IsCardSelected = true;
            _alreadySelected = true;
        }
    }

    public void CheckStatusOfHighlight()
    {
        if (cardView.CanBePlayed(ownerPlayer) && cardView.CanBeBuyed(ownerPlayer))
        {
            cardView.SetHighlightingEnabled(true);
        } else
        {
            cardView.SetHighlightingEnabled(false);
        }
    }

    public void OnMouseUp(GameObject obj)
    {
        if (!enabled)

            return;

        if (!startedDrag)

            return;

        _cardsController.ResetPlayerCardsOnBattlegroundPosition();

        _alreadySelected = false;
        startedDrag = false;
        _playerController.IsCardSelected = false;

        bool playable = true;
        if (_canceledPlay || !cardView.CanBeBuyed(ownerPlayer) || ((cardView.WorkingCard.libraryCard.cardKind == Enumerators.CardKind.CREATURE) && (ownerPlayer.BoardCards.Count >= Constants.MAX_BOARD_UNITS)))
        {
            playable = false;
        }

        if (playable)
        {
            if (boardZone.GetComponent<BoxCollider2D>().bounds.Contains(transform.position) && _isHandCard)
            {
                _isHandCard = false;
                _cardsController.PlayPlayerCard(ownerPlayer, cardView, this);
                cardView.SetHighlightingEnabled(false);
            } else
            {
                transform.position = initialPos;
                transform.eulerAngles = initialRotation;
                if (_tutorialManager.IsTutorial)
                {
                    _tutorialManager.ActivateSelectTarget();
                }
            }
        } else
        {
            _isReturnToHand = true;

            _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);

            transform.DOMove(initialPos, 0.5f).OnComplete(
                () =>
                {
                    transform.position = initialPos;
                    transform.eulerAngles = initialRotation;
                    _isReturnToHand = false;
                });
        }
    }

    public void ResetToInitialPosition()
    {
        transform.position = initialPos;
        transform.eulerAngles = initialRotation;
    }

    public void ResetToHandAnimation()
    {
        _canceledPlay = false;
        _alreadySelected = false;
        startedDrag = false;
        _isReturnToHand = true;
        _isHandCard = true;
        enabled = true;
        gameObject.GetComponent<SortingGroup>().sortingLayerName = Constants.LAYER_HAND_CARDS;
        gameObject.GetComponent<SortingGroup>().sortingOrder = 0;

        _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND, Constants.CARDS_MOVE_SOUND_VOLUME, false, false);

        transform.DOMove(initialPos, 0.5f).OnComplete(
            () =>
            {
                transform.position = initialPos;
                _isReturnToHand = false;
            });
    }
}
