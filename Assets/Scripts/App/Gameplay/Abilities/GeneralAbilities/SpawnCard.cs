using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class SpawnCard : IAbility
    {
        private BoardObject _abilityUnitOwner;

        private GameObject _vfxObject;

        public Enumerators.StatType StatType { get; }

        public NewAbilityData AbilityData { get; private set; }

        private const int CORRUPTED_GOO_MOULD_ID = 156;

        private const int TAINTED_GOO_MOULD_ID = 155;


        public void Init(NewAbilityData data, BoardObject owner)
        {
            AbilityData = data;
            _abilityUnitOwner = owner;
        }

        public void CallAction(object target)
        {
            //_vfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");
            if (AbilityData.MouldId != TAINTED_GOO_MOULD_ID && AbilityData.MouldId != CORRUPTED_GOO_MOULD_ID ||
                 (AbilityData.MouldId == TAINTED_GOO_MOULD_ID || AbilityData.MouldId == CORRUPTED_GOO_MOULD_ID) &&
                 (_abilityUnitOwner as BoardUnit).Card.LibraryCard.CardSetType == _abilityUnitOwner.OwnerPlayer.SelfHero.HeroElement)
            {
                GameClient.Get<IGameplayManager>().GetController<CardsController>().CreateNewCardByIdAndAddToHand(_abilityUnitOwner.OwnerPlayer, AbilityData.MouldId);
            }
        }
    }
}
