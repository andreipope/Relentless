using Loom.ZombieBattleground.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground.Data
{
    public class UserLocalData
    {
        public Enumerators.Language AppLanguage;

        public bool Tutorial = true;
        public DeckId LastSelectedDeckId;

        public bool AgreedTerms = false;

        public bool OpenedFirstPack;

        public int CurrentTutorialId = 0;

        public float MusicVolume = 0.3f;

        public float SoundVolume = 0.5f;

        public bool MusicMuted = false;

        public bool SoundMuted = false;

        public Enumerators.ScreenMode AppScreenMode;

#if !UNITY_ANDROID && !UNITY_IOS
        public Vector2Int AppResolution;
#endif

        public List<int> TutorialTooltipsPassed;

        public Deck TutorialSavedDeck;

        public UserLocalData()
        {
            Reset();
        }

        public void Reset()
        {
            AppLanguage = Enumerators.Language.NONE;
            LastSelectedDeckId = new DeckId(-1);
            OpenedFirstPack = false;
            CurrentTutorialId = 0;
            MusicVolume = 0.3f;
            SoundVolume = 0.5f;
            MusicMuted = false;
            SoundMuted = false;
#if !UNITY_ANDROID && !UNITY_IOS
            AppScreenMode = Enumerators.ScreenMode.FullScreen;

            if (Screen.resolutions.Length > 0)
            {
                Resolution resolution = Screen.resolutions[Screen.resolutions.Length - 1];
                AppResolution = new Vector2Int(resolution.width, resolution.height);
            }
            else
            {
                AppResolution = new Vector2Int(1280, 720);
            }
#endif
            TutorialTooltipsPassed = new List<int>();
        }
    }
}
