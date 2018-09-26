using System.Collections.Generic;
using System.ComponentModel;
using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ApplicationSettingsManager : IService, IApplicationSettingsManager
    {
        private IDataManager _dataManager;

        public List<ResolutionInfo> Resolutions { get; private set; }

        public Enumerators.QualityLevel CurrentQualityLevel { get; private set; }

        public Enumerators.ScreenMode CurrentScreenMode { get; private set; }

        public ResolutionInfo CurrentResolution { get; private set; }

        public void Dispose()
        {
        }

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();

            FillResolutions();
        }

        public void Update()
        {
            HandleSpecificUserActions();
        }


        public void ApplySettings()
        {
            CurrentQualityLevel = _dataManager.CachedUserLocalData.AppQualityLevel;
            CurrentScreenMode = _dataManager.CachedUserLocalData.AppScreenMode;
            CurrentResolution = Resolutions.Find(x => x.Resolution.x == _dataManager.CachedUserLocalData.AppResolution.x &&
                                                      x.Resolution.y == _dataManager.CachedUserLocalData.AppResolution.y);
        }

        public void SetDefaults()
        {
            SetScreenMode(Enumerators.ScreenMode.Window);
            SetQuality(Enumerators.QualityLevel.Ultra);
            SetResolution(Resolutions[Resolutions.Count - 1]);
        }

        public void SetResolution(ResolutionInfo info)
        {
            CurrentResolution = info;

            Screen.SetResolution(info.Resolution.x, info.Resolution.y, CurrentScreenMode == Enumerators.ScreenMode.FullScreen ? true : false);

            SetScreenMode(CurrentScreenMode);

            _dataManager.CachedUserLocalData.AppResolution = CurrentResolution.Resolution;
            _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
        }

        public void SetScreenMode(Enumerators.ScreenMode screenMode)
        {
            CurrentScreenMode = screenMode;

            switch (screenMode)
            {
                case Enumerators.ScreenMode.FullScreen:
                    {
                        Screen.fullScreen = true;
                        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    }
                    break;
                case Enumerators.ScreenMode.Window:
                    {
                        Screen.fullScreen = false;
                        Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
                    }
                    break;
                case Enumerators.ScreenMode.BorderlessWindow:
                    {
                        Screen.fullScreen = false;
                        Screen.fullScreenMode = FullScreenMode.Windowed;
                    }
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(screenMode), (int)screenMode, typeof(Enumerators.ScreenMode));
            }

            _dataManager.CachedUserLocalData.AppScreenMode = CurrentScreenMode;
            _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
        }

        public void SetQuality(Enumerators.QualityLevel qualityLevel)
        {
            CurrentQualityLevel = qualityLevel;

            switch (qualityLevel)
            {
                case Enumerators.QualityLevel.Ultra:
                    break;
                case Enumerators.QualityLevel.High:
                    break;
                case Enumerators.QualityLevel.Medium:
                    break;
                case Enumerators.QualityLevel.Low:
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(qualityLevel), (int)qualityLevel, typeof(Enumerators.QualityLevel));
            }

            _dataManager.CachedUserLocalData.AppQualityLevel = CurrentQualityLevel;
            _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
        }

        private void FillResolutions()
        {
            Resolutions = new List<ResolutionInfo>();

            ResolutionInfo info;
            foreach (Resolution resolution in Screen.resolutions)
            {
                info = new ResolutionInfo()
                {
                    Name = $"{resolution.width} x {resolution.height}",
                    Resolution = new Vector2Int(resolution.width, resolution.height)
                };

                Resolutions.Add(info);
            }
        }

        private void HandleSpecificUserActions()
        {
            if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyUp(KeyCode.Return))
            {
                if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
                {
                    SetScreenMode(Enumerators.ScreenMode.FullScreen);
                }
                else if (Screen.fullScreenMode == FullScreenMode.MaximizedWindow)
                {
                    SetScreenMode(Enumerators.ScreenMode.Window);
                }
                else if(Screen.fullScreenMode == FullScreenMode.Windowed)
                {
                    SetScreenMode(Enumerators.ScreenMode.BorderlessWindow);
                }
            }
        }
    }

    public class ResolutionInfo
    {
        public string Name;
        public Vector2Int Resolution;
    }
}
