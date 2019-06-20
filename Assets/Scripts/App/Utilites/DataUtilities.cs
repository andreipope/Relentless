
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
            Enumerators.Faction faction = overlord.Prototype.Faction;

            string path = "Images/UI/MyDecks/OverlordPortrait/";
            path = path + "overlord_portrait_" + faction.ToString().ToLower();
            return GameClient.Get<ILoadObjectsManager>().GetObjectByPath<Sprite>(path);
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

        public static Sprite GetOverlordImage(Enumerators.Faction overlordFaction)
        {
            string path = "Images/UI/Overlord_Image/";
            path = path + "champion_image_" + overlordFaction.ToString().ToLower();
            return GameClient.Get<ILoadObjectsManager>().GetObjectByPath<Sprite>(path);
        }

        public static OverlordUserInstance GetOverlordDataFromDeck(Deck deck)
        {
            IDataManager dataManager = GameClient.Get<IDataManager>();
            OverlordUserInstance overlord = dataManager.CachedOverlordData.GetOverlordById(deck.OverlordId);
            return overlord;
        }

        public static OverlordUserInstance GetOverlordDataFromDeck(DeckId deckId)
        {
            IDataManager dataManager = GameClient.Get<IDataManager>();
            Deck deck = dataManager.CachedDecksData.Decks.Find(cachedDeck => cachedDeck.Id == deckId);
            OverlordUserInstance overlord = dataManager.CachedOverlordData.GetOverlordById(deck.OverlordId);
            return overlord;
        }
    }
}
