using UnityEngine;
using GrandDevs.CZB.Data;
using TMPro;

namespace GrandDevs.CZB
{
    public class PlayerSkillItem
    {
        public GameObject selfObject;
        public SpriteRenderer icon;
        public TextMeshPro costText;
        //public HeroSkill skill;

        private ILoadObjectsManager _loader;

        public PlayerSkillItem(GameObject gameObject, HeroSkill skill, string iconPath)
        {
            _loader = GameClient.Get<ILoadObjectsManager>();
            selfObject = gameObject;
            // this.skill = skill;
            icon = selfObject.transform.Find("Icon").GetComponent<SpriteRenderer>();
            costText = selfObject.transform.Find("SpellCost/SpellCostText").GetComponent<TextMeshPro>();

            Sprite sp = _loader.GetObjectByPath<Sprite>(iconPath);
            if (sp != null)
                icon.sprite = sp;
        }
    }
}