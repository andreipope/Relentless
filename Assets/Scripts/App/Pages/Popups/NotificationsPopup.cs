// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoomNetwork.CZB
{
    public class NotificationsPopup : IUIPopup
    {
        public GameObject Self
        {
            get { return _selfPage; }
        }

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private INotificationManager _notificationManager;

        private GameObject _selfPage;

        private Queue<Notification> _notifications;

        private bool _isInDrawingNotification;


        private NotificationObject _notificationObject;


        public void Dispose()
        {
            ResetNotifications();
        }

        public void Hide()
        {
            //  _selfPage.SetActive(false);
        }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _notificationManager = GameClient.Get<INotificationManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/NotificationsPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas2.transform, false);

            _notificationObject = new NotificationObject(_selfPage.transform.Find("Panel_TopNotification").gameObject);
            _notificationObject.NotificationObjectClosedEvent += NotificationObjectClosedEventHandler;

            _notifications = new Queue<Notification>();

            _notificationManager.DrawNotificationEvent += DrawNotificationEventHandler;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            _selfPage.SetActive(true);
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
            if(_notifications.Count > 0 && !_isInDrawingNotification)
            {
                DrawNotification(_notifications.Dequeue());
            }

        }

        private void DrawNotificationEventHandler(Notification notification)
        {
            _notifications.Enqueue(notification);
        }

        private void ResetNotifications()
        {
            _notifications.Clear();
        }


        private void DrawNotification(Notification notification)
        {
            _isInDrawingNotification = true;

            _notificationObject.Activate(notification);
            _notificationObject.Draw();
        }

        private void NotificationObjectClosedEventHandler(NotificationObject obj)
        {
            _isInDrawingNotification = false;
        }
    }


    public class NotificationObject
    {
        public event Action<NotificationObject> NotificationObjectClosedEvent;

        private ILoadObjectsManager _loadObjectsManager;
        private ITimerManager _timerManager;

        private float _swipeSpeed = 1000f;
        private float _timeToDrawingNotification = 1f;

        private bool _isHiding,
                     _isShowing;

        private bool _isActive;


        private Text _messageText;
        private Image _iconImage;
        private Button _selectButton;

        private Dictionary<Enumerators.NotificationType, NotificationInfo> _notificationsInfos;

        private RectTransform _selfRectTransform;

        public Notification selfNotification;
        public GameObject selfObject;

        public NotificationObject(GameObject self)
        {
           
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _timerManager = GameClient.Get<ITimerManager>();

            selfObject = self;

            _selfRectTransform = selfObject.GetComponent<RectTransform>();

            _messageText = selfObject.transform.Find("Text_Message").GetComponent<Text>();
            _iconImage = selfObject.transform.Find("Image_Icon").GetComponent<Image>();
            _selectButton = selfObject.GetComponent<Button>();

            _selectButton.onClick.AddListener(SelectButtonOnClickHandler);

            FillNotificationsInfo();
        }

        public void Activate(Notification notification)
        {
            selfNotification = notification;

            _messageText.text = string.Format("{0}", selfNotification.message);

            var style = _notificationsInfos[selfNotification.type];
            _messageText.color = style.color;
            _iconImage.sprite = style.sprite;
        }

        public void Draw()
        {
            if (_isShowing || _isActive)
                return;

            _isActive = true;

            _isShowing = true;

            _timerManager.AddTimer(Showing, null, Time.deltaTime, true);
            
        }

        public void Hide()
        {
            if (_isHiding || _isShowing || !_isActive)
                return;

            _timerManager.StopTimer(HideDirectly);

            _isHiding = true;

            _timerManager.AddTimer(Hiding, null, Time.deltaTime, true);
        }


        public void SetYDirectly(float y)
        {
            _selfRectTransform.anchoredPosition3D = new Vector3(_selfRectTransform.anchoredPosition3D.x, y, _selfRectTransform.anchoredPosition3D.z);
        }


        private void FillNotificationsInfo()
        {
            _notificationsInfos = new Dictionary<Enumerators.NotificationType, NotificationInfo>();
            _notificationsInfos.Add(Enumerators.NotificationType.MESSAGE, new NotificationInfo() { color = Color.white, sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/icon_warning") });
            _notificationsInfos.Add(Enumerators.NotificationType.LOG, new NotificationInfo() { color = Color.grey, sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/icon_warning") });
            _notificationsInfos.Add(Enumerators.NotificationType.ERROR, new NotificationInfo() { color = Color.red, sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/icon_warning") });
            _notificationsInfos.Add(Enumerators.NotificationType.WARNING, new NotificationInfo() { color = Color.yellow, sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/icon_warning") });
        }

        private void HideDirectly(object[] param)
        {
            Hide();
        }


        private void SelectButtonOnClickHandler()
        {
            Hide();
        }

        private void Showing(object[] param)
        {
            if (_selfRectTransform.anchoredPosition3D.y > 0)
            {
                _selfRectTransform.anchoredPosition3D -= Vector3.up * Time.deltaTime * _swipeSpeed;
            }
            else
            {
                _isShowing = false;
                _timerManager.StopTimer(Showing);

                SetYDirectly(0);

                _timerManager.AddTimer(HideDirectly, null, _timeToDrawingNotification, false);
            }
        }

        private void Hiding(object[] param)
        {
            if (_selfRectTransform.anchoredPosition3D.y < _selfRectTransform.sizeDelta.y)
            {
                _selfRectTransform.anchoredPosition3D += Vector3.up * Time.deltaTime * _swipeSpeed;
            }
            else
            {
                _isHiding = false;
                _isActive = false;
                _timerManager.StopTimer(Hiding);

                SetYDirectly(_selfRectTransform.sizeDelta.y);

                if (NotificationObjectClosedEvent != null)
                    NotificationObjectClosedEvent(this);
            }
        }
    }

    public class NotificationInfo
    {
        public Color color;
        public Sprite sprite;
    }

}