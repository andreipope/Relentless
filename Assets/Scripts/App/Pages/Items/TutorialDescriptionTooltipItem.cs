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

        public Enumerators.TutorialObjectOwner OwnerType;

        public TutorialDescriptionTooltipItem(int id, string description, Enumerators.TooltipAlign align, Enumerators.TutorialObjectOwner owner, int ownerId, Vector3 position, bool resizable)
        {
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();

            this.Id = id;
            OwnerType = owner;

            _selfObject = MonoBehaviour.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Tutorials/TutorialDescriptionTooltip"));

            _textDescription = _selfObject.transform.Find("Text").GetComponent<TextMeshPro>();


            _textDescription.text = description;         

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

            if (resizable && _currentBattleground != null)
            {
                _textDescription.autoSizeTextContainer = true;
                Vector2 textSize = _textDescription.GetPreferredValues(description);
                Vector2 backgroundSize = Vector2.one / DefaultTextSize * textSize;
                float value = (backgroundSize.x > backgroundSize.y ? backgroundSize.x : backgroundSize.y);
                _currentBattleground.transform.localScale = Vector3.one * value;
            }

            BoardUnitView unit = null;

            if (ownerId > 0)
            {
                switch (owner)
                {
                    case Enumerators.TutorialObjectOwner.PlayerBattleframe:
                        unit = _gameplayManager.CurrentPlayer.BoardCards.Find((x) =>
                            x.Model.TutorialObjectId == ownerId);
                        break;
                    case Enumerators.TutorialObjectOwner.EnemyBattleframe:
                        unit = _gameplayManager.OpponentPlayer.BoardCards.Find((x) =>
                            x.Model.TutorialObjectId == ownerId);
                        break;
                    default: break;
                }

                if(unit != null)
                {
                    _selfObject.transform.SetParent(unit.Transform, false);
                    _selfObject.transform.localPosition = position;
                }
            }
            else
            {
                _selfObject.transform.position = position;
            }
        }

        public void Show()
        {
            _selfObject?.SetActive(true);
            IsActiveInThisClick = true;
        }

        public void Hide()
        {
            _selfObject?.SetActive(false);
        }

        public void Dispose()
        {
            if (_selfObject != null)
            {
                MonoBehaviour.Destroy(_selfObject);
            }
        }
    }
}
