using System;
using System.Collections.Generic;
using DeepEqual.Syntax;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.Protobuf;
using NUnit.Framework;
using Card = Loom.ZombieBattleground.Data.Card;
using PictureTransform = Loom.ZombieBattleground.Data.PictureTransform;
using Deck = Loom.ZombieBattleground.Data.Deck;
using Hero = Loom.ZombieBattleground.Data.Hero;

namespace Loom.ZombieBattleground.Test
{
    [Category("QuickSubset")]
    public class DataTest
    {
        [Test]
        public void DeckProtobufSerialization()
        {
            Deck original = new Deck(
                1,
                2,
                "deck name",
                new List<DeckCardData>
                {
                    new DeckCardData("card 1", 3),
                    new DeckCardData("card 2", 4)
                },
                Enumerators.OverlordSkill.HEALING_TOUCH,
                Enumerators.OverlordSkill.MEND
            );

            Deck deserialized = original.ToProtobuf().FromProtobuf();
            original.ShouldDeepEqual(deserialized);
        }

        [Test]
        public void CardProtobufSerialization()
        {
            Card original = new Card(
                123,
                "Foo",
                3,
                "description",
                "flavor",
                "awesomePicture",
                4,
                5,
                Enumerators.Faction.ITEM,
                "awesomeFrame",
                Enumerators.CardKind.CREATURE,
                Enumerators.CardRank.GENERAL,
                Enumerators.CardType.WALKER,
                new List<AbilityData>
                {
                    CreateAbilityData(true,
                        () => new List<AbilityData.ChoosableAbility>
                        {
                            new AbilityData.ChoosableAbility("choosable ability 1", CreateAbilityData(false, null), ""),
                            new AbilityData.ChoosableAbility("choosable ability 2", CreateAbilityData(false, null), "")
                        })
                },
                new PictureTransform(
                    new FloatVector3(0.3f, 0.4f, 0.5f),
                    FloatVector3.One
                ),
                Enumerators.UniqueAnimation.ShammannArrival,
                false
            );

            Card deserialized = original.ToProtobuf().FromProtobuf();
            original.ShouldDeepEqual(deserialized);
        }

        [Test]
        public void HeroProtobufSerialization()
        {
            Protobuf.Hero protobuf = new Protobuf.Hero
            {
                HeroId = 1,
                Icon = "icon",
                Name = "name",
                ShortDescription = "short desc",
                LongDescription = "long desc",
                Experience = 100500,
                Level = 373,
                Faction = Protobuf.Faction.Types.Enum.Life,
                Skills =
                {
                    new Skill
                    {
                        Title = "title",
                        IconPath = "supericon",
                        Description = "desc",
                        Cooldown = 1,
                        InitialCooldown = 2,
                        Value = 3,
                        Damage = 4,
                        Count = 5,
                        Skill_ = OverlordSkillKind.Types.Enum.Freeze,
                        SkillTargets =
                        {
                            OverlordAbilityTarget.Types.Enum.Opponent,
                            OverlordAbilityTarget.Types.Enum.AllCards
                        },
                        TargetUnitSpecialStatus = UnitSpecialStatus.Types.Enum.Frozen,
                        FactionTargets =
                        {
                            Protobuf.Faction.Types.Enum.Fire,
                            Protobuf.Faction.Types.Enum.Life
                        },
                        Unlocked = true,
                        CanSelectTarget = true
                    }
                },
                PrimarySkill =  OverlordSkillKind.Types.Enum.HealingTouch,
                SecondarySkill = OverlordSkillKind.Types.Enum.Mend
            };

            Hero client = new Hero(
                1,
                "icon",
                "name",
                "short desc",
                "long desc",
                100500,
                373,
                Enumerators.Faction.LIFE,
                new List<HeroSkill>
                {
                    new HeroSkill(
                        0,
                        "title",
                        "supericon",
                        "desc",
                        1,
                        2,
                        3,
                        4,
                        5,
                        Enumerators.OverlordSkill.FREEZE,
                        new List<Enumerators.SkillTargetType>
                        {
                            Enumerators.SkillTargetType.OPPONENT,
                            Enumerators.SkillTargetType.ALL_CARDS
                        },
                        Enumerators.UnitStatus.FROZEN,
                        new List<Enumerators.Faction>
                        {
                            Enumerators.Faction.FIRE,
                            Enumerators.Faction.LIFE
                        },
                        true,
                        true,
                        false
                    )
                },
                Enumerators.OverlordSkill.HEALING_TOUCH,
                Enumerators.OverlordSkill.MEND
            );

            client.ShouldDeepEqual(protobuf.FromProtobuf());
        }

        private static AbilityData CreateAbilityData(bool includeChoosableAbility, Func<List<AbilityData.ChoosableAbility>> choosableAbilityFunc)
        {
            List<AbilityData.ChoosableAbility> choosableAbilities = new List<AbilityData.ChoosableAbility>();
            if (includeChoosableAbility)
            {
                choosableAbilities = choosableAbilityFunc();
            }

            return
                new AbilityData(
                    Enumerators.AbilityType.RAGE,
                    Enumerators.AbilityActivity.ACTIVE,
                    Enumerators.AbilityTrigger.IN_HAND,
                    new List<Enumerators.Target>
                    {
                        Enumerators.Target.ITSELF,
                        Enumerators.Target.PLAYER
                    },
                    Enumerators.Stat.DAMAGE,
                    Enumerators.Faction.TOXIC,
                    Enumerators.AbilityEffect.TARGET_ROCK,
                    Enumerators.AttackRestriction.ONLY_DIFFERENT,
                    Enumerators.CardType.WALKER,
                    Enumerators.UnitStatus.FROZEN,
                    Enumerators.CardType.HEAVY,
                    1,
                    2,
                    3,
                    "nice name",
                    4,
                    5,
                    6,
                    new List<AbilityData.VisualEffectInfo>
                    {
                        new AbilityData.VisualEffectInfo(Enumerators.VisualEffectType.Impact, "path1"),
                        new AbilityData.VisualEffectInfo(Enumerators.VisualEffectType.Moving, "path2")
                    },
                    Enumerators.GameMechanicDescription.Death,
                    Enumerators.Faction.LIFE,
                    Enumerators.AbilitySubTrigger.AllAllyUnitsInPlay,
                    choosableAbilities,
                    7,
                    8
                );
        }
    }
}
