using System;
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

        public Enumerators.ScreenMode CurrentScreenMode { get; private set; }

        public ResolutionInfo CurrentResolution { get; private set; }

        public void Dispose()
        {
        }

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();

#if !UNITY_ANDROID && !UNITY_IOS
            FillResolutions();
#endif
        }

        public void Update()
        {
#if !UNITY_ANDROID && !UNITY_IOS
            HandleSpecificUserActions();
#endif
        }

        public void ApplySettings()
        {
#if !UNITY_ANDROID && !UNITY_IOS
            CurrentScreenMode = _dataManager.CachedUserLocalData.AppScreenMode;
            CurrentResolution = Resolutions.Find(x => x.Resolution.x == _dataManager.CachedUserLocalData.AppResolution.x &&
                x.Resolution.y == _dataManager.CachedUserLocalData.AppResolution.y);
#endif
        }

        public void SetDefaults()
        {
#if !UNITY_ANDROID && !UNITY_IOS
            SetResolution(Resolutions[Resolutions.Count - 1]);
            SetScreenMode(Enumerators.ScreenMode.Window);
#endif
        }

#if !UNITY_ANDROID && !UNITY_IOS
        public void SetResolution(ResolutionInfo info)
        {
            CurrentResolution = info;

            Screen.SetResolution(info.Resolution.x, info.Resolution.y, CurrentScreenMode == Enumerators.ScreenMode.FullScreen ? true : false);

            SetScreenMode(CurrentScreenMode);

            _dataManager.CachedUserLocalData.AppResolution = CurrentResolution.Resolution;
            _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
        }
#endif

        public async void SetScreenMode(Enumerators.ScreenMode screenMode)
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
                        Screen.fullScreenMode = FullScreenMode.Windowed;
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
            await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);

#if !UNITY_ANDROID && !UNITY_IOS
            Screen.SetResolution(CurrentResolution.Resolution.x, CurrentResolution.Resolution.y, CurrentScreenMode == Enumerators.ScreenMode.FullScreen);          
#endif
        }

#if !UNITY_ANDROID && !UNITY_IOS
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
                switch (Screen.fullScreenMode)
                {
                    case FullScreenMode.FullScreenWindow:
                        SetScreenMode(Enumerators.ScreenMode.FullScreen);
                        break;
                    case FullScreenMode.MaximizedWindow:
                        SetScreenMode(Enumerators.ScreenMode.Window);
                        break;
                    case FullScreenMode.Windowed:
                        SetScreenMode(Enumerators.ScreenMode.BorderlessWindow);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(Screen.fullScreenMode), Screen.fullScreenMode, null);
                }
            }
        }
#endif
    }

    public class ResolutionInfo
    {
        public string Name;
        public Vector2Int Resolution;
    }
}
