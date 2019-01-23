using Loom.ZombieBattleground.Common;
using System;
using System.Collections;
using System.Collections.Generic;
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

        private static Vector2 DefaultTextSize = new Vector3(3.2f, 1.4f);

        private GameObject _selfObject;

        private SpriteRenderer _currentBattleground;

        private TextMeshPro _textDescription;

        public int Id;

        public bool IsActiveInThisClick;

        public bool NotDestroyed => _selfObject != null;

        public float Width;

        public Enumerators.TutorialObjectOwner OwnerType;

        private Enumerators.TooltipAlign _align;

        private Vector3 _currentPosition;

        private BoardUnitView _ownerUnit;

        private bool _dynamicPosition;

        public TutorialDescriptionTooltipItem(int id, string description, Enumerators.TooltipAlign align, Enumerators.TutorialObjectOwner owner, int ownerId, Vector3 position, bool resizable, bool dynamicPosition)
        {
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();

            this.Id = id;
            OwnerType = owner;
            _align = align;
            _dynamicPosition = dynamicPosition;
            _currentPosition = position;

            _selfObject = MonoBehaviour.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Tutorials/TutorialDescriptionTooltip"));

            _textDescription = _selfObject.transform.Find("Text").GetComponent<TextMeshPro>();


            _textDescription.text = description;

            SetBattlegroundType(align);

            if (resizable && _currentBattleground != null)
            {
                _textDescription.autoSizeTextContainer = true;
                Vector2 textSize = _textDescription.GetPreferredValues(description);
                Vector2 backgroundSize = Vector2.one / DefaultTextSize * textSize;
                float value = (backgroundSize.x > backgroundSize.y ? backgroundSize.x : backgroundSize.y);
                _currentBattleground.transform.localScale = Vector3.one * value;
            }

            Width = _currentBattleground.bounds.size.x;

            if (ownerId > 0)
            {
                switch (owner)
                {
                    case Enumerators.TutorialObjectOwner.PlayerBattleframe:
                        _ownerUnit = _gameplayManager.CurrentPlayer.BoardCards.Find((x) =>
                            x.Model.TutorialObjectId == ownerId);
                        break;
                    case Enumerators.TutorialObjectOwner.EnemyBattleframe:
                        _ownerUnit = _gameplayManager.OpponentPlayer.BoardCards.Find((x) =>
                            x.Model.TutorialObjectId == ownerId);
                        break;
                    default: break;
                }
            }

            SetPosition();

            UpdatePosition();
        }

        public void UpdatePosition()
        {
            if(_dynamicPosition)
            {
                TutorialDescriptionTooltipItem tooltip;

                foreach (int index in _tutorialManager.CurrentTutorialStep.TutorialDescriptionTooltipsToActivate)
                {
                    if (index != Id)
                    {
                        tooltip = _tutorialManager.GetDescriptionTooltip(index);

                        if (tooltip == null)
                            continue;

                        if (Mathf.Abs(_selfObject.transform.position.x - tooltip._selfObject.transform.position.x) < (Width + tooltip.Width) / 2)
                        {
                            if (_align == Enumerators.TooltipAlign.CenterLeft)
                            {
                                SetBattlegroundType(Enumerators.TooltipAlign.CenterRight);
                                _currentPosition.x *= -1.2f;

                            }
                            else if (_align == Enumerators.TooltipAlign.CenterRight)
                            {
                                SetBattlegroundType(Enumerators.TooltipAlign.CenterLeft);
                                _currentPosition.x *= -0.9f;
                            }
                            SetPosition();
                            Helpers.InternalTools.DoActionDelayed(tooltip.UpdatePosition, Time.deltaTime);
                        }
                    }
                }
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
        }

        public void Hide()
        {
            if (_selfObject != null)
            {
                _selfObject.SetActive(false);
            }
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
            if(_selfObject != null && _selfObject.activeInHierarchy && _ownerUnit != null)
            {
                _selfObject.transform.position = _ownerUnit.Transform.TransformPoint(_currentPosition);
            }
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
                if (OwnerType == Enumerators.TutorialObjectOwner.IncorrectButton)
                {
                    _selfObject.transform.position -= Vector3.up * 2;
                }
            }
        }

        private void SetBattlegroundType(Enumerators.TooltipAlign align)
        {
            Vector3 size = Vector3.one;
            if(_currentBattleground != null)
            {
                _currentBattleground.gameObject.SetActive(false);
                size = _currentBattleground.transform.localScale;
            }

            switch (align)
            {
                case Enumerators.TooltipAlign.CenterLeft:
                case Enumerators.TooltipAlign.CenterRight:
                case Enumerators.TooltipAlign.TopMiddle:
                case Enumerators.TooltipAlign.BottomMiddle:
                    _currentBattleground = _selfObject.transform.Find("ArrowType/Arrow_" + align.ToString()).GetComponent<SpriteRenderer>();
                    _currentBattleground.gameObject.SetActive(true);
                    break;
                default:
                    throw new NotImplementedException(nameof(align) + " doesn't implemented");
            }

            _currentBattleground.transform.localScale = size;
        }
    }
}
