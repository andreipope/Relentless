using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using InstanceId = Loom.ZombieBattleground.Data.InstanceId;

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

        public PlayerAction Mulligan(IEnumerable<InstanceId> cards)
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

        public PlayerAction CardPlay(InstanceId card, int position)
        {
            return new PlayerAction
            {
                ActionType = PlayerActionType.Types.Enum.CardPlay,
                PlayerId = _playerId,
                CardPlay = new PlayerActionCardPlay
                {
                    Card = card.ToProtobuf(),
                    Position = position
                }
            };
        }

        public PlayerAction RankBuff(InstanceId card, IEnumerable<InstanceId> units)
        {
            PlayerActionRankBuff rankBuff = new PlayerActionRankBuff
            {
                Card = card.ToProtobuf()
            };

            foreach (InstanceId unit in units)
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
            InstanceId card,
            int abilityIndex,
            int choosableAbilityIndex,
            IReadOnlyList<ParametrizedAbilityBoardObject> targets = null
        )
        {
            List<ParametrizedAbilityInstanceId> parametrizedTargetsInstanceIds = new List<ParametrizedAbilityInstanceId>();

            if (targets != null)
            {
                foreach (ParametrizedAbilityBoardObject target in targets)
                {
                    if (target.BoardObject == null)
                        continue;

                    ParametrizedAbilityInstanceId targetParametrizedInstanceId;
                    InstanceId instanceId;
                    Enumerators.AffectObjectType affectObjectType;

                    switch (target.BoardObject)
                    {
                        case BoardUnitModel model:
                            instanceId = model.Card.InstanceId;
                            affectObjectType = Enumerators.AffectObjectType.Character;
                            break;
                        case Player player:
                            instanceId = player.InstanceId;
                            affectObjectType = Enumerators.AffectObjectType.Player;
                            break;
                        case HandBoardCard handCard:
                            instanceId = handCard.CardView.WorkingCard.InstanceId;
                            affectObjectType = Enumerators.AffectObjectType.Card;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    targetParametrizedInstanceId = new ParametrizedAbilityInstanceId(instanceId, affectObjectType, target.Parameters);
                    parametrizedTargetsInstanceIds.Add(targetParametrizedInstanceId);
                }
            }

            return CardAbilityUsed(card, abilityIndex, choosableAbilityIndex, parametrizedTargetsInstanceIds);
        }

        public PlayerAction CardAbilityUsed(
            InstanceId card,
            int abilityIndex,
            int choosableAbilityIndex,
            IReadOnlyList<ParametrizedAbilityInstanceId> targets = null
        )
        {
            List<Protobuf.Unit> unitTargets = new List<Protobuf.Unit>();

            if (targets != null)
            {
                foreach (ParametrizedAbilityInstanceId target in targets)
                {
                    Protobuf.Unit targetUnit = new Protobuf.Unit
                    {
                        InstanceId = target.Id.ToProtobuf(),
                        AffectObjectType = (AffectObjectType.Types.Enum) target.AffectObjectType,
                        Parameter = target.Parameters.ToProtobuf()
                    };

                    unitTargets.Add(targetUnit);
                }
            }

            return CardAbilityUsed(card, abilityIndex, choosableAbilityIndex, unitTargets);
        }

        public PlayerAction CardAbilityUsed(
            InstanceId card,
            int abilityIndex,
            int choosableAbilityIndex,
            IReadOnlyList<Protobuf.Unit> targets = null
        )
        {
            PlayerActionCardAbilityUsed cardAbilityUsed = new PlayerActionCardAbilityUsed
            {
                AbilityIndex = abilityIndex,
                ChoosableAbilityIndex = choosableAbilityIndex,
                Card = card.ToProtobuf(),
                Targets = { targets }
            };

            return new PlayerAction
            {
                ActionType = PlayerActionType.Types.Enum.CardAbilityUsed,
                PlayerId = _playerId,
                CardAbilityUsed = cardAbilityUsed
            };
        }

        public PlayerAction OverlordSkillUsed(SkillId skillId, Enumerators.AffectObjectType affectObjectType, InstanceId target)
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
                        InstanceId = target.ToProtobuf(),
                        AffectObjectType = (AffectObjectType.Types.Enum) affectObjectType,
                        Parameter = new Parameter()
                    }
                }
            };
        }

        public PlayerAction CardAttack(InstanceId attacker, Enumerators.AffectObjectType type, InstanceId target)
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
                        AffectObjectType = (AffectObjectType.Types.Enum) type,
                        Parameter = new Parameter()
                    }
                }
            };
        }

        public PlayerAction CheatDestroyCardsOnBoard(IEnumerable<InstanceId> targets)
        {
            return new PlayerAction
            {
                ActionType = PlayerActionType.Types.Enum.CheatDestroyCardsOnBoard,
                PlayerId = _playerId,
                CheatDestroyCardsOnBoard = new PlayerActionCheatDestroyCardsOnBoard
                {
                    DestroyedCards =
                    {
                        targets.Select(card => card.ToProtobuf())
                    }
                }
            };
        }
    }
}
