using System;
using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class ApplicationSettingsManager : IService, IApplicationSettingsManager
    {
        public List<ResolutionInfo> Resolutions { get; private set; }

        public Enumerators.QualityLevel CurrentQualityLevel { get; private set; }

        public Enumerators.ScreenMode CurrentScreenMode { get; private set; }

        public ResolutionInfo CurrentResolution { get; private set; }

        public void Dispose()
        {
        }

        public void Init()
        {
            FillResolutions();
        }

        public void Update()
        {
        }

        public void SetResolution(IntVector2 size)
        {
            Screen.SetResolution(size.X, size.Y, CurrentScreenMode == Enumerators.ScreenMode.FULL_SCREEN ? true : false);

            SetScreenMode(CurrentScreenMode);
        }

        public void SetScreenMode(Enumerators.ScreenMode screenMode)
        {
            CurrentScreenMode = screenMode;

            switch (screenMode)
            {
                case Enumerators.ScreenMode.FULL_SCREEN:
                    {
                        Screen.fullScreen = true;
                        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    }
                    break;
                case Enumerators.ScreenMode.WINDOW:
                    {
                        Screen.fullScreen = false;
                        Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
                    }
                    break;
                case Enumerators.ScreenMode.BORDERLESS_WINDOW:
                    {
                        Screen.fullScreen = false;
                        Screen.fullScreenMode = FullScreenMode.Windowed;
                    }
                    break;
                default: break;
            }
        }

        public void SetQuality(Enumerators.QualityLevel qualityLevel)
        {
            CurrentQualityLevel = qualityLevel;

            switch (qualityLevel)
            {
                case Enumerators.QualityLevel.ULTRA:
                    {
                    }
                    break;
                case Enumerators.QualityLevel.HIGH:
                    {

                    }
                    break;
                case Enumerators.QualityLevel.MEDIUM:
                    {

                    }
                    break;
                case Enumerators.QualityLevel.LOW:
                    {

                    }
                    break;
                default: break;
            }
        }

        private void FillResolutions()
        {
            Resolutions = new List<ResolutionInfo>();

            ResolutionInfo info;
            foreach (Resolution resolution in Screen.resolutions)
            {
                info = new ResolutionInfo()
                {
                    name = $"{resolution.width} x {resolution.height}",
                    resolution = new IntVector2(resolution.width, resolution.height)
                };

                Resolutions.Add(info);
            }
        }
    }

    public class ResolutionInfo
    {
        public string name;
        public IntVector2 resolution;
    }
}
