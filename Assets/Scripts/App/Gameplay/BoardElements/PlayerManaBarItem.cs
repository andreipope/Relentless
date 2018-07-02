// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using UnityEngine.UI;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Gameplay;
using LoomNetwork.CZB.Data;
using System.Collections.Generic;
using TMPro;

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
                {
                    _manaBottles[i].Active();
                }
                else
                {
                    _manaBottles[i].Disactive();
                }
            }
        }
    }
}