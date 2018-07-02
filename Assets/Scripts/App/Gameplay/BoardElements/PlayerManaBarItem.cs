using UnityEngine;
using UnityEngine.UI;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Gameplay;
using GrandDevs.CZB.Data;
using System.Collections.Generic;
using TMPro;

namespace GrandDevs.CZB
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