using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public class PlayerActionFactory
    {
        private readonly string _playerId;

        public PlayerActionFactory(string playerId)
        {
            _playerId = playerId;
        }

        public PlayerAction EndTurn()
        {
            return new PlayerAction
            {
                ActionType = PlayerActionType.Types.Enum.EndTurn,
                PlayerId = _playerId,
                EndTurn = new PlayerActionEndTurn()
            };
        }

        public PlayerAction LeaveMatch()
        {
            return new PlayerAction
            {
                ActionType = PlayerActionType.Types.Enum.LeaveMatch,
                PlayerId = _playerId,
                LeaveMatch = new PlayerActionLeaveMatch()
            };
        }

        public PlayerAction Mulligan(IEnumerable<Data.InstanceId> cards)
        {
            return new PlayerAction
            {
                ActionType = PlayerActionType.Types.Enum.Mulligan,
                PlayerId = _playerId,
                Mulligan = new PlayerActionMulligan
                {
                    MulliganedCards =
                    {
                        cards.Select(card => card.ToProtobuf())
                    }
                }
            };
        }

        public PlayerAction CardPlay(WorkingCard card, int position)
        {
            return CardPlay(card.ToProtobuf(), position);
        }

        public PlayerAction CardPlay(CardInstance card, int position)
        {
            return new PlayerAction
            {
                ActionType = PlayerActionType.Types.Enum.CardPlay,
                PlayerId = _playerId,
                CardPlay = new PlayerActionCardPlay
                {
                    Card = card,
                    Position = position
                }
            };
        }

        public PlayerAction RankBuff(WorkingCard card, IEnumerable<Data.InstanceId> units)
        {
            PlayerActionRankBuff rankBuff = new PlayerActionRankBuff
            {
                Card = card.ToProtobuf()
            };

            foreach (Data.InstanceId unit in units)
            {
                Protobuf.Unit protoUnit = new Protobuf.Unit
                {
                    InstanceId = unit.ToProtobuf(),
                    AffectObjectType = AffectObjectType.Types.Enum.Character,
                    Parameter = new Parameter()
                };

                rankBuff.Targets.Add(protoUnit);
            }

            return new PlayerAction
            {
                ActionType = PlayerActionType.Types.Enum.RankBuff,
                PlayerId = _playerId,
                RankBuff = rankBuff
            };
        }

        public PlayerAction CardAbilityUsed(
            WorkingCard card,
            Enumerators.AbilityType abilityType,
            Enumerators.CardKind cardKind,
            Enumerators.AffectObjectType affectObjectType,
            IReadOnlyList<ParametrizedAbilityBoardObject> targets = null,
            IEnumerable<Data.InstanceId> cards = null
        )
        {
            PlayerActionCardAbilityUsed cardAbilityUsed = new PlayerActionCardAbilityUsed
            {
                CardKind = (CardKind.Types.Enum) cardKind,
                AbilityType = (CardAbilityType.Types.Enum) abilityType,
                Card = card.ToProtobuf()
            };

            Protobuf.Unit targetUnit;
            if (targets != null)
            {
                foreach (ParametrizedAbilityBoardObject parametrizedAbility in targets)
                {
                    if (parametrizedAbility.BoardObject == null)
                        continue;

                    targetUnit = new Protobuf.Unit();

                    if (parametrizedAbility.BoardObject is BoardUnitModel model)
                    {
                        targetUnit = new Protobuf.Unit
                        {
                            InstanceId = model.Card.InstanceId.ToProtobuf(),
                            AffectObjectType = AffectObjectType.Types.Enum.Character,
                            Parameter = new Parameter
                            {
                                Attack = parametrizedAbility.Parameters.Attack,
                                Defense = parametrizedAbility.Parameters.Defense,
                                CardName = parametrizedAbility.Parameters.CardName
                            }
                        };
                    }
                    else if (parametrizedAbility.BoardObject is Player player)
                    {
                        targetUnit = new Protobuf.Unit
                        {
                            InstanceId = new Data.InstanceId(player.InstanceId.Id).ToProtobuf(),
                            AffectObjectType = AffectObjectType.Types.Enum.Player,
                            Parameter = new Parameter()
                        };
                    }
                    else if (parametrizedAbility.BoardObject is HandBoardCard handCard)
                    {
                        targetUnit = new Protobuf.Unit
                        {
                            InstanceId = handCard.CardView.WorkingCard.InstanceId.ToProtobuf(),
                            AffectObjectType = AffectObjectType.Types.Enum.Card,
                            Parameter = new Parameter
                            {
                                Attack = parametrizedAbility.Parameters.Attack,
                                Defense = parametrizedAbility.Parameters.Defense,
                                CardName = parametrizedAbility.Parameters.CardName
                            }
                        };
                    }

                    cardAbilityUsed.Targets.Add(targetUnit);
                }
            }

            if (cards != null)
            {
                foreach (Data.InstanceId otherCard in cards)
                {
                    targetUnit = new Protobuf.Unit
                    {
                        InstanceId = otherCard.ToProtobuf(),
                        AffectObjectType = AffectObjectType.Types.Enum.Card,
                        Parameter = new Parameter()
                    };

                    cardAbilityUsed.Targets.Add(targetUnit);
                }
            }

            return new PlayerAction
            {
                ActionType = PlayerActionType.Types.Enum.CardAbilityUsed,
                PlayerId = _playerId,
                CardAbilityUsed = cardAbilityUsed
            };
        }

        public PlayerAction OverlordSkillUsed(SkillId skillId, Enumerators.AffectObjectType affectObjectType, Data.InstanceId targetInstanceId)
        {
            return new PlayerAction
            {
                ActionType = PlayerActionType.Types.Enum.OverlordSkillUsed,
                PlayerId = _playerId,
                OverlordSkillUsed = new PlayerActionOverlordSkillUsed
                {
                    SkillId = skillId.Id,
                    Target = new Protobuf.Unit
                    {
                        InstanceId = targetInstanceId.ToProtobuf(),
                        AffectObjectType = (AffectObjectType.Types.Enum) affectObjectType,
                        Parameter = new Parameter()
                    }
                }
            };
        }

        public PlayerAction CardAttack(Data.InstanceId attacker, Enumerators.AffectObjectType type, Data.InstanceId target)
        {
            return new PlayerAction
            {
                ActionType = PlayerActionType.Types.Enum.CardAttack,
                PlayerId = _playerId,
                CardAttack = new PlayerActionCardAttack
                {
                    Attacker = attacker.ToProtobuf(),
                    Target = new Protobuf.Unit
                    {
                        InstanceId = target.ToProtobuf(),
                        AffectObjectType = (Protobuf.AffectObjectType.Types.Enum) type,
                        Parameter = new Parameter()
                    }
                }
            };
        }
    }
}
