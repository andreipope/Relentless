
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

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

        public static Enumerators.Skill GetOverlordAbilityType(OverlordId overlordId, SkillId skillId)
        {
            IDataManager dataManager = GameClient.Get<IDataManager>();
            OverlordUserInstance overlordUserInstance = dataManager.CachedOverlordData.GetOverlordById(overlordId);
            int index = overlordUserInstance.Skills.FindIndex(skill => skill.Prototype.Id == skillId);
            return index != -1 ? overlordUserInstance.Skills[index].Prototype.Skill : Enumerators.Skill.NONE;
        }

        public static Sprite GetAbilityIcon(OverlordSkillUserInstance skill)
        {
            ILoadObjectsManager loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            if (!skill.UserData.IsUnlocked)
            {
                return loadObjectsManager.GetObjectByPath<Sprite>("Images/UI/MyDecks/skill_unselected");
            }

            string iconPath = skill.Prototype.IconPath;
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

        public static bool IsSkillLocked(OverlordId overlordId, SkillId skillId)
        {
            IDataManager dataManager = GameClient.Get<IDataManager>();
            OverlordUserInstance overlordUserInstance = dataManager.CachedOverlordData.GetOverlordById(overlordId);
            int index = overlordUserInstance.Skills.FindIndex(skill => skill.Prototype.Id == skillId);
            return index != -1 && overlordUserInstance.Skills[index].UserData.IsUnlocked;
        }


        public static Enumerators.Skill GetSkill(OverlordId overlordId, SkillId skillId)
        {
            IDataManager dataManager = GameClient.Get<IDataManager>();
            OverlordUserInstance overlordUserInstance = dataManager.CachedOverlordData.GetOverlordById(overlordId);
            int index = overlordUserInstance.Skills.FindIndex(skill => skill.Prototype.Id == skillId);
            return index != -1 ? overlordUserInstance.Skills[index].Prototype.Skill : Enumerators.Skill.NONE;
        }

        public static SkillId GetSkillId(OverlordId overlordId, Enumerators.Skill skillType)
        {
            IDataManager dataManager = GameClient.Get<IDataManager>();
            OverlordUserInstance overlordUserInstance = dataManager.CachedOverlordData.GetOverlordById(overlordId);
            int index = overlordUserInstance.Skills.FindIndex(skill => skill.Prototype.Skill == skillType);
            return overlordUserInstance.Skills[index].Prototype.Id;
        }
    }
}
