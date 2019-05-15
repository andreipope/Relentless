using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class SoundManager : ISoundManager, IService
    {
        private List<SoundTypeList> _gameSounds;

        private List<SoundContainer> _soundContainers, _containersToRemove;

        protected ILoadObjectsManager LoadObjectsManager;
        protected IDataManager DataManager;

        private Transform _soundsRoot;

        private float _soundVolume;

        private float _musicVolume;

        private SoundsData _tutorialSoundsFilename;
        private SoundsData _cardsSoundsFilename;
        private SoundsData _overlordAbilitiesSoundsFilename;
        private SoundsData _spellsSoundsFilename;
        private SoundsData _uniqueArrivalsSoundsFilename;
        private SoundsData _zombieDeathAnimationsSoundsFilename;

        public float SoundVolume => _soundVolume;
        public float MusicVolume => _musicVolume;

        private int _lastSoundContainerIdentificator;

        public void Dispose()
        {
        }

        public void Init()
        {
            LoadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            DataManager = GameClient.Get<IDataManager>();

            _tutorialSoundsFilename = LoadObjectsManager.GetObjectByPath<SoundsData>("SoundData/TutorialSounds");
            _cardsSoundsFilename = LoadObjectsManager.GetObjectByPath<SoundsData>("SoundData/CardsSounds");
            _overlordAbilitiesSoundsFilename = LoadObjectsManager.GetObjectByPath<SoundsData>("SoundData/OverlordAbilitiesSounds");
            _spellsSoundsFilename = LoadObjectsManager.GetObjectByPath<SoundsData>("SoundData/SpellsSounds");
            _uniqueArrivalsSoundsFilename = LoadObjectsManager.GetObjectByPath<SoundsData>("SoundData/UniqueArrivalsSounds");
            _zombieDeathAnimationsSoundsFilename = LoadObjectsManager.GetObjectByPath<SoundsData>("SoundData/ZombieDeathAnimationsSounds");

            _soundsRoot = new GameObject("SoundContainers").transform;
            _soundsRoot.gameObject.AddComponent<AudioListener>();

            _soundContainers = new List<SoundContainer>();
            _containersToRemove = new List<SoundContainer>();

            InitializeSounds();
        }

        public void Update()
        {
            if (_soundContainers.Count > 0)
            {
                lock (_soundContainers)
                {
                    foreach (SoundContainer container in _soundContainers)
                    {
                        if (!container.ForceClose)
                        {
                            if (container.Container == null)
                            {
                                container.ForceClose = true;
                                continue;
                            }

                            if (container.AudioSource.isPlaying || AudioListener.pause)
                            {
                                continue;
                            }

                            if (container.IsPlaylist)
                            {
                                if (container.PlayNextSound())
                                {
                                    continue;
                                }
                            }
                        }

                        container.Dispose();
                        _containersToRemove.Add(container);
                    }
                }

                if (_containersToRemove.Count > 0)
                {
                    foreach (SoundContainer container in _containersToRemove)
                    {
                        _soundContainers.Remove(container);
                    }

                    _containersToRemove.Clear();
                }
            }
        }

        public void ApplySoundData()
        {
            _soundVolume = DataManager.CachedUserLocalData.SoundVolume;
            _musicVolume = DataManager.CachedUserLocalData.MusicVolume;
            MusicMuted = DataManager.CachedUserLocalData.MusicMuted;
            SfxMuted = DataManager.CachedUserLocalData.SoundMuted;

            SetSoundMuted(SfxMuted, false);
            SetMusicMuted(MusicMuted, false);
            SetMusicVolume(_musicVolume, false);
            SetSoundVolume(_soundVolume, false);
        }

        public bool SfxMuted { get; set; }

        public bool MusicMuted { get; set; }

        public float GetSoundLength(Enumerators.SoundType soundType, string namePattern)
        {
            SoundTypeList soundTypeList = _gameSounds.Find(x => x.SoundType == soundType);

            AudioClip clip = soundTypeList.AudioTypeClips.Find(x => x.name.Contains(namePattern));

            return clip != null ? clip.length : 0f;
        }

        public float GetSoundLength(Enumerators.SoundType soundType)
        {
            SoundTypeList soundTypeList = _gameSounds.Find(x => x.SoundType == soundType);

            return soundTypeList.AudioTypeClips.Count > 0 ? soundTypeList.AudioTypeClips[0].length : 0f;
        }

        public void SetSoundPaused(int identificator, bool pause)
        {
            SoundContainer container = _soundContainers.Find(x => x.UniqueIdentificator == identificator);

            if (container != null)
            {
                if(pause)
                {
                    container.AudioSource.Pause();
                }
                else
                {
                    container.AudioSource.UnPause();
                }
            }
        }

        public void PlaySound(
            Enumerators.SoundType soundType, string clipTitle, float volume = -1f,
            Enumerators.CardSoundType cardSoundType = Enumerators.CardSoundType.NONE)
        {
            foreach (SoundContainer item in _soundContainers)
            {
                if (cardSoundType.ToString() == item.Tag)
                    return;
            }

            CreateSound(soundType, 128, volume, null, false, false, 0, clipTitle, false, cardSoundType.ToString());
        }

        public void PlaySound(
            Enumerators.SoundType soundType, string clipTitle, float fadeOutAfterTime, float volume = -1f,
            Enumerators.CardSoundType cardSoundType = Enumerators.CardSoundType.NONE)
        {
            foreach (SoundContainer item in _soundContainers)
            {
                if (cardSoundType.ToString() == item.Tag)
                    return;
            }

            SoundContainer thisSoundContainer = CreateSound(soundType, 128, volume, null, false, false, 0, clipTitle, false,
                cardSoundType.ToString());
            FadeSound(thisSoundContainer, false, 0.005f, 0f, fadeOutAfterTime);
        }

        public void PlaySound(
            Enumerators.SoundType soundType, float volume = -1f, bool isLoop = false,
            bool dropOldBackgroundMusic = false, bool isInQueue = false)
        {
            PlaySound(soundType, 128, volume, null, isLoop, false, dropOldBackgroundMusic, isInQueue);
        }

        public int PlaySound(
            Enumerators.SoundType soundType, string clipTitle, float volume = -1f, bool isLoop = false,
            bool isInQueue = false)
        {
            SoundContainer soundContainer = CreateSound(soundType, 128, volume, null, isLoop, false, 0, clipTitle, isInQueue);
            if(soundContainer != null)
            {
                return soundContainer.UniqueIdentificator;
            }
            return -1;
        }

        public void PlaySound(
            Enumerators.SoundType soundType, int priority = 128, string clipTitle = "", float volume = -1f, bool isLoop = false,
            bool isInQueue = false)
        {
            CreateSound(soundType, priority, volume, null, isLoop, false, 0, clipTitle, isInQueue);
        }

        public void PlaySound(
            Enumerators.SoundType soundType, int clipIndex, float volume = -1f, bool isLoop = false,
            bool isInQueue = false)
        {
            CreateSound(soundType, 128, volume, null, isLoop, false, clipIndex, isInQueue: isInQueue);
        }

        public void CrossfaidSound(Enumerators.SoundType soundType, Transform parent = null, bool isLoop = false)
        {
            List<SoundContainer> oldContainers = _soundContainers.FindAll(x => x.SoundParameters.IsBackground);

            if (oldContainers.Count > 0)
            {
                float volumeStep = oldContainers[0].AudioSource.volume / 15f;
                FadeSound(oldContainers, volumeStep: volumeStep);

                SoundContainer soundContainer = CreateSound(soundType, 128, 0, parent, isLoop);
                soundContainer.AudioSource.time = Mathf.Min(oldContainers[0].AudioSource.time, soundContainer.AudioSource.clip.length - 0.01f);
                FadeSound(soundContainer, true, volumeStep, oldContainers[0].AudioSource.volume);
            }
            else
            {
                SoundContainer soundContainer = CreateSound(soundType, 128, 0, parent, isLoop);
                FadeSound(soundContainer, true, Constants.BackgroundSoundVolume / 15f, Constants.BackgroundSoundVolume);
            }
        }

        public void PlaySound(
            Enumerators.SoundType soundType,
            int priority = 128,
            float volume = -1f,
            Transform parent = null,
            bool isLoop = false,
            bool isPlaylist = false,
            bool dropOldBackgroundMusic = false,
            bool isInQueue = false)
        {
            if (dropOldBackgroundMusic)
            {
                StopBackroundMusic();
            }
            CreateSound(soundType, priority, volume, parent, isLoop, isPlaylist, isInQueue: isInQueue);
        }

        public void SetMusicMuted(bool status, bool withSaving = true)
        {
            List<SoundContainer> containers = _soundContainers.FindAll(x => x.SoundParameters.IsBackground);

            if (containers != null)
            {
                foreach (SoundContainer container in containers)
                {
                    container.AudioSource.mute = status;
                }
            }

            MusicMuted = status;

            if (withSaving)
            {
                SaveVolumeSettings();
            }
        }

        public void SetSoundMuted(bool status, bool withSaving = true)
        {
            List<SoundContainer> containers = _soundContainers.FindAll(x => !x.SoundParameters.IsBackground);

            if (containers != null)
            {
                foreach (SoundContainer container in containers)
                {
                    container.AudioSource.mute = status;
                }
            }

            SfxMuted = status;

            if (withSaving)
            {
                SaveVolumeSettings();
            }
        }

        public void SetMusicVolume(float value, bool withSaving = true)
        {
            _musicVolume = value;
            List<SoundContainer> containers = _soundContainers.FindAll(x => x.SoundParameters.IsBackground);

            if (containers != null)
            {
                foreach (SoundContainer container in containers)
                {
                    container.SoundParameters.Volume = _musicVolume * Constants.BackgroundSoundVolume;
                    container.AudioSource.volume = _musicVolume * Constants.BackgroundSoundVolume;
                }
            }

            if (withSaving)
            {
                SaveVolumeSettings();
            }
        }

        public void SetSoundVolume(float value, bool withSaving = true)
        {
            _soundVolume = value;
            List<SoundContainer> containers = _soundContainers.FindAll(x => !x.SoundParameters.IsBackground);

            if (containers != null)
            {
                foreach (SoundContainer container in containers)
                {
                    container.SoundParameters.Volume = _soundVolume;
                    container.AudioSource.volume = _soundVolume;
                }
            }

            if (withSaving)
            {
                SaveVolumeSettings();
            }
        }

        public void SaveVolumeSettings()
        {
            DataManager.CachedUserLocalData.MusicVolume = _musicVolume;
            DataManager.CachedUserLocalData.SoundVolume = _soundVolume;
            DataManager.CachedUserLocalData.SoundMuted = SfxMuted;
            DataManager.CachedUserLocalData.MusicMuted = MusicMuted;
            DataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
        }

        public void StopPlaying(Enumerators.SoundType soundType, int id = 0)
        {
            SoundContainer container;
            if (id == 0)
            {
                container = _soundContainers.Find(x => x.SoundType == soundType);
            }
            else
            {
                container = _soundContainers.Find(x => x.SoundType == soundType && x.GetHashCode() == id);
            }

            if (container != null)
            {
                container.AudioSource.Stop();
                container.ForceClose = true;
            }
        }

        public void StopPlaying(List<AudioClip> clips, int id = 0)
        {
            SoundContainer container;
            if (id == 0)
            {
                container = _soundContainers.Find(x => x.SoundParameters.AudioClips == clips);
            }
            else
            {
                container = _soundContainers.Find(x => x.SoundParameters.AudioClips == clips && x.GetHashCode() == id);
            }

            if (container != null)
            {
                container.AudioSource.Stop();
                container.ForceClose = true;
            }
        }

        public void TurnOffSound()
        {
            StopMusicInContatiners(_soundContainers);
        }

        private void StopBackroundMusic()
        {
            StopMusicInContatiners(_soundContainers.FindAll(x => x.SoundParameters.IsBackground));
        }

        private void StopMusicInContatiners(List<SoundContainer> containers)
        {
            if (containers == null)
                return;

            for (int i = 0; i < containers.Count; i++)
            {
                if (containers[i] == null)
                {
                    containers.RemoveAt(i--);
                    continue;
                }

                containers[i].AudioSource?.Stop();
                containers[i].ForceClose = true;
            }
        }

        private void FadeSound(
            SoundContainer soundContainer, bool isIn = false, float volumeStep = 0, float targetVolume = 0,
            float targetTime = 0.1f)
        {
            List<SoundContainer> list = new List<SoundContainer>();
            list.Add(soundContainer);
            FadeSound(list, isIn, volumeStep, targetVolume, targetTime);
        }

        private void FadeSound(
            List<SoundContainer> soundcontainers, bool isIn = false, float volumeStep = 0.05f, float targetVolume = 1,
            float targetTime = 0.1f)
        {
            GameClient.Get<ITimerManager>().AddTimer(
                x =>
                {
                    bool stop = false;
                    foreach (SoundContainer container in soundcontainers)
                    {
                        if (container == null || container.AudioSource == null)
                        {
                            break;
                        }

                        container.AudioSource.volume += volumeStep * (isIn ? 1 : -1);

                        if (container.AudioSource.volume == 0 && !isIn)
                        {
                            container.AudioSource.Stop();
                            container.ForceClose = true;
                            stop = true;
                        }
                        else if (isIn && container.AudioSource.volume >= targetVolume)
                        {
                            container.AudioSource.volume = targetVolume;
                            stop = true;
                        }
                    }

                    if (!stop)
                    {
                        FadeSound(soundcontainers, isIn, volumeStep, targetVolume);
                    }
                },
                null,
                targetTime);
        }

        private SoundContainer CreateSound(
            Enumerators.SoundType soundType,
            int priority = 128,
            float volume = -1f,
            Transform parent = null,
            bool isLoop = false,
            bool isPlaylist = false,
            int clipIndex = 0,
            string clipTitle = "",
            bool isInQueue = false,
            string tag = "")
        {
            return DoSoundContainer(soundType, priority, volume, parent, isLoop, isPlaylist, clipIndex, clipTitle, isInQueue,
                tag);
        }

        private SoundContainer DoSoundContainer(
            Enumerators.SoundType soundType,
            int priority = 128,
            float volume = -1f,
            Transform parent = null,
            bool isLoop = false,
            bool isPlaylist = false,
            int clipIndex = 0,
            string clipTitle = "",
            bool isInQueue = false,
            string tag = "")
        {
            SoundParam soundParam = new SoundParam();
            SoundContainer container = new SoundContainer();
            SoundTypeList soundTypeList = _gameSounds.Find(x => x.SoundType == soundType);

            switch (soundType)
            {
                case Enumerators.SoundType.BACKGROUND:
                case Enumerators.SoundType.BATTLEGROUND:
                    soundParam.IsBackground = true;
                    break;
                default:
                    soundParam.IsBackground = false;
                    break;
            }

            soundParam.AudioClips = soundTypeList.AudioTypeClips;
            if (!string.IsNullOrEmpty(clipTitle))
            {
                soundParam.AudioClips = soundParam.AudioClips.Where(clip => clip.name.Contains(clipTitle)).ToList();
            }

            // small hack to ignore missing audio clips
            if (soundParam.AudioClips.Count == 0)
            {
                return null;
            }

            soundParam.IsLoop = isLoop;
            soundParam.IsMute = false;
            soundParam.PlayOnAwake = false;

            soundParam.Priority = priority;

            if (soundParam.IsBackground)
            {
                if (volume < 0)
                {
                    soundParam.Volume = MusicVolume;
                }
                else
                {
                    soundParam.Volume = (1 * MusicVolume) * volume;
                }
            }
            else
            {
                if (volume < 0)
                {
                    soundParam.Volume = SoundVolume;
                }
                else
                {
                    soundParam.Volume = (1 * SoundVolume) * volume;
                }
            }

            if (SfxMuted && !soundParam.IsBackground)
            {
                soundParam.IsMute = true;
            }
            else if (MusicMuted && soundParam.IsBackground)
            {
                soundParam.IsMute = true;
            }

            soundParam.StartPosition = 0f;

            container.Tag = tag;
            container.Init(_soundsRoot, soundType, soundParam, isPlaylist, _lastSoundContainerIdentificator, clipIndex);

            if (parent != null)
            {
                container.Container.transform.SetParent(parent);
            }

            _lastSoundContainerIdentificator++;

            _soundContainers.Add(container);
            return container;
        }

        private void InitializeSounds()
        {
            _gameSounds = new List<SoundTypeList>();

            int countOfTypes = Enum.GetNames(typeof(Enumerators.SoundType)).Length;

            for (int i = 0; i < countOfTypes; i++)
            {
                SoundTypeList soundsList = new SoundTypeList();
                soundsList.SoundType = (Enumerators.SoundType) i;
                soundsList.AudioTypeClips = LoadAudioClipsByType(soundsList.SoundType);

                _gameSounds.Add(soundsList);
            }
        }

        private List<AudioClip> LoadAudioClipsByType(Enumerators.SoundType soundType)
        {
            List<AudioClip> list;

            string pathToSoundsLibrary = "Sounds/";

            switch (soundType)
            {
                case Enumerators.SoundType.TUTORIAL:
                    list = LoadObjectsManager.GetObjectsByPath<AudioClip>(_tutorialSoundsFilename.soundList).ToList();
                    break;
                case Enumerators.SoundType.CARDS:
                    list = LoadObjectsManager.GetObjectsByPath<AudioClip>(_cardsSoundsFilename.soundList).ToList();
                    break;
                case Enumerators.SoundType.OVERLORD_ABILITIES:
                    list = LoadObjectsManager.GetObjectsByPath<AudioClip>(_overlordAbilitiesSoundsFilename.soundList).ToList();
                    break;
                case Enumerators.SoundType.SPELLS:
                    list = LoadObjectsManager.GetObjectsByPath<AudioClip>(_spellsSoundsFilename.soundList).ToList();
                    break;
                case Enumerators.SoundType.UNIQUE_ARRIVALS:
                    list = LoadObjectsManager.GetObjectsByPath<AudioClip>(_uniqueArrivalsSoundsFilename.soundList).ToList();
                    break;
                case Enumerators.SoundType.ZOMBIE_DEATH_ANIMATIONS:
                    list = LoadObjectsManager.GetObjectsByPath<AudioClip>(_zombieDeathAnimationsSoundsFilename.soundList).ToList();
                    break;
                default:
                    list = LoadObjectsManager.GetObjectsByPath<AudioClip>(new string[] { pathToSoundsLibrary + soundType.ToString() }).ToList();
                    break;
            }
            return list;
        }
    }

    public class SoundTypeList
    {
        public Enumerators.SoundType SoundType;

        public List<AudioClip> AudioTypeClips;

        public SoundTypeList()
        {
            AudioTypeClips = new List<AudioClip>();
        }
    }

    public class SoundContainer
    {
        public Enumerators.SoundType SoundType;

        public AudioSource AudioSource;

        public GameObject Container;

        public SoundParam SoundParameters;

        public bool IsPlaylist;

        public bool ForceClose;

        public int CurrentSoundIndex;

        public int UniqueIdentificator;

        public string Tag;

        public void Init(
            Transform soundsContainerRoot, Enumerators.SoundType type, SoundParam soundParam, bool playlistEnabled, int identificator,
            int soundIndex = 0)
        {
            ForceClose = false;
            CurrentSoundIndex = soundIndex;
            UniqueIdentificator = identificator;
            SoundParameters = soundParam;
            IsPlaylist = playlistEnabled;
            SoundType = type;
            Container = new GameObject("AudioClip " + SoundType);
            Container.transform.SetParent(soundsContainerRoot, false);
            AudioSource = Container.AddComponent<AudioSource>();

            AudioSource.clip = soundParam.AudioClips[CurrentSoundIndex];
            AudioSource.volume = soundParam.Volume;
            AudioSource.loop = soundParam.IsLoop;
            AudioSource.time = soundParam.StartPosition;
            AudioSource.mute = soundParam.IsMute;
            AudioSource.playOnAwake = soundParam.PlayOnAwake;
            AudioSource.priority = soundParam.Priority;
            AudioSource.spatialBlend = 1f;

            AudioSource.Play();
        }

        public bool PlayNextSound()
        {
            CurrentSoundIndex++;

            if (CurrentSoundIndex < SoundParameters.AudioClips.Count)
            {
                AudioSource.Stop();
                AudioSource.clip = SoundParameters.AudioClips[CurrentSoundIndex];
                AudioSource.Play();
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            if (Container != null)
            {
                AudioSource?.Stop();
                Object.Destroy(Container);
            }
        }
    }

    public class SoundParam
    {
        public float Volume;

        public float StartPosition;

        public bool IsLoop;

        public bool IsMute;

        public bool PlayOnAwake;

        public bool IsBackground;

        public int Priority;

        public List<AudioClip> AudioClips;
    }
}
