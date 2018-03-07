using GrandDevs.CZB.Common;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.CZB
{
    public interface ISoundManager
    {
        int PlaySound(Enumerators.SoundType soundType, int priority = 128, float volume = -1f, Transform parent = null, bool isLoop = false, bool isPlaylist = false, bool dropOldBackgroundMusic = true, bool returnHashCode = false);
        AudioSource PlaySound(List<AudioClip> clips, Enumerators.SoundType soundType, int clipIndex = 0, int priority = 128, float volume = -1f, Transform parent = null, bool isLoop = false,
                             bool isPlaylist = false, bool dropOldBackgroundMusic = true, bool returnHashCode = false);

        void SetMusicVolume(float value);
        void SetSoundVolume(float value);

        void TurnOffSound();
        void StopPlaying(Enumerators.SoundType soundType, int id = 0);
        void StopPlaying(List<AudioClip> clips, int id = 0);
    }
}