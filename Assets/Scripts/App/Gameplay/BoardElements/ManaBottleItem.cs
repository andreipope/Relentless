// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using DG.Tweening;


namespace LoomNetwork.CZB
{
    public class ManaBottleItem
    {
        public GameObject selfObject;

        private SpriteRenderer _fullBoottle,
                               _glowBottle;

        public ManaBottleItem() { }

        public ManaBottleItem(GameObject gameObject)
        {
            selfObject = gameObject;
            _fullBoottle = selfObject.transform.Find("ManaIconBlue/goobottle_goo").GetComponent<SpriteRenderer>();
            _glowBottle = selfObject.transform.Find("ManaIconBlue/glow_goo").GetComponent<SpriteRenderer>();
        }

        public void Active()
        {
            _fullBoottle.DOFade(1.0f, 0.5f);
            _glowBottle.DOFade(1.0f, 0.5f);
        }

        public void Disactive()
        {
            _fullBoottle.DOFade(0.0f, 0.5f);
            _glowBottle.DOFade(0.0f, 0.5f);
        }
    }
}