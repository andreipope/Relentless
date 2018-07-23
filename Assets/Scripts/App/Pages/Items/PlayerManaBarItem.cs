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
            _gooAmountText.text = gooValue.ToString() + "/10";
            
            if (gooValue > 10 && _overflowObject == null)
                CreateOverflow();
            else if (gooValue < 11 && _overflowObject != null)
                DestroyOverflow();

            if (_overflowGooAmountText != null)
                _overflowGooAmountText.text = gooValue.ToString() + "/10";

            for (var i = 0; i < _gooBottles.Count; i++)
            {
                if (i < gooValue)
                    Active(_gooBottles[i]);
                else
                   Disactive(_gooBottles[i]);
            }
            UpdateGooMeter(gooValue);
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

        private void UpdateGooMeter(int gooValue)
        {
            int targetRotation = 90 - _meterArrowStep * gooValue;
            if (targetRotation < -90)
                targetRotation = -90;
            _arrowObject.transform.DORotate(Vector3.forward * targetRotation, 1f);
            //_arrowObject.transform.localEulerAngles = Vector3.forward * (90 - _meterArrowStep);
        }

        private void CreateOverflow()
        {
            _overflowObject = MonoBehaviour.Instantiate<GameObject>(GameClient.Get<ILoadObjectsManager>().GetObjectByPath<GameObject>(_overflowPrefabPath), _selfObject.transform);
            _overflowObject.transform.localPosition = _overflowPos;
            _overflowGooAmountText = _overflowObject.transform.Find("clock/Text").GetComponent<TextMeshPro>();
        }


        private void DestroyOverflow()
        {
            Debug.Log("DestroyOverflow");
            MonoBehaviour.Destroy(_overflowObject);
            _overflowObject = null;
            _overflowGooAmountText = null;
        }

        public struct GooBottleItem
        {
            public SpriteRenderer fullBoottle,
                                   glowBottle;


            public GooBottleItem(GameObject gameObject)
            {
                fullBoottle = gameObject.transform.Find("Goo").GetComponent<SpriteRenderer>();
                glowBottle = gameObject.transform.Find("BottleGlow").GetComponent<SpriteRenderer>();
            }
        }
    }
}