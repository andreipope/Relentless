
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public static class DataUtilities
    {
        public static Sprite GetAbilityIcon(OverlordId overlordId, Enumerators.Skill skill)
        {
            IDataManager dataManager = GameClient.Get<IDataManager>();
            ILoadObjectsManager loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            OverlordUserInstance overlord = dataManager.CachedOverlordData.GetOverlordById(overlordId);

            if (skill == Enumerators.Skill.NONE)
            {
                return loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
            }

            string iconPath = overlord.GetSkill(skill).Prototype.IconPath;
            return loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + iconPath);
        }

        public static Sprite GetOverlordThumbnailSprite(OverlordId overlordId)
        {
            IDataManager dataManager = GameClient.Get<IDataManager>();
            ILoadObjectsManager loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            OverlordUserInstance overlord = dataManager.CachedOverlordData.GetOverlordById(overlordId);
            string path = "Images/UI/MyDecks/OverlordPortrait";

            switch(overlord.Prototype.Faction)
            {
                case Enumerators.Faction.AIR:
                    return loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_air");
                case Enumerators.Faction.FIRE:
                    return loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_fire");
                case Enumerators.Faction.EARTH:
                    return loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_earth");
                case Enumerators.Faction.TOXIC:
                    return loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_toxic");
                case Enumerators.Faction.WATER:
                    return loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_water");
                case Enumerators.Faction.LIFE:
                    return loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_life");
                default:
                    return null;
            }
        }

        public static void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK,
                Constants.SfxSoundVolume, false, false, true);
        }
    }
}
