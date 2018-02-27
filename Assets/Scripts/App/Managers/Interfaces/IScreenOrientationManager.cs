using GrandDevs.CZB.Common;

namespace GrandDevs.CZB
{
    public interface IScreenOrientationManager
    {
        Enumerators.ScreenOrientationMode CurrentOrientation { get; }

        void SwitchOrientation(Enumerators.ScreenOrientationMode mode);
    }
}