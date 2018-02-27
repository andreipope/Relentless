// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace CCGKit
{
    /// <summary>
    /// The available effect targets.
    /// </summary>
    public enum EffectTarget
    {
        Player,
        Opponent,
        TargetPlayer,
        RandomPlayer,
        AllPlayers,
        ThisCard,
        PlayerCard,
        OpponentCard,
        TargetCard,
        RandomPlayerCard,
        RandomOpponentCard,
        RandomCard,
        AllPlayerCards,
        AllOpponentCards,
        AllCards,
        PlayerOrPlayerCreature,
        OpponentOrOpponentCreature,
        AnyPlayerOrCreature
    }

    /// <summary>
    /// The base class for targets.
    /// </summary>
    public abstract class Target
    {
        public virtual EffectTarget GetTarget()
        {
            return EffectTarget.Player;
        }
    }

    public interface IPlayerTarget
    {
    }

    public interface ICardTarget
    {
    }

    public interface IUserTarget
    {
    }

    public interface IComputedTarget
    {
    }

    public abstract class PlayerTargetBase : Target, IPlayerTarget
    {
        public List<PlayerCondition> conditions = new List<PlayerCondition>();
    }

    public abstract class CardTargetBase : Target, ICardTarget
    {
        public List<CardCondition> conditions = new List<CardCondition>();
    }

    public class PlayerTarget : PlayerTargetBase, IComputedTarget
    {
        public override EffectTarget GetTarget()
        {
            return EffectTarget.Player;
        }
    }

    public class OpponentTarget : PlayerTargetBase, IComputedTarget
    {
        public override EffectTarget GetTarget()
        {
            return EffectTarget.Opponent;
        }
    }

    public class TargetPlayer : PlayerTargetBase, IUserTarget
    {
        public override EffectTarget GetTarget()
        {
            return EffectTarget.TargetPlayer;
        }
    }

    public class RandomPlayer : PlayerTargetBase, IComputedTarget
    {
        public override EffectTarget GetTarget()
        {
            return EffectTarget.RandomPlayer;
        }
    }

    public class AllPlayers : PlayerTargetBase, IComputedTarget
    {
        public override EffectTarget GetTarget()
        {
            return EffectTarget.AllPlayers;
        }
    }

    public class ThisCard : CardTargetBase, IComputedTarget
    {
        public override EffectTarget GetTarget()
        {
            return EffectTarget.ThisCard;
        }
    }

    public class PlayerCard : CardTargetBase, IUserTarget
    {
        public override EffectTarget GetTarget()
        {
            return EffectTarget.PlayerCard;
        }
    }

    public class OpponentCard : CardTargetBase, IUserTarget
    {
        public override EffectTarget GetTarget()
        {
            return EffectTarget.OpponentCard;
        }
    }

    public class TargetCard : CardTargetBase, IUserTarget
    {
        public override EffectTarget GetTarget()
        {
            return EffectTarget.TargetCard;
        }
    }

    public class RandomPlayerCard : CardTargetBase, IComputedTarget
    {
        public override EffectTarget GetTarget()
        {
            return EffectTarget.RandomPlayerCard;
        }
    }

    public class RandomOpponentCard : CardTargetBase, IComputedTarget
    {
        public override EffectTarget GetTarget()
        {
            return EffectTarget.RandomOpponentCard;
        }
    }

    public class RandomCard : CardTargetBase, IComputedTarget
    {
        public override EffectTarget GetTarget()
        {
            return EffectTarget.RandomCard;
        }
    }

    public class AllPlayerCards : CardTargetBase, IComputedTarget
    {
        public override EffectTarget GetTarget()
        {
            return EffectTarget.AllPlayerCards;
        }
    }

    public class AllOpponentCards : CardTargetBase, IComputedTarget
    {
        public override EffectTarget GetTarget()
        {
            return EffectTarget.AllOpponentCards;
        }
    }

    public class AllCards : CardTargetBase, IComputedTarget
    {
        public override EffectTarget GetTarget()
        {
            return EffectTarget.AllCards;
        }
    }
}
