using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class InputFieldItem
    {
        public Action<InputFieldItem> OnHighlighted;
        public Action<InputFieldItem> OnEndHighlighted;

        public bool IsHighlighting { get { return _isHighlighting; } }
        public bool Interactable { get { return _contentInputField.interactable; } }

        public GameObject selfObject;

        private SourceInputField _contentInputField;


        private RectTransform _selfTransform;

        private EventTrigger _selfEventTrigger;

        private Transform _selfParent;
        private Vector3 _selfPosition;
        private int _selfSiblingIndex;


        private IUIManager _uiManager;
        private ITimerManager _timer;
        private IMobileKeyboardManager _mobileKeyboardManager;

        private bool _isEditing = false,
                     _isHighlighting = false;

        private string _onlyTheseSymbols;

        public bool isRequired;

        private bool _wasEmpty;

        public InputFieldItem(GameObject self, bool require = false)
        {
            selfObject = self;
            isRequired = require;

            _uiManager = GameClient.Get<IUIManager>();
            _timer = GameClient.Get<ITimerManager>();
            _mobileKeyboardManager = GameClient.Get<IMobileKeyboardManager>();

            _selfTransform = selfObject.GetComponent<RectTransform>();
            _selfEventTrigger = selfObject.AddComponent<EventTrigger>();

            _contentInputField = selfObject.GetComponent<SourceInputField>();

#if UNITY_EDITOR
            _contentInputField.onValueChange.AddListener(ContentInputFieldOnValueChangedHandler);
            _contentInputField.onEndEdit.AddListener(ContentInputFieldOnEndEditHandler);
#endif

#if !UNITY_EDITOR
            RegisterCallbackOnTrigger(EventTriggerType.PointerClick, InputClickedEventTriggerHandler);
            RegisterCallbackOnTrigger(EventTriggerType.Select, InputClickedEventTriggerHandler);
#endif

            _contentInputField.onValueChange.AddListener((val) =>
            {
                if (!string.IsNullOrEmpty(_onlyTheseSymbols) && !string.IsNullOrEmpty(val))
                {
                    if (!_onlyTheseSymbols.Contains(val[val.Length - 1].ToString()))
                    {
                        _contentInputField.text = val.Trim(val[val.Length - 1]);
                    }
                }
            });

            _contentInputField.custom_allowMobileKeyboard = false;
            _mobileKeyboardManager.KeyboardCanceledEvent += KeyboardCanceledEventHandler;
            _mobileKeyboardManager.KeyboardDoneEvent += KeyboardDoneEventHandler;
            _mobileKeyboardManager.KeyboardUpdateEvent += KeyboardUpdateEventHandler;

            Deactivate();
        }

        public void RestrictSymbolsTo(string these)
        {
            _onlyTheseSymbols = these;
        }

        public void Reset()
        {
            _contentInputField.text = string.Empty;
            _contentInputField.interactable = true;
            Deactivate();
        }

        public void SetInteractable(bool interactable = true)
        {
            _contentInputField.interactable = interactable;
            selfObject.GetComponent<SourceInputField>().enabled = interactable;
        }

        public string GetContent()
        {
            return _contentInputField.text;
        }

        public void SetContent(string value)
        {
            _contentInputField.text = value;
        }

        public void SetErrorState()
        {

        }

        private void Activate()
        {

        }

        private void Deactivate()
        {

        }


        private void CheckAndSetState(string data)
        {
            if (string.IsNullOrEmpty(data))
                Deactivate();
            else
                Activate();
        }


#if UNITY_EDITOR
        private void ContentInputFieldOnValueChangedHandler(string data)
        {
            CheckAndSetState(data);
        }

        private void ContentInputFieldOnEndEditHandler(string data)
        {

        }
#endif

        private void InputClickedEventTriggerHandler(BaseEventData eventData)
        {
            if (_isHighlighting || _isEditing)
                return;

            HighlightContent();
        }

        private void KeyboardUpdateEventHandler(string obj)
        {
            if (!_isEditing)
                return;

            _contentInputField.text = obj;
            CheckAndSetState(obj);
        }

        private void KeyboardCanceledEventHandler()
        {
            if (!_isEditing)
                return;

            UnhighlightContent();
        }

        public void KeyboardDoneEventHandler()
        {
            if (!_isEditing)
                return;

            UnhighlightContent();
        }

        public void HighlightContent()
        {
            if (_isEditing && _contentInputField.interactable)
                return;

            _isEditing = true;
            _isHighlighting = true;
            _wasEmpty = string.IsNullOrEmpty(GetContent());

            LayoutElement layoutElement = selfObject.GetComponent<LayoutElement>();
            if (layoutElement == null) layoutElement = selfObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            _selfParent = _selfTransform.parent;
            _selfPosition = _selfTransform.anchoredPosition3D;
            _selfSiblingIndex = _selfTransform.GetSiblingIndex();


            _selfTransform.SetParent(_uiManager.Canvas.transform);
            _selfTransform.anchoredPosition3D = new Vector3(_selfTransform.anchoredPosition.x, -Screen.height / 2 + _selfTransform.sizeDelta.y / 2, 0f);

#if !UNITY_EDITOR
            _mobileKeyboardManager.DrawKeyboard(_contentInputField.text, true, _contentInputField.keyboardType, multiline: _contentInputField.multiLine);
#endif

            _contentInputField.OnSelect(new BaseEventData(EventSystem.current));

            if (OnHighlighted != null)
                OnHighlighted(this);
        }

        public void UnhighlightContent()
        {
            if (!_isEditing)
                return;

            _isEditing = false;
            _isHighlighting = false;


            _selfTransform.SetParent(_selfParent);
            _selfTransform.SetSiblingIndex(_selfSiblingIndex);
            _selfTransform.anchoredPosition3D = _selfPosition;

            _contentInputField.OnDeselect(new BaseEventData(EventSystem.current));

            LayoutElement layoutElement = selfObject.GetComponent<LayoutElement>();
            if (layoutElement != null) MonoBehaviour.Destroy(layoutElement);

#if !UNITY_EDITOR
            _mobileKeyboardManager.HideKeyboard(false, false);
#endif

            if (OnEndHighlighted != null)
                OnEndHighlighted(this);
        }

        private void RegisterCallbackOnTrigger(EventTriggerType type, UnityAction<BaseEventData> callback)
        {
            var entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener(callback);
            _selfEventTrigger.triggers.Add(entry);
        }
    }
}
