
using System;
using System.Text.RegularExpressions;
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

        public static Sprite GetOverlordImage(OverlordId overlordId)
        {
            Enumerators.Faction faction = GetFaction(overlordId);

            string path = "Images/UI/Overlord_Image/";
            path = path + "champion_image_" + faction.ToString().ToLowerInvariant();
            return GameClient.Get<ILoadObjectsManager>().GetObjectByPath<Sprite>(path);
        }

        public static Enumerators.Faction GetFaction(OverlordId overlordId)
        {
            IDataManager dataManager = GameClient.Get<IDataManager>();

            OverlordUserInstance overlord = dataManager.CachedOverlordData.GetOverlordById(overlordId);
            return overlord.Prototype.Faction;
        }


        public static Sprite GetOverlordImage(Enumerators.Faction overlordFaction)
        {
            string path = "Images/UI/Overlord_Image/";
            path = path + "champion_image_" + overlordFaction.ToString().ToLowerInvariant();
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
            if (deck == null)
                return null;

            OverlordUserInstance overlord = dataManager.CachedOverlordData.GetOverlordById(deck.OverlordId);
            return overlord;
        }

        public static Vector3 GetOverlordImagePositionInViewDeck(Enumerators.Faction faction)
        {
            switch (faction)
            {
                case Enumerators.Faction.FIRE:
                    return new Vector3(116f, -315f, 0f);
                case Enumerators.Faction.WATER:
                    return new Vector3(11f, -345f, 0f);
                case Enumerators.Faction.EARTH:
                    return new Vector3(-3f, -275f, 0f);
                case Enumerators.Faction.AIR:
                    return new Vector3(-150f, -200f, 0f);
                case Enumerators.Faction.LIFE:
                    return new Vector3(-42f, -185f, 0f);
                case Enumerators.Faction.TOXIC:
                    return new Vector3(101f, -185f, 0f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(faction), faction, null);
            }
        }

        public static Vector3 GetOverlordImageScaleInViewDeck(Enumerators.Faction faction)
        {
            switch (faction)
            {
                case Enumerators.Faction.FIRE:
                    return Vector3.one * 1.4f;
                case Enumerators.Faction.WATER:
                    return Vector3.one * 1.2f;
                case Enumerators.Faction.EARTH:
                    return Vector3.one;
                case Enumerators.Faction.AIR:
                    return Vector3.one;
                case Enumerators.Faction.LIFE:
                    return Vector3.one * 1.2f;
                case Enumerators.Faction.TOXIC:
                    return Vector3.one;
                default:
                    throw new ArgumentOutOfRangeException(nameof(faction), faction, null);
            }
        }

        public static Sprite GetOverlordDeckIcon(Enumerators.Faction faction)
        {
            string path = "Images/UI/DeckIcons/";
            path = path + "icon_" + faction.ToString().ToLowerInvariant();
            return GameClient.Get<ILoadObjectsManager>().GetObjectByPath<Sprite>(path);
        }

        public static Card ApplyCardVariantOverrides(IReadOnlyCard variantCard, Card standardCard)
        {
            return new Card(
                variantCard.CardKey,
                variantCard.Overrides?.Set ?? standardCard.Set,
                variantCard.Overrides?.Name ?? standardCard.Name,
                variantCard.Overrides?.Cost ?? standardCard.Cost,
                variantCard.Overrides?.Description ?? standardCard.Description,
                variantCard.Overrides?.FlavorText ?? standardCard.FlavorText,
                variantCard.Overrides?.Picture ?? standardCard.Picture,
                variantCard.Overrides?.Damage ?? standardCard.Damage,
                variantCard.Overrides?.Defense ?? standardCard.Defense,
                variantCard.Overrides?.Faction ?? standardCard.Faction,
                variantCard.Overrides?.Frame ?? standardCard.Frame,
                variantCard.Overrides?.Kind ?? standardCard.Kind,
                variantCard.Overrides?.Rank ?? standardCard.Rank,
                variantCard.Overrides?.Type ?? standardCard.Type,
                variantCard.Overrides?.Abilities ?? standardCard.Abilities,
                variantCard.Overrides?.PictureTransforms ?? standardCard.PictureTransforms,
                variantCard.Overrides?.UniqueAnimation ?? standardCard.UniqueAnimation,
                variantCard.Overrides?.Hidden ?? standardCard.Hidden,
                null
            );
        }
    }
}
