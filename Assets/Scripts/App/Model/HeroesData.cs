using System.Collections;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Data;
using Newtonsoft.Json;
using GrandDevs.Internal;

namespace GrandDevs.CZB.Data
{
    public class HeroesData
    {
        public List<Hero> heroes;
        [JsonIgnore]
        private bool _casted;
        [JsonIgnore]
        public List<Hero> Heroes
        {
            get {
                if (!_casted)
                    CastData();
                return heroes; }
        }

        public HeroesData()
        {

        }

        public void CastData()
        {
            foreach (var item in heroes)
            {
                item.heroElement = Utilites.CastStringTuEnum<Enumerators.SetType>(item.element);
            }
            _casted = true;           
        }
    }

    public class Hero
    {
        public int heroId;
        public string icon;
        public string name;
        public string element;
        [JsonIgnore]
        public Enumerators.SetType heroElement;
        public HeroSkill skill;

        public Hero()
        {

        }
    }

    public class HeroSkill
    {
        public string title;
        public Enumerators.SkillTargetType skillTargetType;
        public int cost;
        public int value;

        public HeroSkill()
        {

        }
    }
}