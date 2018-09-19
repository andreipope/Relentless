using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ChangeGooVials : IAbility
    {
        private BoardObject _abilityUnitOwner;

        private GameObject _vfxObject;

        private IGameplayManager _gameplayManager;

        private CardsController _cardsController;

        public NewAbilityData AbilityData { get; private set; }

        public void Init(NewAbilityData data, BoardObject owner)
        {
            AbilityData = data;
            _abilityUnitOwner = owner;

            _gameplayManager = GameClient.Get<IGameplayManager>();
            _cardsController = _gameplayManager.GetController<CardsController>();
        }

        public void CallAction(object target)
        {
            //_vfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/GreenHealVFX");

            int Count = 0;
            int count = Count - (Constants.MaximumPlayerGoo - _abilityUnitOwner.OwnerPlayer.GooOnCurrentTurn);
            if (_abilityUnitOwner.OwnerPlayer.GooOnCurrentTurn >= Constants.MaximumPlayerGoo -1)
            {
                for (int i = 0; i < count; i++)
                {
                    _cardsController.AddCardToHand(_abilityUnitOwner.OwnerPlayer);
                }
            }

            _abilityUnitOwner.OwnerPlayer.GooOnCurrentTurn += AbilityData.NumberOfVials;
        }
    }
}
