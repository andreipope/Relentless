using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Loom.ZombieBattleground
{
    public interface IAbility
    {
        BoardUnitModel UnitModelOwner { get; }
        Player PlayerOwner { get; }

        void DoAction();
    }
}
