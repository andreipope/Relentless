using DG.Tweening;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.Rendering;

public class HandBoardCard : OwnableBoardObject
{
    public GameObject BoardZone;

    public bool Enabled = true;

    public BoardCardView CardView { get; protected set; }

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

    public HandBoardCard(GameObject selfObject, BoardCardView boardCardView)
    {
        GameObject = selfObject;

        CardView = boardCardView;

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

    public override Player OwnerPlayer => CardView.BoardUnitModel.OwnerPlayer;

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
                _cardsController.HoverPlayerCardOnBattleground(OwnerPlayer, CardView);
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

            if (_gameplayManager.IsTutorial &&
                !_tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().
                SpecificBattlegroundInfo.DisabledInitialization)
            {
                if (CardView.CanBeBuyed(OwnerPlayer))
                {
                    if (!_tutorialManager.GetCurrentTurnInfo().PlayCardsSequence.Exists(info =>
                        info.TutorialObjectId == CardView.BoardUnitModel.Card.TutorialObjectId))
                    {
                        _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.PlayerOverlordTriedToPlayUnsequentionalCard);
                        return;
                    }

                    _tutorialManager.DeactivateSelectHandPointer(Enumerators.TutorialObjectOwner.PlayerCardInHand);
                }
            }

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

        if (_gameplayManager.IsGameEnded)
            return;

        _cardsController.ResetPlayerCardsOnBattlegroundPosition();

        _alreadySelected = false;
        StartedDrag = false;
        _playerController.IsCardSelected = false;

        bool playable = !_canceledPlay &&
            CardView.CanBeBuyed(OwnerPlayer) &&
            (CardView.BoardUnitModel.Card.Prototype.CardKind != Enumerators.CardKind.CREATURE ||
                OwnerPlayer.BoardCards.Count < OwnerPlayer.MaxCardsInPlay);

        if (playable)
        {
            if (BoardZone.GetComponent<BoxCollider2D>().bounds.Contains(Transform.position) && _isHandCard)
            {
                _isHandCard = false;
                _cardsController.PlayPlayerCard(OwnerPlayer, CardView, this, PlayCardOnBoard =>
                {
                    if (OwnerPlayer == _gameplayManager.CurrentPlayer)
                    {
                        PlayerMove playerMove = new PlayerMove(Enumerators.PlayerActionType.PlayCardOnBoard, PlayCardOnBoard);
                        _gameplayManager.PlayerMoves.AddPlayerMove(playerMove);
                    }
                });

                CardView.SetHighlightingEnabled(false);
            }
            else
            {
                ReturnToHandAnim();

                if(_tutorialManager.IsTutorial)
                {
                    _tutorialManager.ActivateSelectHandPointer(Enumerators.TutorialObjectOwner.PlayerCardInHand);
                }
            }
        }
        else
        {
            _canceledPlay = false;
            ReturnToHandAnim();

            if (_tutorialManager.IsTutorial)
            {
                _tutorialManager.ActivateSelectHandPointer(Enumerators.TutorialObjectOwner.PlayerCardInHand);
            }
        }
    }

    private void ReturnToHandAnim()
    {
        _isReturnToHand = true;
        _gameplayManager.CanDoDragActions = true;

        _soundManager.PlaySound(Enumerators.SoundType.CARD_FLY_HAND, Constants.CardsMoveSoundVolume);

        Transform.DOMove(InitialPos, 0.5f).OnComplete(
            () =>
            {
                Transform.position = InitialPos;
                Transform.eulerAngles = InitialRotation;
                _isReturnToHand = false;

                _gameplayManager.CanDoDragActions = true;
            });
    }

    public void ResetToInitialPosition()
    {
        _gameplayManager.CanDoDragActions = true;

        Transform.position = InitialPos;
        Transform.eulerAngles = InitialRotation;
    }

    public void ResetToHandAnimation()
    {
        _gameplayManager.CanDoDragActions = true;

        _canceledPlay = false;
        _alreadySelected = false;
        StartedDrag = false;
        _isReturnToHand = true;
        _isHandCard = true;
        Enabled = true;
        GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.HandCards;
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
