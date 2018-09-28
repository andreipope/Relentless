using Loom.ZombieBattleground.Common;
using UnityEngine;

namespace Loom.ZombieBattleground.Data
{
    public class UserLocalData
    {
        public Enumerators.Language AppLanguage;

        public bool Tutorial = true;

        public int LastSelectedDeckId;

        public bool AgreedTerms = false;

        public bool OpenedFirstPack;

        public int CurrentTutorialId = 0;

        public float MusicVolume = 1;

        public float SoundVolume = 1;

        public bool MusicMuted = false;

        public bool SoundMuted = false;

        public Enumerators.QualityLevel AppQualityLevel;

        public Enumerators.ScreenMode AppScreenMode;

        public Vector2Int AppResolution;

        public UserLocalData()
        {
            Reset();
        }

        public void Reset()
        {
            AppLanguage = Enumerators.Language.NONE;
            LastSelectedDeckId = -1;
            OpenedFirstPack = false;
            CurrentTutorialId = 0;
            MusicVolume = 1;
            SoundVolume = 1;
            MusicMuted = false;
            SoundMuted = false;
            AppQualityLevel = Enumerators.QualityLevel.Ultra;
            AppScreenMode = Enumerators.ScreenMode.FullScreen;

            Resolution resolution = Screen.resolutions[Screen.resolutions.Length - 1];
            AppResolution = new Vector2Int(resolution.width, resolution.height);
        }
    }
}
