using Loom.ZombieBattleground.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class TutorialDescriptionTooltipItem
    {
        private readonly ITutorialManager _tutorialManager;
        private readonly ILoadObjectsManager _loadObjectsManager;
        private readonly IGameplayManager _gameplayManager;

        private const float KoefSize = 0.88f;

        private const float AdditionalInterval = 0.5f;

        private const float MinIntervalFromDifferentAlign = 1f;

        private static Vector2 DefaultTextSize = new Vector3(3.2f, 1.4f);

        private GameObject _selfObject;

        private SpriteRenderer _currentBackground;

        private TextMeshPro _textDescription;

        public int Id;

        public bool IsActiveInThisClick;

        public bool NotDestroyed => _selfObject != null;

        public Enumerators.TooltipAlign Align => _align;

        public float Width;

        public Enumerators.TutorialObjectOwner OwnerType;

        private Enumerators.TooltipAlign _align;

        private Vector3 _currentPosition;

        private BoardUnitView _ownerUnit;

        private bool _dynamicPosition;

        private Enumerators.TutorialObjectLayer _layer = Enumerators.TutorialObjectLayer.Default;

        private bool _isDrawing;

        public TutorialDescriptionTooltipItem(int id,
                                                string description,
                                                Enumerators.TooltipAlign align,
                                                Enumerators.TutorialObjectOwner owner,
                                                Vector3 position,
                                                bool resizable,
                                                bool dynamicPosition,
                                                int ownerId = 0,
                                                Enumerators.TutorialObjectLayer layer = Enumerators.TutorialObjectLayer.Default,
                                                BoardObject boardObjectOwner = null)
        {
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();

            this.Id = id;
            OwnerType = owner;
            _align = align;
            _dynamicPosition = dynamicPosition;
            _currentPosition = position;
            _layer = layer;

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
            Width = _currentBackground.bounds.size.x;

            if (ownerId > 0)
            {
                switch (OwnerType)
                {
                    case Enumerators.TutorialObjectOwner.PlayerBattleframe:
                        _ownerUnit = _gameplayManager.CurrentPlayer.BoardCards.First((x) =>
                            x.Model.TutorialObjectId == ownerId);
                        break;
                    case Enumerators.TutorialObjectOwner.EnemyBattleframe:
                        _ownerUnit = _gameplayManager.OpponentPlayer.BoardCards.First((x) =>
                            x.Model.TutorialObjectId == ownerId);
                        break;
                    default: break;
                }
            }
            else if(boardObjectOwner != null)
            {
                switch(OwnerType)
                {
                    case Enumerators.TutorialObjectOwner.Battleframe:
                    case Enumerators.TutorialObjectOwner.EnemyBattleframe:
                    case Enumerators.TutorialObjectOwner.PlayerBattleframe:
                        _ownerUnit = _gameplayManager.GetController<BattlegroundController>().GetBoardUnitViewByModel(boardObjectOwner as BoardUnitModel);
                        break;
                    case Enumerators.TutorialObjectOwner.HandCard:
                        break;

                }
            }

            SetPosition();

            UpdatePosition();

            _isDrawing = true;
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

                        if ((_align == tooltip.Align && distance < Width + AdditionalInterval) ||
                            (_align != tooltip.Align && distance < MinIntervalFromDifferentAlign))
                        {
                            _align = _align == Enumerators.TooltipAlign.CenterLeft ? Enumerators.TooltipAlign.CenterRight : Enumerators.TooltipAlign.CenterLeft;
                            SetBackgroundType(_align);
                            _currentPosition.x *= -1f;
                            UpdateTextPosition();
                            SetPosition();
                            Helpers.InternalTools.DoActionDelayed(tooltip.UpdatePosition, Time.deltaTime);
                        }
                    }
                }
            }

            switch (_layer)
            {
                case Enumerators.TutorialObjectLayer.Default:
                    _textDescription.renderer.sortingLayerName = SRSortingLayers.GameUI2;
                    _currentBackground.sortingLayerName = SRSortingLayers.GameUI2;
                    _currentBackground.sortingOrder = 1;
                    _textDescription.renderer.sortingOrder = 2;
                    break;
                case Enumerators.TutorialObjectLayer.AboveUI:
                    _textDescription.renderer.sortingLayerName = SRSortingLayers.GameUI3;
                    _currentBackground.sortingLayerName = SRSortingLayers.GameUI3;
                    _currentBackground.sortingOrder = 1;
                    _textDescription.renderer.sortingOrder = 2;
                    break;
                default:
                    _textDescription.renderer.sortingLayerName = SRSortingLayers.GameUI2;
                    _currentBackground.sortingLayerName = SRSortingLayers.GameUI2;
                    _currentBackground.sortingOrder = 0;
                    _textDescription.renderer.sortingOrder = 1;
                    break;
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
        }

        public void Hide()
        {
            _selfObject?.SetActive(false);

            _isDrawing = false;
        }

        public void Dispose()
        {
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
                        if (_ownerUnit != null)
                        {
                            _selfObject.transform.position = _ownerUnit.Transform.TransformPoint(_currentPosition);
                        }
                        break;
                    case Enumerators.TutorialObjectOwner.HandCard:
                        break;
                }
            }
        }

        private void UpdateTextPosition()
        {
            Vector3 textPosition = Vector3.zero;
            switch (_align)
            {
                case Enumerators.TooltipAlign.TopMiddle:
                    textPosition.y = -_currentBackground.bounds.size.y * 0.52f;
                    break;
                case Enumerators.TooltipAlign.CenterLeft:
                    textPosition.x = _currentBackground.bounds.size.x * 0.51f;
                    break;
                case Enumerators.TooltipAlign.CenterRight:
                    textPosition.x = -_currentBackground.bounds.size.x * 0.51f;
                    break;
                case Enumerators.TooltipAlign.BottomMiddle:
                    textPosition.y = _currentBackground.bounds.size.y * 0.52f;
                    break;
                default:
                    break;
            }
            _textDescription.transform.localPosition = textPosition;
        }

        private void SetPosition()
        {
            if (_ownerUnit != null)
            {
                _selfObject.transform.position = _ownerUnit.Transform.TransformPoint(_currentPosition);
            }
            else
            {
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
                    _currentBackground = _selfObject.transform.Find("ArrowType/Arrow_" + align.ToString()).GetComponent<SpriteRenderer>();
                    _currentBackground.gameObject.SetActive(true);
                    break;
                default:
                    throw new NotImplementedException(nameof(align) + " doesn't implemented");
            }

            _currentBackground.transform.localScale = size;
        }
    }
}
