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

        public void DrawOverlordSayPopup(string description,
                                        Enumerators.TooltipAlign align,
                                        Enumerators.TutorialObjectOwner owner,
                                        float duration = Constants.OverlordTalkingPopupDuration,
                                        string soundToPlay = Constants.Empty,
                                        float soundToPlayBeginDelay = 0,
                                        Enumerators.TutorialActivityAction actionToHideThisPopup = Enumerators.TutorialActivityAction.Undefined,
                                        float minimumShowTime = Constants.OverlordTalkingPopupMinimumShowTime)
        {
            if (_overlordSayPopups.Find(x => x.Description == description) != null)
                return;

            OverlordSayPopup overlordSayPopup = new OverlordSayPopup(description,
                                                                    align,
                                                                    owner,
                                                                    _overlordsChatContainer,
                                                                    duration,
                                                                    soundToPlay,
                                                                    soundToPlayBeginDelay,
                                                                    actionToHideThisPopup,
                                                                    minimumShowTime);
            overlordSayPopup.OverlordSayPopupHided += OverlordSayPopupHided;
            _overlordSayPopups.Add(overlordSayPopup);
            UpdatePopupsPositions();
            SortPopups();
        }

        public void UpdatePopupsByReportActivityAction(Enumerators.TutorialActivityAction action)
        {
            List<OverlordSayPopup> sortingPopups = _overlordSayPopups.FindAll(popup => popup.ActionToHideThisPopup == action);
            if(sortingPopups.Count > 0)
            {
                foreach (OverlordSayPopup sayPopup in sortingPopups)
                {
                    sayPopup.Close();
                }
            }
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

            public Enumerators.TutorialObjectOwner Owner;

            public Enumerators.TutorialActivityAction ActionToHideThisPopup;

            public float HeightPopup;

            public string Description { get; private set; }

            private const float DurationOfHide = 0.5f;
            private const float MinHeight = 2.85f;

            private readonly ITutorialManager _tutorialManager;
            private readonly ILoadObjectsManager _loadObjectsManager;

            private GameObject _selfObject;

            private SpriteRenderer _currentBattleground;

            private TextMeshPro _textDescription;

            private bool _canBeClosed = false;

            public OverlordSayPopup(string description,
                                    Enumerators.TooltipAlign align,
                                    Enumerators.TutorialObjectOwner owner,
                                    Transform parent,
                                    float drawDuration = Constants.OverlordTalkingPopupDuration,
                                    string soundToPlay = Constants.Empty,
                                    float soundToPlayBeginDelay = 0,
                                    Enumerators.TutorialActivityAction actionToHideThisPopup = Enumerators.TutorialActivityAction.Undefined,
                                    float minimumShowTime = Constants.OverlordTalkingPopupMinimumShowTime)
            {
                _tutorialManager = GameClient.Get<ITutorialManager>();
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

                this.Align = align;
                this.Owner = owner;
                ActionToHideThisPopup = actionToHideThisPopup;

                _selfObject = MonoBehaviour.Instantiate(
                    _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/Tutorials/OverlordSayTooltip"), parent, false);

                _textDescription = _selfObject.transform.Find("Text").GetComponent<TextMeshPro>();

                Description = description;

                _textDescription.text = Description;


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

                if (actionToHideThisPopup == Enumerators.TutorialActivityAction.Undefined)
                {
                    UpdatePossibilityForClose();
                    InternalTools.DoActionDelayed(Close, drawDuration);
                }
                else
                {
                    InternalTools.DoActionDelayed(UpdatePossibilityForClose, minimumShowTime);
                }

                if(!string.IsNullOrEmpty(soundToPlay))
                {
                    _tutorialManager.PlayTutorialSound(soundToPlay, soundToPlayBeginDelay);
                }
            }

            private void UpdatePossibilityForClose()
            {
                _canBeClosed = true;
            }

            public void UpdatePosition(float height)
            {
                Vector3 position = Vector3.zero;
                switch (Owner)
                {
                    case Enumerators.TutorialObjectOwner.EnemyOverlord:
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
                    case Enumerators.TutorialObjectOwner.PlayerOverlord:
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

            public void Close()
            {
                if (!_canBeClosed)
                    return;

                _textDescription.DOFade(0, DurationOfHide);
                _currentBattleground.DOFade(0, DurationOfHide).OnComplete(Hide);
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
