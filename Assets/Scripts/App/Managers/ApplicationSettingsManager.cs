using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public static event Action OnResolutionChanged;

        public const float WaitForResolutionChangeFinishAnimating = 1.5f;

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

        public async void Update()
        {
#if !UNITY_ANDROID && !UNITY_IOS
            await HandleSpecificUserActions();
#endif
        }

        public async Task ApplySettings()
        {
#if !UNITY_ANDROID && !UNITY_IOS
            CurrentScreenMode = _dataManager.CachedUserLocalData.AppScreenMode;
            CurrentResolution = Resolutions.Find(x => x.Resolution.x == _dataManager.CachedUserLocalData.AppResolution.x &&
                x.Resolution.y == _dataManager.CachedUserLocalData.AppResolution.y);

            await SetScreenMode(CurrentScreenMode);
#endif
        }

        public async Task SetDefaults()
        {
#if !UNITY_ANDROID && !UNITY_IOS
            if (Resolutions.Count > 0)
            {
                await SetResolution(Resolutions[Resolutions.Count - 1]);
            }

            await SetScreenMode(Enumerators.ScreenMode.Window);
#endif
        }

#if !UNITY_ANDROID && !UNITY_IOS
        public async Task SetResolution(ResolutionInfo info)
        {
            CurrentResolution = info;

            Screen.SetResolution(info.Resolution.x, info.Resolution.y, CurrentScreenMode == Enumerators.ScreenMode.FullScreen);

            _dataManager.CachedUserLocalData.AppResolution = CurrentResolution.Resolution;
            await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);

            //Wait until game screen has finish animating for it's resolution changes
            await Task.Delay(TimeSpan.FromSeconds(WaitForResolutionChangeFinishAnimating));
            OnResolutionChanged?.Invoke();
        }
#endif

        public async Task SetScreenMode(Enumerators.ScreenMode screenMode)
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
            await MakeResolutionHighestInFullScreenMode();
#endif
        }
  
#if !UNITY_ANDROID && !UNITY_IOS      
        private async Task MakeResolutionHighestInFullScreenMode()
        {
            if (Resolutions.Count == 0)
                return;

            if(CurrentScreenMode == Enumerators.ScreenMode.FullScreen)
            {
                await SetResolution(Resolutions[Resolutions.Count - 1]);
            }
            else
            {
                await SetResolution(CurrentResolution);
            }
        }

        public void FillResolutions()
        {
            if(Resolutions != null)
            {
                Resolutions.Clear();
            }
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

        public ResolutionInfo AddResolution(Resolution resolution)
        {
            ResolutionInfo resolutionInfo = Resolutions.Find(info => info.Resolution.x == resolution.width && info.Resolution.y == resolution.height);
            if (resolutionInfo != null)
                return resolutionInfo;

            resolutionInfo = new ResolutionInfo
            {
                Name = $"{resolution.width} x {resolution.height}",
                Resolution = new Vector2Int(resolution.width, resolution.height)
            };

            Resolutions.Add(resolutionInfo);

            return resolutionInfo;
        }

        private async Task HandleSpecificUserActions()
        {
            if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyUp(KeyCode.Return))
            {
                switch (Screen.fullScreenMode)
                {
                    case FullScreenMode.FullScreenWindow:
                        await SetScreenMode(Enumerators.ScreenMode.FullScreen);
                        break;
                    case FullScreenMode.MaximizedWindow:
                        await SetScreenMode(Enumerators.ScreenMode.Window);
                        break;
                    case FullScreenMode.Windowed:
                        await SetScreenMode(Enumerators.ScreenMode.BorderlessWindow);
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
