// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class GameplayActionReport_HealPlayerByAbility : ReportViewBase
    {
        private Player _callerPlayer;
        private AbilityData _usedAbility;
        private Player _abilityUsedOnPlayer;

        public GameplayActionReport_HealPlayerByAbility(GameObject prefab, Transform parent, GameActionReport gameAction) : base(prefab, parent, gameAction) { }

        public override void SetInfo()
        {
            base.SetInfo();
            _callerPlayer = gameAction.parameters[0] as Player;
            _usedAbility = gameAction.parameters[1] as AbilityData;
            _abilityUsedOnPlayer = gameAction.parameters[2] as Player;
        }

        public override void OnPointerEnterEventHandler(PointerEventData obj)
        {
            base.OnPointerEnterEventHandler(obj);
        }

        public override void OnPointerExitEventHandler(PointerEventData obj)
        {
            base.OnPointerExitEventHandler(obj);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

    }
}
