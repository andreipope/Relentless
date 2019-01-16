using System.Collections.Generic;
using System;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;
using Loom.ZombieBattleground.Helpers;

namespace Loom.ZombieBattleground
{
    public class OverlordsTalkingController : IController
    {
        private Transform _overlordsChatContainer;

        private List<OverlordSayPopup> _overlordSayPopups;

        private ITutorialManager _tutorialManager;

        public void Dispose()
        {
        }

        public void Init()
        {
            _overlordSayPopups = new List<OverlordSayPopup>();
            GameClient.Get<IGameplayManager>().GameInitialized += GameStartedHandler;
            _tutorialManager = GameClient.Get<ITutorialManager>();
        }

        private void GameStartedHandler()
        {
            if(_tutorialManager.IsTutorial)
            {
                _overlordsChatContainer = new GameObject("OverlordsTalkingContainer").transform;
            }
        }

        public void ResetAll()
        {
            foreach (OverlordSayPopup popup in _overlordSayPopups)
            {
                popup.Dispose();
            }
            _overlordSayPopups.Clear();

            if(_overlordsChatContainer != null)
            {
                MonoBehaviour.Destroy(_overlordsChatContainer.gameObject);
            }
        }

        public void Update()
        {
        }

        public void DrawOverlordSayPopup(string description, Enumerators.TooltipAlign align, Enumerators.TooltipOwner owner)
        {
            OverlordSayPopup overlordSayPopup = new OverlordSayPopup(description, align, owner, _overlordsChatContainer);
            overlordSayPopup.OverlordSayPopupHided += OverlordSayPopupHided;
            _overlordSayPopups.Add(overlordSayPopup);
            UpdatePopupsPositions();
            SortPopups();
        }

        private void OverlordSayPopupHided(OverlordSayPopup popup)
        {
            _overlordSayPopups.Remove(popup);
            UpdatePopupsPositions();
        }

        private void SortPopups()
        {

        }

        private void UpdatePopupsPositions()
        {
            var sortingGroupsPopups = _overlordSayPopups.GroupBy(popup => new { popup.Align, popup.Owner })
                        .Select(group => new
                        {
                            Name = group.Key,
                            Count = group.Count(),
                            Popups = group.Select(p => p)
                        });

            float height = 0;
            foreach (var group in sortingGroupsPopups)
            {
                height = 0;
                foreach (OverlordSayPopup sayPopup in group.Popups)
                {
                    sayPopup.UpdatePosition(height);
                    height += sayPopup.HeightPopup;
                }
            }
        }

        public class OverlordSayPopup
        {
            public event Action<OverlordSayPopup> OverlordSayPopupHided;

            public Enumerators.TooltipAlign Align;

            public Enumerators.TooltipOwner Owner;

            public float HeightPopup;

            private const float DurationOfShow = 2f;
            private const float DurationOfHide = 0.5f;
            private const float MinHeight = 2.85f;

            private readonly ITutorialManager _tutorialManager;
            private readonly ILoadObjectsManager _loadObjectsManager;

            private GameObject _selfObject;

            private SpriteRenderer _currentBattleground;

            private TextMeshPro _textDescription;

            public OverlordSayPopup(string description, Enumerators.TooltipAlign align, Enumerators.TooltipOwner owner, Transform parent)
            {
                _tutorialManager = GameClient.Get<ITutorialManager>();
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                this.Align = align;
                this.Owner = owner;

                _selfObject = MonoBehaviour.Instantiate(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Tutorials/OverlordSayTooltip"), parent, false);

                _textDescription = _selfObject.transform.Find("Text").GetComponent<TextMeshPro>();

                _textDescription.text = description;


                switch (align)
                {
                    case Enumerators.TooltipAlign.CenterLeft:
                    case Enumerators.TooltipAlign.CenterRight:
                        _currentBattleground = _selfObject.transform.Find("ArrowType/Arrow_" + align.ToString()).GetComponent<SpriteRenderer>();
                        _currentBattleground.gameObject.SetActive(true);
                        break;
                    default:
                        break;
                }

                HeightPopup = _currentBattleground.sprite.bounds.size.y * _selfObject.transform.localScale.y;

                InternalTools.DoActionDelayed(() =>
                {
                    _textDescription.DOFade(0, DurationOfHide);
                    _currentBattleground.DOFade(0, DurationOfHide).OnComplete(Hide);
                }, DurationOfShow);
            }

            public void UpdatePosition(float height)
            {
                Vector3 position = Vector3.zero;
                switch (Owner)
                {
                    case Enumerators.TooltipOwner.EnemyOverlord:
                        if(Align == Enumerators.TooltipAlign.CenterLeft)
                        {
                            position = Constants.LeftOpponentOverlordPositionForChat;
                        }
                        else if(Align == Enumerators.TooltipAlign.CenterRight)
                        {
                            position = Constants.RightOpponentOverlordPositionForChat;
                        }
                        position.y -= height;
                        break;
                    case Enumerators.TooltipOwner.PlayerOverlord:
                        if (Align == Enumerators.TooltipAlign.CenterLeft)
                        {
                            position = Constants.LeftPlayerOverlordPositionForChat;
                        }
                        else if (Align == Enumerators.TooltipAlign.CenterRight)
                        {
                            position = Constants.RightPlayerOverlordPositionForChat;
                        }
                        position.y += height;
                        break;
                    default:
                        break;
                }

                _selfObject.transform.position = position;
            }

            public void Hide()
            {
                MonoBehaviour.Destroy(_selfObject);

                OverlordSayPopupHided?.Invoke(this);

                _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.OverlordSayPopupHided);
            }

            public void Dispose()
            {

            }
        }

    }
}
