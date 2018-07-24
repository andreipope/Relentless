// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

namespace LoomNetwork.CZB
{
    public class PlayerManaBarItem
    {
        private GameObject _selfObject,
                            _arrowObject,
            
                            _overflowObject;

        private TextMeshPro _gooAmountText,
                            _overflowGooAmountText;
        private List<GooBottleItem> _gooBottles;

        private Transform _overflowBottleContainer;

        private int _maxValue, _currentValue;

        private const int _meterArrowStep = 12;

        private Vector3 _overflowPos;

        private string _overflowPrefabPath;

        public PlayerManaBarItem() { }

        public PlayerManaBarItem(GameObject gameObject, string overflowPrefabName, Vector3 overflowPos)
        {
            _overflowPrefabPath = "Prefabs/" + overflowPrefabName;
            _overflowPos = overflowPos;
            _selfObject = gameObject;
            _gooAmountText = _selfObject.transform.Find("GooMeter/Text").GetComponent<TextMeshPro>();
            _arrowObject = _selfObject.transform.Find("GooMeter/ArrowCenter").gameObject;
            _gooBottles = new List<GooBottleItem>();
            GameObject bottle = null;
            for (int i = 0; i < _selfObject.transform.childCount; i++)
            {
                bottle = _selfObject.transform.GetChild(i).gameObject;
                if (bottle.name.Contains("BottleGoo"))
                    _gooBottles.Add(new GooBottleItem(bottle));
            }
            _arrowObject.transform.localEulerAngles = Vector3.forward * 90;
        }

        public void SetGoo(int gooValue)
        {
            _currentValue = gooValue;
            _gooAmountText.text = _currentValue.ToString() + "/" + _maxValue;

            UpdateGooOVerflow();

            for (var i = 0; i < _gooBottles.Count; i++)
            {
                if (i < _currentValue)
                    Active(_gooBottles[i]);
                else
                   Disactive(_gooBottles[i]);
            }
            UpdateGooMeter();
        }

       

        public void SetVialGoo(int maxValue)
        {
            _maxValue = maxValue;
            _gooAmountText.text = _currentValue.ToString() + "/" + _maxValue;
            for (var i = 0; i < _gooBottles.Count; i++)
            {
                _gooBottles[i].self.SetActive(i < _maxValue ? true : false);
            }
            UpdateGooOVerflow();
        }

        private void UpdateGooOVerflow()
        {
            if (_currentValue > _maxValue && _overflowObject == null)
                CreateOverflow();
            else if (_currentValue <= _maxValue && _overflowObject != null)
                DestroyOverflow();

            if (_overflowGooAmountText != null)
            {
                _overflowGooAmountText.text = _currentValue.ToString() + "/" + _maxValue;
                for (int i = 0; i < _overflowBottleContainer.childCount; i++)
                {
                    _overflowBottleContainer.GetChild(i).gameObject.SetActive(i < _currentValue ? true : false); ;
                }
            }
        }

        public void Active(GooBottleItem item)
        {
            item.fullBoottle.DOFade(1.0f, 0.5f);
            item.glowBottle.DOFade(1.0f, 0.5f);
        }

        public void Disactive(GooBottleItem item)
        {
            item.fullBoottle.DOFade(0.0f, 0.5f);
            item.glowBottle.DOFade(0.0f, 0.5f);
        }

        private void UpdateGooMeter()
        {
            int targetRotation = 90 - _meterArrowStep * _currentValue;
            if (targetRotation < -90)
                targetRotation = -90;
            _arrowObject.transform.DORotate(Vector3.forward * targetRotation, 1f);
            //_arrowObject.transform.localEulerAngles = Vector3.forward * (90 - _meterArrowStep);
        }

        private void CreateOverflow()
        {
            _overflowObject = MonoBehaviour.Instantiate<GameObject>(GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>(_overflowPrefabPath));
            _overflowObject.transform.localPosition = _overflowPos;
            _overflowGooAmountText = _overflowObject.transform.Find("clock/Text").GetComponent<TextMeshPro>();
            _overflowBottleContainer = _overflowObject.transform.Find("Bottles").transform;
            for (int i = 0; i < _overflowBottleContainer.childCount; i++)
            {
                _overflowBottleContainer.GetChild(i).gameObject.SetActive(i < _currentValue ? true : false); ;
            }
            _selfObject.SetActive(false);
        }


        private void DestroyOverflow()
        {
            Debug.Log("DestroyOverflow");
            MonoBehaviour.Destroy(_overflowObject);
            _overflowObject = null;
            _overflowBottleContainer = null;
            _overflowGooAmountText = null;
            _selfObject.SetActive(true);
        }

        public struct GooBottleItem
        {
            public SpriteRenderer fullBoottle,
                                   glowBottle;
            public GameObject self;


            public GooBottleItem(GameObject gameObject)
            {
                self = gameObject;
                fullBoottle = self.transform.Find("Goo").GetComponent<SpriteRenderer>();
                glowBottle = self.transform.Find("BottleGlow").GetComponent<SpriteRenderer>();
            }
        }
    }
}