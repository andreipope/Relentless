using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class PastActionsPopup : IUIPopup
    {
        public GameObject Self { get; private set; }

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;

        private Transform _parentOfRighrBlockElements;

        private Image _effectTypeImage;

        private ActionElement _leftBlockCardUnitElement,
                              _rightBlockCardUnitElement,
                              _leftBlockCardSpellElement,
                              _rightBlockCardSpellElement,
                              _leftBlockOverlordElement,
                              _rightBlockOverlordElement,
                              _leftBlockOverlordSkillElement;

        private List<ActionElement> _rightBlockElements;

        public void Dispose()
        {
        }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
        }

        public void Hide()
        {
            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            Self = Object.Instantiate(
              _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/PastActionPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _parentOfRighrBlockElements = Self.transform.Find("Block_OnWho/Group_MultipleItems");

            _effectTypeImage = Self.transform.Find("Block_Effect/Image_Effect").GetComponent<Image>();

            Setup();
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
            if(Input.GetKeyDown(KeyCode.K))
            {
                _uiManager.DrawPopup<PastActionsPopup>(new GameActionReport(Enumerators.ActionType.PLAY_CARD_FROM_HAND, new object[] { }));
            }
        }


        private void Setup()
        {
            _leftBlockCardUnitElement = new UnitCardElement(Self.transform.Find("Block_Who/Card_Unit").gameObject);
            _leftBlockCardSpellElement = new UnitCardElement(Self.transform.Find("Block_Who/Card_Spell").gameObject);
            _leftBlockOverlordElement = new UnitCardElement(Self.transform.Find("Block_Who/Item_Overlord").gameObject);
            _leftBlockOverlordSkillElement = new UnitCardElement(Self.transform.Find("Block_Who/Item_OverlordSkill").gameObject);

            _rightBlockCardUnitElement = new UnitCardElement(Self.transform.Find("Block_OnWho/Card_Unit").gameObject);
            _rightBlockCardSpellElement = new UnitCardElement(Self.transform.Find("Block_OnWho/Card_Spell").gameObject);
            _rightBlockOverlordElement = new UnitCardElement(Self.transform.Find("Block_OnWho/Item_Overlord").gameObject);


            _rightBlockElements = new List<ActionElement>();
        }



        public class ActionElement
        {
            protected ILoadObjectsManager _loadObjectsManager;

            public ActionElement()
            {
                _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            }
        }

        public class UnitCardElement : ActionElement
        {
            private GameObject _selfObject;

            public UnitCardElement(GameObject selfObject)
            {
                _selfObject = selfObject;

                _selfObject.SetActive(false);
            }
        }

        public class SpellCardElement : ActionElement
        {
            private GameObject _selfObject;

            public SpellCardElement(GameObject selfObject)
            {
                _selfObject = selfObject;

                _selfObject.SetActive(false);
            }
        }

        public class OverlordElement : ActionElement
        {
            private GameObject _selfObject;

            public OverlordElement(GameObject selfObject)
            {
                _selfObject = selfObject;

                _selfObject.SetActive(false);
            }
        }

        public class OverlordSkillElement : ActionElement
        {
            private GameObject _selfObject;

            public OverlordSkillElement(GameObject selfObject)
            {
                _selfObject = selfObject;

                _selfObject.SetActive(false);
            }
        }

        public class SmallUnitCardElement : ActionElement
        {
            private GameObject _selfObject;

            public SmallUnitCardElement(Transform parent)
            {
                _selfObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/PastActionBar/Item_CardUnitSmall"), parent, false);


            }


            public void Dispose()
            {
                Object.Destroy(_selfObject);
            }
        }

        public class SmallSpellCardElement : ActionElement
        {
            private GameObject _selfObject;

            public SmallSpellCardElement(Transform parent)
            {
                _selfObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/PastActionBar/Item_CardSpellSmall"), parent, false);


            }


            public void Dispose()
            {
                Object.Destroy(_selfObject);
            }
        }
    }
}
