using UnityEngine;

namespace Loom.ZombieBattleground
{
    /// <summary>
    /// When switching from windowed mode to fullscreen, Unity retains the rendering resolution of the window.
    /// This is not ideal, since we want the game to render at desktop resolution when in fullscreen.
    /// This class fixes that by switching to the desktop resolution on fullscreen switch.
    /// </summary>
    public class CorrectResolutionSwitch : MonoBehaviour
    {
        private FullScreenMode _prevFullScreenMode;
        private Resolution _prevResolution;

        private void Awake()
        {
            _prevFullScreenMode = Screen.fullScreenMode;
            _prevResolution = Screen.currentResolution;
        }

        private void Update()
        {
            FullScreenMode fullScreenMode = Screen.fullScreenMode;
            if (fullScreenMode != _prevFullScreenMode)
            {
                if (_prevFullScreenMode == FullScreenMode.Windowed &&
                    (fullScreenMode == FullScreenMode.FullScreenWindow ||
                        fullScreenMode == FullScreenMode.ExclusiveFullScreen))
                {
                    Screen.SetResolution(_prevResolution.width, _prevResolution.height, fullScreenMode);
                }

                _prevFullScreenMode = fullScreenMode;
            }

            _prevResolution = Screen.currentResolution;
        }
    }
}
