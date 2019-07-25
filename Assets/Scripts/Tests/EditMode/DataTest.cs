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
        private static readonly JsonSerializerSettings StringJsonSerializerSettings =
            JsonUtility.CreateStrictSerializerSettings((sender, args) => throw args.ErrorContext.Error);

        [Test]
        public void ZbVersionDeserialization()
        {
            string json =
                @"{""version"":{""id"":7,""major"":0,""minor"":1,""patch"":14,""environment"":""development"",""auth_url"":""https://dev-auth.loom.games"",""read_url"":""ws://battleground-testnet-asia2.dappchains.com:9999/queryws"",""write_url"":""ws://battleground-testnet-asia2.dappchains.com:46657/websocket"",""vault_url"":""https://dev-vault.delegatecall.com/v1"",""data_version"":""v6"",""is_maintenace_mode"":false,""is_force_update"":false,""download_url_pc"":""https://loom.games/releases/zombie-battleground-latest-pc.zip"",""download_url_mac"":""https://loom.games/releases/zombie-battleground-latest-mac.zip"",""download_url_app_store"":""https://itunes.apple.com/us/app/zombie-battleground-tcg/id1432628453"",""download_url_play_store"":""https://play.google.com/store/apps/details?id=games.loom.battleground"",""download_url_steam_store"":""https://store.steampowered.com/app/997630/Zombie_Battleground_TCG_BETA"",""plasmachain_chain_id"":""default"",""plasmachain_reader_host"":""wss://test-z-us1.dappchains.com/queryws"",""plasmachain_writer_host"":""wss://test-z-us1.dappchains.com/websocket"",""plasmachain_zbgcard_contract_address"":""0x2658d8c94062227d17a4ba61adb166e152369de3"",""plasmachain_cardfaucet_contract_address"":""0x42ac2c5ef756896b2820e5a2b433c5cc1ae7ca41"",""plasmachain_boosterpack_contract_address"":""0xdc745ac9945c981a63748a6b46dc31c2909bc865"",""plasmachain_superpack_contract_address"":""0xd05b46ffb3828218d5b7d9b1225575477c9e79d7"",""plasmachain_airpack_contract_address"":""0x4408927c62a6c8013612c11d630c222c130fd4f8"",""plasmachain_earthpack_contract_address"":""0x2926196ef74fe0611c474ba822c3b41b8796373e"",""plasmachain_firepack_contract_address"":""0x5a1a9d8d8cb5ce2e1f664133effb1ba0c9597074"",""plasmachain_lifepack_contract_address"":""0xcc8450ab3f874e741d187d897602ec5a72c4a0be"",""plasmachain_toxicpack_contract_address"":""0x750ffb9928d9fb1dd3b8b7eda47c130b410dde72"",""plasmachain_waterpack_contract_address"":""0x4f5b70188f14b6d80e8bf0002eca2ab2863ece5e"",""plasmachain_binancepack_contract_address"":""0x837da2498b31d1654d51c1871b10fc4e3d192f02"",""plasmachain_fiatpurchase_contract_address"":""0xaff6212ab34f4066ee46f4b20429b2c74726eb67"",""plasmachain_openlottery_contract_address"":""0xdeadbeefdeadbeefdeadbeefdeadbeefdeadbeef"",""plasmachain_tronlottery_contract_address"":""""}}";

            ZbVersion zbVersion = JsonConvert.DeserializeObject<ZbVersion>(json, JsonUtility.CreateStrictSerializerSettings((sender, args) => throw args.ErrorContext.Error));
            Assert.NotNull(zbVersion);
            Assert.AreEqual("https://dev-auth.loom.games", zbVersion.Version.AuthUrl);
            Assert.AreEqual("default", zbVersion.Version.PlasmachainChainId);
            Assert.AreEqual("wss://test-z-us1.dappchains.com/websocket", zbVersion.Version.PlasmachainWriterHost);
            Assert.AreEqual("0xdc745ac9945c981a63748a6b46dc31c2909bc865", zbVersion.Version.PlasmachainBoosterPackContractAddress);
            Assert.AreEqual("", zbVersion.Version.PlasmachainTronLotteryContractAddress);
        }
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
        public void PictureTransformDeserialization()
        {
            string json =
                "        {\r\n            \"position\": {\r\n                \"x\": 0.07,\r\n                \"y\": 0.02,\r\n                \"z\": 0\r\n            },\r\n            \"scale\": {\r\n                \"x\": 0.9,\r\n                \"y\": 0.9,\r\n                \"z\": 0.9\r\n            }\r\n        }";
            PictureTransform pictureTransform = JsonConvert.DeserializeObject<PictureTransform>(json, StringJsonSerializerSettings);
            Assert.AreEqual(0.07f, pictureTransform.Position.X);
            Assert.AreEqual(0.02f, pictureTransform.Position.Y);
            Assert.AreEqual(0f, pictureTransform.Position.Z);
            Assert.AreEqual(0.9f, pictureTransform.Scale.X);
            Assert.AreEqual(0.9f, pictureTransform.Scale.Y);
            Assert.AreEqual(0.9f, pictureTransform.Scale.Z);
        }

        [Test]
        public void CardSerialization()
        {
            Card cardPrototype = new Card(
                new CardKey(new MouldId(123), Enumerators.CardVariant.Standard),
                Enumerators.CardSet.KickstarterExclusive,
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

            string cardPrototypeSerializedToJson = JsonConvert.SerializeObject(cardPrototype, Formatting.Indented, StringJsonSerializerSettings);
            Card cardPrototypeDeserializedFromJson = JsonConvert.DeserializeObject<Card>(cardPrototypeSerializedToJson, StringJsonSerializerSettings);
            cardPrototype.ShouldDeepEqual(cardPrototypeDeserializedFromJson);

            WorkingCard workingCardProtobuf = new WorkingCard(cardPrototype, cardPrototype, null, new Data.InstanceId(373));
            WorkingCard workingCardDeserializedFromProtobuf = workingCardProtobuf.ToProtobuf().FromProtobuf(null);
            workingCardProtobuf.ShouldDeepEqual(workingCardDeserializedFromProtobuf);
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
