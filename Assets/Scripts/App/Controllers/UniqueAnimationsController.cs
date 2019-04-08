using System.Collections.Generic;
using UnityEngine;
using System;
using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{
    public class UniqueAnimationsController : IController
    {
        public void Dispose()
        {
        }

        public void Init()
        {
        }

        public void ResetAll()
        {
        }

        public void Update()
        {
        }

        public bool HasUniqueAnimation(BoardUnitModel boardUnitModel)
        {
            return boardUnitModel.Card.Prototype.UniqueAnimation != Enumerators.UniqueAnimation.None;
        }

        public void PlayUniqueArrivalAnimation(BoardObject boardObject, WorkingCard card, Action startGeneralArrivalCallback, Action endArrivalCallback)
        {
            UniqueAnimation animation = GetUniqueAnimationByType(card.Prototype.UniqueAnimation);
            animation.Play(boardObject, startGeneralArrivalCallback, endArrivalCallback);
        }

        private UniqueAnimation GetUniqueAnimationByType(Enumerators.UniqueAnimation uniqueAnimationType)
        {
            UniqueAnimation uniqueAnimation;

            switch (uniqueAnimationType)
            {
                case Enumerators.UniqueAnimation.ShammannArrival:
                    uniqueAnimation = new ShammannArrivalUniqueAnimation();
                    break;
                case Enumerators.UniqueAnimation.ZVirusArrival:
                    uniqueAnimation = new ZVirusArrivalUniqueAnimation();
                    break;
                case Enumerators.UniqueAnimation.ZeuzArrival:
                    uniqueAnimation = new ZeuZArrivalUniqueAnimation();
                    break;
                case Enumerators.UniqueAnimation.CerberusArrival:
                    uniqueAnimation = new CerberusArrivalUniqueAnimation();
                    break;
                case Enumerators.UniqueAnimation.TzunamyArrival:
                    uniqueAnimation = new TzunamyArrivalUniqueAnimation();
                    break;
                case Enumerators.UniqueAnimation.ChernoBillArrival:
                    uniqueAnimation = new ChernoBillArrivalUniqueAnimation();
                    break;
                default:
                    throw new NotImplementedException(nameof(uniqueAnimationType) + " not implemented yet");
            }

            return uniqueAnimation;
        }
    }
}
