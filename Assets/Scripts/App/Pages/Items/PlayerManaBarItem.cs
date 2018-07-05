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
        private GameObject selfObject;
        private TextMeshPro _manaText;
        private List<ManaBottleItem> _manaBottles;

        public PlayerManaBarItem() { }

        public PlayerManaBarItem(GameObject gameObject)
        {
            selfObject = gameObject;
            _manaText = selfObject.transform.Find("ManaAmount/Text").GetComponent<TextMeshPro>();
            _manaBottles = new List<ManaBottleItem>();
            GameObject bottle = null;
            for (int i = 0; i < selfObject.transform.childCount; i++)
            {
                bottle = selfObject.transform.GetChild(i).gameObject;
                if (bottle.name.Contains("ManaIcon"))
                    _manaBottles.Add(new ManaBottleItem(bottle));
            }
        }

        public void SetMana(int mana)
        {
            _manaText.text = mana.ToString();
            for (var i = 0; i < _manaBottles.Count; i++)
            {
                if (i < mana)
                    Active(_manaBottles[i]);
                else
                   Disactive(_manaBottles[i]);
            }
        }


        public void Active(ManaBottleItem item)
        {
            item.fullBoottle.DOFade(1.0f, 0.5f);
            item.glowBottle.DOFade(1.0f, 0.5f);
        }

        public void Disactive(ManaBottleItem item)
        {
            item.fullBoottle.DOFade(0.0f, 0.5f);
            item.glowBottle.DOFade(0.0f, 0.5f);
        }


        public struct ManaBottleItem
        {
            public SpriteRenderer fullBoottle,
                                   glowBottle;


            public ManaBottleItem(GameObject gameObject)
            {
                fullBoottle = gameObject.transform.Find("ManaIconBlue/goobottle_goo").GetComponent<SpriteRenderer>();
                glowBottle = gameObject.transform.Find("ManaIconBlue/glow_goo").GetComponent<SpriteRenderer>();
            }
        }
    }
}