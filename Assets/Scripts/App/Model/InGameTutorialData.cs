using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;

namespace Loom.ZombieBattleground
{
    public class InGameTutorialData
    {
        public int Id;
        public string Description;
        public Enumerators.TutorialActivityAction ActivityAction;
        public Enumerators.TooltipAlign Align;
        public bool AppearOnce;
        public bool IsEnabled;
        public Enumerators.TutorialObjectOwner Owner;
        public FloatVector2 Position;
        public float AppearDelay;
        public string Tag;
    }
}
