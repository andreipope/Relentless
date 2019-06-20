using System;
using System.Collections.Generic;
using System.Numerics;
using DeepEqual.Syntax;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.Iap;
using Loom.ZombieBattleground.Protobuf;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using AbilityData = Loom.ZombieBattleground.Data.AbilityData;
using Card = Loom.ZombieBattleground.Data.Card;
using PictureTransform = Loom.ZombieBattleground.Data.PictureTransform;
using Deck = Loom.ZombieBattleground.Data.Deck;
using CardKey = Loom.ZombieBattleground.Data.CardKey;

namespace Loom.ZombieBattleground.Test
{
    [Category("EditQuickSubset")]
    public class DataTest
    {
        [Test]
        public void TransactionResponseSerialization()
        {
            string json =
                @"{""VerifyHash"":{""hash"":""0xf1a0b8586d04cf9fab76636aa859b575c8e8eae18bfc57f093d1c97703a3eca9"",""signature"":""0x59e2e78ae04a206aad5639f120df820fd971ff3242b7afc1ec6ab7ffdeb2f25716b2dc6e403046446118e41dea6ed0753a867272ca1600b160abb9185f1717141b""},""UserId"":1,""Booster"":1,""Air"":0,""Earth"":0,""Fire"":0,""Life"":0,""Toxic"":0,""Water"":0,""Super"":0,""Small"":0,""Minion"":0,""Binance"":0,""TxID"":170141183460469231731687303715884105729}";

            AuthFiatApiFacade.TransactionReceipt transactionReceipt = JsonConvert.DeserializeObject<AuthFiatApiFacade.TransactionReceipt>(json);
            Assert.AreEqual(BigInteger.Parse("170141183460469231731687303715884105729"), transactionReceipt.TxId);
        }

        [Test]
        public void BigIntegerSerialization()
        {
            BigInteger original = new BigInteger(long.MaxValue) * new BigInteger(long.MaxValue);
            BigInteger deserialized = original.ToProtobufUInt().FromProtobuf();
            Assert.AreEqual(original, deserialized);
        }

        [Test]
        public void CardKeySerialization()
        {
            CardKey original = new CardKey(new MouldId(long.MaxValue), Enumerators.CardVariant.Limited);
            string serialized = JsonConvert.SerializeObject(original);
            CardKey deserialized = JsonConvert.DeserializeObject<CardKey>(serialized);
            Assert.AreEqual(original, deserialized);
        }

        [Test]
        public void DeckProtobufSerialization()
        {
            Deck original = new Deck(
                new DeckId(1),
                new OverlordId(2),
                "deck name",
                new List<DeckCardData>
                {
                    new DeckCardData(new CardKey(new MouldId(1), Enumerators.CardVariant.Standard), 3),
                    new DeckCardData(new CardKey(new MouldId(2), Enumerators.CardVariant.Standard), 4)
                },
                Enumerators.Skill.HEALING_TOUCH,
                Enumerators.Skill.MEND
            );

            Deck deserialized = original.ToProtobuf().FromProtobuf();
            original.ShouldDeepEqual(deserialized);
        }

        [Test]
        public void CardProtobufSerialization()
        {
            Card card = new Card(
                new CardKey(new MouldId(123), Enumerators.CardVariant.Standard),
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
                true
            );

            WorkingCard original = new WorkingCard(card, card, null, new Data.InstanceId(373));
            WorkingCard deserialized = original.ToProtobuf().FromProtobuf(null);
            original.ShouldDeepEqual(deserialized);
        }

        [Test]
        public void OverlordProtobufSerialization()
        {
            Protobuf.OverlordPrototype protobufPrototype = new Protobuf.OverlordPrototype
            {
                Id = 1,
                Icon = "icon",
                Name = "name",
                ShortDescription = "short desc",
                LongDescription = "long desc",

                Faction = Protobuf.Faction.Types.Enum.Life,
                Skills =
                {
                    new Protobuf.OverlordSkillPrototype
                    {
                        Id = 333,
                        Title = "title",
                        IconPath = "supericon",
                        Description = "desc",
                        Cooldown = 1,
                        InitialCooldown = 2,
                        Value = 3,
                        Damage = 4,
                        Count = 5,
                        Skill = OverlordSkillType.Types.Enum.Freeze,
                        SkillTargets =
                        {
                            SkillTarget.Types.Enum.Opponent,
                            SkillTarget.Types.Enum.AllCards
                        },
                        TargetUnitSpecialStatus = UnitSpecialStatus.Types.Enum.Frozen,
                        TargetFactions =
                        {
                            Protobuf.Faction.Types.Enum.Fire,
                            Protobuf.Faction.Types.Enum.Life
                        },
                        CanSelectTarget = true,
                        SingleUse = true
                    }
                },
                InitialDefense = 50
            };

            Protobuf.OverlordUserData protobufUserData = new Protobuf.OverlordUserData
            {
                Level = 373,
                Experience = 100500,
                UnlockedSkillIds = { 1, 2, 3 }
            };

            Data.OverlordPrototype clientPrototype = new Data.OverlordPrototype(
                new OverlordId(1),
                "icon",
                "name",
                "short desc",
                "long desc",
                Enumerators.Faction.LIFE,
                new List<Data.OverlordSkillPrototype>
                {
                    new Data.OverlordSkillPrototype(
                        new SkillId(333),
                        "title",
                        "supericon",
                        "desc",
                        1,
                        2,
                        3,
                        4,
                        5,
                        Enumerators.Skill.FREEZE,
                        new List<Enumerators.SkillTarget>
                        {
                            Enumerators.SkillTarget.OPPONENT,
                            Enumerators.SkillTarget.ALL_CARDS
                        },
                        Enumerators.UnitSpecialStatus.FROZEN,
                        new List<Enumerators.Faction>
                        {
                            Enumerators.Faction.FIRE,
                            Enumerators.Faction.LIFE
                        },
                        true,
                        true
                    )
                },
                50
            );

            Data.OverlordUserData clientUserData = new Data.OverlordUserData(
                373,
                100500
            );

            clientPrototype.ShouldDeepEqual(protobufPrototype.FromProtobuf());
            clientUserData.ShouldDeepEqual(protobufUserData.FromProtobuf());
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
                    Enumerators.UnitSpecialStatus.FROZEN,
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
                    8,
                    Enumerators.CardKind.CREATURE,
                    new List<Enumerators.GameMechanicDescription>
                    {
                        Enumerators.GameMechanicDescription.Death,
                        Enumerators.GameMechanicDescription.Aura
                    }
                );
        }
    }
}
