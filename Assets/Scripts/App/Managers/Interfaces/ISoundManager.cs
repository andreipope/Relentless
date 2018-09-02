using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public interface ISoundManager
    {
        bool SfxMuted { get; set; }

        bool MusicMuted { get; set; }

        float GetSoundLength(Enumerators.SoundType soundType, string namePattern);

        float GetSoundLength(Enumerators.SoundType soundType);

        void PlaySound(Enumerators.SoundType soundType, string clipTitle, float volume = -1f, Enumerators.CardSoundType cardSoundType = Enumerators.CardSoundType.NONE);

        void PlaySound(Enumerators.SoundType soundType, string clipTitle, float fadeOutAfterTime, float volume = -1f, Enumerators.CardSoundType cardSoundType = Enumerators.CardSoundType.NONE);

        void PlaySound(Enumerators.SoundType soundType, int priority = 128, float volume = -1f, Transform parent = null, bool isLoop = false, bool isPlaylist = false, bool dropOldBackgroundMusic = true, bool isInQueue = false);

        void PlaySound(Enumerators.SoundType soundType, float volume = -1f, bool isLoop = false, bool dropOldBackgroundMusic = false, bool isInQueue = false);

        void PlaySound(Enumerators.SoundType soundType, string clipTitle, float volume = -1f, bool isLoop = false, bool isInQueue = false);

        void PlaySound(Enumerators.SoundType soundType, int clipIndex, float volume = -1f, bool isLoop = false, bool isInQueue = false);

        void CrossfaidSound(Enumerators.SoundType soundType, Transform parent = null, bool isLoop = false);

        void SetMusicVolume(float value);

        void SetSoundVolume(float value);

        void TurnOffSound();

        void StopPlaying(Enumerators.SoundType soundType, int id = 0);

        void StopPlaying(List<AudioClip> clips, int id = 0);

        void SetSoundMuted(bool status);

        void SetMusicMuted(bool status);
    }
}
