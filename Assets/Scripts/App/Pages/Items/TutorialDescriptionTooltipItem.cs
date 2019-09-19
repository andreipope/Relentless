using DG.Tweening;
using log4net;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class TutorialDescriptionTooltipItem
    {
        private static readonly ILog Log = Logging.GetLog(nameof(TutorialDescriptionTooltipItem));

        private readonly ITutorialManager _tutorialManager;
        private readonly ILoadObjectsManager _loadObjectsManager;
        private readonly IGameplayManager _gameplayManager;
        private readonly BattlegroundController _battlegroundController;

        private const float KoefSize = 0.88f;

        private const float AdditionalInterval = 0.5f;

        private const float MinIntervalFromDifferentAlign = 1f;

        private static Vector2 DefaultTextSize = new Vector3(3.2f, 1.4f);

        private GameObject _selfObject;

        private GameObject _currentBackground;

        private TextMeshPro _textDescription;

        public int Id;

        public bool IsActiveInThisClick;

        public bool NotDestroyed => _selfObject != null;

        public Enumerators.TooltipAlign Align => _align;

        public int OwnerId => _ownerId;

        public float Width;

        public Enumerators.TutorialObjectOwner OwnerType;

        private Enumerators.TooltipAlign _align;

        private Vector3 _currentPosition;

        private BoardUnitView _ownerUnit;

        private BoardCardView _ownerCardInHand;

        private bool _dynamicPosition;

        private Enumerators.TutorialObjectLayer _layer = Enumerators.TutorialObjectLayer.Default;

        private bool _isDrawing;

        private int _ownerId;

        private bool _canBeClosed = false;

        private float _minimumShowTime;

        private Sequence _showingSequence;

        private string _tutorialUIElementOwnerName;

        public TutorialDescriptionTooltipItem(int id,
                                                string description,
                                                Enumerators.TooltipAlign align,
                                                Enumerators.TutorialObjectOwner owner,
                                                Vector3 position,
                                                bool resizable,
                                                bool dynamicPosition,
                                                int ownerId = 0,
                                                Enumerators.TutorialObjectLayer layer = Enumerators.TutorialObjectLayer.Default,
                                                IBoardObject boardObjectOwner = null,
                                                float minimumShowTime = Constants.DescriptionTooltipMinimumShowTime,
                                                string tutorialUIElementOwnerName = Constants.Empty)
        {
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

            this.Id = id;
            OwnerType = owner;
            _ownerId = ownerId;
            _align = align;
            _dynamicPosition = dynamicPosition;
            _currentPosition = position;
            _layer = layer;
            _minimumShowTime = minimumShowTime;
            _tutorialUIElementOwnerName = tutorialUIElementOwnerName;

            _selfObject = MonoBehaviour.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Tutorials/TutorialDescriptionTooltip"));

            _textDescription = _selfObject.transform.Find("Text").GetComponent<TextMeshPro>();


            description = description.Replace("\n", " ");

            _textDescription.text = description;

            SetBackgroundType(align);
            if (resizable && _currentBackground != null)
            {
                _textDescription.ForceMeshUpdate();                
                RectTransform rect = _textDescription.GetComponent<RectTransform>();
                Vector2 defaultSize = rect.sizeDelta;
                float koef = 1;
                while (rect.sizeDelta.y < _textDescription.renderedHeight)
                {
                    rect.sizeDelta = defaultSize * koef;
                    koef += Time.deltaTime;
                    _textDescription.ForceMeshUpdate();
                }
                Vector2 backgroundSize = Vector2.one / DefaultTextSize * rect.sizeDelta;
                float value = (backgroundSize.x > backgroundSize.y ? backgroundSize.x : backgroundSize.y);
                _currentBackground.transform.localScale = Vector3.one * value;
            }
            UpdateTextPosition();

            if (ownerId > 0)
            {
                CardModel cardModel = null;
                switch (OwnerType)
                {
                    case Enumerators.TutorialObjectOwner.PlayerBattleframe:
                        cardModel = _gameplayManager.CurrentPlayer.CardsOnBoard.First((x) =>
                            x.TutorialObjectId == ownerId);
                        break;
                    case Enumerators.TutorialObjectOwner.EnemyBattleframe:
                        cardModel = _gameplayManager.OpponentPlayer.CardsOnBoard.First((x) =>
                            x.TutorialObjectId == ownerId);
                        break;
                    case Enumerators.TutorialObjectOwner.PlayerCardInHand:
                        if (_ownerId != 0)
                        {
                            _ownerCardInHand =
                                _battlegroundController.GetCardViewByModel<BoardCardView>(
                                    _gameplayManager.CurrentPlayer.CardsInHand.FirstOrDefault(card => card.Card.TutorialObjectId == ownerId)
                                );
                        }
                        else if(_gameplayManager.CurrentPlayer.CardsInHand.Count > 0)
                        {
                            _ownerCardInHand =
                                _battlegroundController.GetCardViewByModel<BoardCardView>(_gameplayManager.CurrentPlayer.CardsInHand[0]);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (cardModel != null)
                {
                    _ownerUnit = _gameplayManager.GetController<BattlegroundController>().GetCardViewByModel<BoardUnitView>(cardModel);
                }
            }
            else if(boardObjectOwner != null)
            {
                switch(OwnerType)
                {
                    case Enumerators.TutorialObjectOwner.Battleframe:
                    case Enumerators.TutorialObjectOwner.EnemyBattleframe:
                    case Enumerators.TutorialObjectOwner.PlayerBattleframe:
                        _ownerUnit = _gameplayManager.GetController<BattlegroundController>().GetCardViewByModel<BoardUnitView>(boardObjectOwner as CardModel);
                        break;
                    case Enumerators.TutorialObjectOwner.HandCard:
                        break;

                }
            }

            SetPosition();

            UpdatePosition();

            _isDrawing = true;

            StartShowTimer();
        }

        public void UpdatePosition()
        {
            if(_dynamicPosition)
            {
                TutorialDescriptionTooltipItem tooltip;
                float distance = 0;

                foreach (int index in _tutorialManager.CurrentTutorialStep.TutorialDescriptionTooltipsToActivate)
                {
                    if (index != Id)
                    {
                        tooltip = _tutorialManager.GetDescriptionTooltip(index);

                        if (tooltip == null)
                            continue;

                        distance = Mathf.Abs(_selfObject.transform.position.x - tooltip._selfObject.transform.position.x);

                        if (tooltip.Align == Enumerators.TooltipAlign.CenterLeft ||
                            tooltip.Align == Enumerators.TooltipAlign.CenterRight)
                        {

                            if ((_align == tooltip.Align && distance < Width + AdditionalInterval) ||
                                (_align != tooltip.Align && distance < MinIntervalFromDifferentAlign))
                            {
                                _align = _align == Enumerators.TooltipAlign.CenterLeft ? Enumerators.TooltipAlign.CenterRight : Enumerators.TooltipAlign.CenterLeft;
                                SetBackgroundType(_align);
                                _currentPosition.x *= -1f;
                                SetPosition();
                                UpdateTextPosition();
                                Helpers.InternalTools.DoActionDelayed(tooltip.UpdatePosition, Time.deltaTime);
                            }
                        }
                        else
                        {
                            if(_selfObject.transform.position.x > tooltip._selfObject.transform.position.x)
                            {
                                _align = Enumerators.TooltipAlign.CenterLeft;
                                _currentPosition.x = Mathf.Abs(_currentPosition.x);
                            }
                            else
                            {
                                _align = Enumerators.TooltipAlign.CenterRight;
                                _currentPosition.x = -Mathf.Abs(_currentPosition.x);
                            }
                            SetBackgroundType(_align);
                            SetPosition();
                            UpdateTextPosition();
                        }
                    }
                }
            }

            switch (_layer)
            {
                case Enumerators.TutorialObjectLayer.Default:
                    _textDescription.renderer.sortingLayerName = "GameUI2";
                    UpdateBackgroundLayers("GameUI2", 1);
                    _textDescription.renderer.sortingOrder = 2;
                    break;
                case Enumerators.TutorialObjectLayer.AboveUI:
                    _textDescription.renderer.sortingLayerName = "GameplayInfo";
                    UpdateBackgroundLayers("GameplayInfo", 1);
                    _textDescription.renderer.sortingOrder = 2;
                    break;
                default:
                    _textDescription.renderer.sortingLayerName = "GameUI2";
                    UpdateBackgroundLayers("GameUI2", 0);
                    _textDescription.renderer.sortingOrder = 1;
                    break;
            }
        }

        private void UpdateBackgroundLayers(string name, int order)
        {
            foreach (SpriteRenderer child in _currentBackground.GetComponentsInChildren<SpriteRenderer>())
            {
                child.sortingLayerName = name;
                child.sortingOrder = order;
            }
        }

        public void Show(Vector3? position = null)
        {
            _selfObject?.SetActive(true);
            IsActiveInThisClick = true;
            if (position != null)
            {
                _currentPosition = (Vector3)position;
                SetPosition();
            }
            _isDrawing = true;
            UpdatePossibilityForClose();
        }

        public void Hide()
        {
            if (!_isDrawing || !_canBeClosed)
                return;

            _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.DescriptionTooltipClosed);

            _selfObject?.SetActive(false);

            _isDrawing = false;

            if(_showingSequence != null)
            {
                _showingSequence.Kill();
                _showingSequence = null;
            }
        }

        public void Dispose()
        {
            _isDrawing = false;
            if (_selfObject != null)
            {
                MonoBehaviour.Destroy(_selfObject);
            }
        }

        public void Update()
        {
            if (_isDrawing)
            {
                switch (OwnerType)
                {
                    case Enumerators.TutorialObjectOwner.Battleframe:
                    case Enumerators.TutorialObjectOwner.EnemyBattleframe:
                    case Enumerators.TutorialObjectOwner.PlayerBattleframe:
                        if (_ownerUnit != null && !_ownerUnit.Model.IsDead && _ownerUnit.GameObject != null && _ownerUnit.GameObject)
                        {
                            _selfObject.transform.position = _ownerUnit.Transform.TransformPoint(_currentPosition);
                        }
                        else if(_ownerId != 0)
                        {
                            UpdatePossibilityForClose();
                            Hide();
                        }
                        break;
                    case Enumerators.TutorialObjectOwner.HandCard:
                        break;
                    case Enumerators.TutorialObjectOwner.PlayerCardInHand:
                        if (_ownerCardInHand == null || !_ownerCardInHand.GameObject || _ownerCardInHand.GameObject == null)
                        {
                            UpdatePossibilityForClose();
                            Hide();
                            return;
                        }
                         
                        _selfObject.transform.position = _ownerCardInHand.Transform.TransformPoint(_currentPosition);
                        break;
                }
            }
        }

        private void StartShowTimer()
        {
            _canBeClosed = false;
            if (_minimumShowTime > 0f)
            {
                _showingSequence = InternalTools.DoActionDelayed(UpdatePossibilityForClose, _minimumShowTime);
            }
            else
            {
                UpdatePossibilityForClose();
            }
        }

        private void UpdatePossibilityForClose()
        {
            _canBeClosed = true;
        }

        private void UpdateTextPosition()
        {
            Vector3 centerOfChilds = Vector3.zero;
            SpriteRenderer[] childs = _currentBackground.GetComponentsInChildren<SpriteRenderer>();
            Width = childs[0].bounds.size.x * 4;

            foreach (SpriteRenderer child in childs)
            {
                centerOfChilds += child.transform.position;
            }
            centerOfChilds /= childs.Length;
            Vector3 textPosition = centerOfChilds;

            switch (_align)
            {
                case Enumerators.TooltipAlign.CenterRight:
                case Enumerators.TooltipAlign.CenterLeft:
                    textPosition.x *= 1.03f;
                    break;
                default:
                    break;
            }
            _textDescription.transform.position = textPosition;
        }

        private void SetPosition()
        {
            if (_ownerUnit != null)
            {
                _selfObject.transform.position = _ownerUnit.Transform.TransformPoint(_currentPosition);
            }
            else
            {
                if (OwnerType == Enumerators.TutorialObjectOwner.UI)
                {
                    GameObject ownerObject = GameObject.Find(_tutorialUIElementOwnerName);
                    if (ownerObject && ownerObject != null)
                    {
                        _currentPosition = ownerObject.transform.position + _currentPosition;
                    }
                }

                _selfObject.transform.position = _currentPosition;
            }           
        }

        private void SetBackgroundType(Enumerators.TooltipAlign align)
        {
            Vector3 size = Vector3.one;
            if(_currentBackground != null)
            {
                _currentBackground.gameObject.SetActive(false);
                size = _currentBackground.transform.localScale;
            }

            switch (align)
            {
                case Enumerators.TooltipAlign.CenterLeft:
                case Enumerators.TooltipAlign.CenterRight:
                case Enumerators.TooltipAlign.TopMiddle:
                case Enumerators.TooltipAlign.BottomMiddle:
                    _currentBackground = _selfObject.transform.Find("ArrowType/Arrow_" + align.ToString()).gameObject;
                    _currentBackground.gameObject.SetActive(true);
                    break;
                default:
                    Log.Warn($"Align {align} didnt implmented! Will use  default 'CenterLeft'");
                    SetBackgroundType(Enumerators.TooltipAlign.CenterLeft);
                    return;

            }

            _currentBackground.transform.localScale = size;
        }
    }
}
