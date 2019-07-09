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

        public bool HasUniqueAnimation(CardModel cardModel)
        {
            return cardModel.Card.Prototype.UniqueAnimation != Enumerators.UniqueAnimation.None;
        }

        public void PlayUniqueArrivalAnimation(IBoardObject boardObject, WorkingCard card, Action startGeneralArrivalCallback, Action endArrivalCallback)
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
                case Enumerators.UniqueAnimation.MountainArrival:
                    uniqueAnimation = new MountainArrivalUniqueAnimation();
                    break;
                case Enumerators.UniqueAnimation.GargantuaArrival:
                    uniqueAnimation = new GargantuaArrivalUniqueAnimation();
                    break;
                case Enumerators.UniqueAnimation.BlizzardArrival:
                    uniqueAnimation = new BlizzardArrivalUniqueAnimation();
                    break;
                case Enumerators.UniqueAnimation.GoozillaArrival:
                    uniqueAnimation = new GoozillaArrivalUniqueAnimation();
                    break;
                default:
                    throw new NotImplementedException(nameof(uniqueAnimationType) + " not implemented yet");
            }

            return uniqueAnimation;
        }
    }
}
