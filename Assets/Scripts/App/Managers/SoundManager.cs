using System;
using System.Collections.Generic;
using System.Linq;
using LoomNetwork.CZB.Common;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class SoundManager : ISoundManager, IService
    {
        private List<SoundTypeList> _gameSounds;

        private List<SoundContainer> _soundContainers, _containersToRemove;

        private Transform _soundsRoot;

        private float _sfxVolume;

        private float _musicVolume;

        private Queue<QueuedSoundElement> _queuedSoundElements;

        private QueuedSoundElement _currentActiveQueuedSoundElement;

        public void Dispose()
        {
        }

        public void Init()
        {
            _sfxVolume = 1f;
            _musicVolume = 1f;

            _soundsRoot = new GameObject("SoundContainers").transform;
            _soundsRoot.gameObject.AddComponent<AudioListener>();
            Object.DontDestroyOnLoad(_soundsRoot);

            _soundContainers = new List<SoundContainer>();
            _containersToRemove = new List<SoundContainer>();
            _queuedSoundElements = new Queue<QueuedSoundElement>();

            _currentActiveQueuedSoundElement = null;

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

                            if (container.AudioSource.isPlaying)
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

            // if (_queuedSoundElements.Count > 0)
            // {
            // if (_soundContainers.FindAll(x => x.isInQueue && x.audioSource.isPlaying).Count == 0)
            // {
            // _currentActiveQueuedSoundElement = _queuedSoundElements.Dequeue();
            // _currentActiveQueuedSoundElement.DoAction();
            // }
            // }
        }

        public bool SfxMuted { get; set; }

        public bool MusicMuted { get; set; }

        public float GetSoundLength(Enumerators.SoundType soundType, string namePattern)
        {
            SoundTypeList soundTypeList = _gameSounds.Find(x => x.SoundType == soundType);

            AudioClip clip = soundTypeList.AudioTypeClips.Find(x => x.name.Contains(namePattern));

            return clip != null?clip.length:0f;
        }

        public float GetSoundLength(Enumerators.SoundType soundType)
        {
            SoundTypeList soundTypeList = _gameSounds.Find(x => x.SoundType == soundType);

            return soundTypeList.AudioTypeClips.Count > 0?soundTypeList.AudioTypeClips[0].length:0f;
        }

        public void PlaySound(Enumerators.SoundType soundType, string clipTitle, float volume = -1f, Enumerators.CardSoundType cardSoundType = Enumerators.CardSoundType.None)
        {
            foreach (SoundContainer item in _soundContainers)
            {
                if (cardSoundType.ToString().Equals(item.Tag))
                    return;
            }

            CreateSound(soundType, volume, null, false, false, 0, clipTitle, false, cardSoundType.ToString());
        }

        public void PlaySound(Enumerators.SoundType soundType, string clipTitle, float fadeOutAfterTime, float volume = -1f, Enumerators.CardSoundType cardSoundType = Enumerators.CardSoundType.None)
        {
            foreach (SoundContainer item in _soundContainers)
            {
                if (cardSoundType.ToString().Equals(item.Tag))
                    return;
            }

            SoundContainer thisSoundContainer = CreateSound(soundType, volume, null, false, false, 0, clipTitle, false, cardSoundType.ToString());
            FadeSound(thisSoundContainer, false, 0.005f, 0f, fadeOutAfterTime);
        }

        public void PlaySound(Enumerators.SoundType soundType, float volume = -1f, bool isLoop = false, bool dropOldBackgroundMusic = false, bool isInQueue = false)
        {
            PlaySound(soundType, 128, volume, null, isLoop, false, dropOldBackgroundMusic, isInQueue);
        }

        public void PlaySound(Enumerators.SoundType soundType, string clipTitle, float volume = -1f, bool isLoop = false, bool isInQueue = false)
        {
            CreateSound(soundType, volume, null, isLoop, false, 0, clipTitle, isInQueue);
        }

        public void PlaySound(Enumerators.SoundType soundType, int clipIndex, float volume = -1f, bool isLoop = false, bool isInQueue = false)
        {
            CreateSound(soundType, volume, null, isLoop, false, clipIndex, isInQueue: isInQueue);
        }

        public void CrossfaidSound(Enumerators.SoundType soundType, Transform parent = null, bool isLoop = false)
        {
            List<SoundContainer> oldContainers = _soundContainers.FindAll(x => x.SoundParameters.IsBackground);
            float volumeStep = oldContainers[0].AudioSource.volume / 15f;
            FadeSound(oldContainers, volumeStep: volumeStep);

            SoundContainer soundContainer = CreateSound(soundType, 0, parent, isLoop);
            soundContainer.AudioSource.time = oldContainers[0].AudioSource.time;
            FadeSound(soundContainer, true, volumeStep, oldContainers[0].AudioSource.volume);
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

            CreateSound(soundType, volume, parent, isLoop, isPlaylist, isInQueue: isInQueue);
        }

        public void SetMusicMuted(bool status)
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
        }

        public void SetSoundMuted(bool status)
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
        }

        public void SetMusicVolume(float value)
        {
            // GameClient.Get<IPlayerManager>().GetPlayerData.volumeMusic = value;
            // GameClient.Get<IDataManager>().SavePlayerData();
            _musicVolume = value;
            List<SoundContainer> containers = _soundContainers.FindAll(x => x.SoundParameters.IsBackground);

            if (containers != null)
            {
                foreach (SoundContainer container in containers)
                {
                    container.SoundParameters.Volume = _musicVolume;
                    container.AudioSource.volume = _musicVolume;
                }
            }
        }

        public void SetSoundVolume(float value)
        {
            // GameClient.Get<IPlayerManager>().GetPlayerData.volumeSound = value;
            // GameClient.Get<IDataManager>().SavePlayerData();
            _sfxVolume = value;
            List<SoundContainer> containers = _soundContainers.FindAll(x => !x.SoundParameters.IsBackground);

            if (containers != null)
            {
                foreach (SoundContainer container in containers)
                {
                    container.SoundParameters.Volume = _sfxVolume;
                    container.AudioSource.volume = _sfxVolume;
                }
            }
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
                container = _soundContainers.Find(x => (x.SoundType == soundType) && (x.GetHashCode() == id));
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
                container = _soundContainers.Find(x => (x.SoundParameters.AudioClips == clips) && (x.GetHashCode() == id));
            }

            if (container != null)
            {
                container.AudioSource.Stop();
                container.ForceClose = true;
            }
        }

        public void TurnOffSound()
        {
            foreach (SoundContainer container in _soundContainers)
            {
                container.AudioSource.Stop();
                container.ForceClose = true;
            }
        }

        private void StopBackroundMusic()
        {
            List<SoundContainer> oldContainers = _soundContainers.FindAll(x => x.SoundParameters.IsBackground);

            foreach (SoundContainer oldCotainer in oldContainers)
            {
                oldCotainer.AudioSource.Stop();
                oldCotainer.ForceClose = true;
            }
        }

        private void FadeSound(SoundContainer soundContainer, bool isIn = false, float volumeStep = 0, float targetVolume = 0, float targetTime = 0.1f)
        {
            List<SoundContainer> list = new List<SoundContainer>();
            list.Add(soundContainer);
            FadeSound(list, isIn, volumeStep, targetVolume, targetTime);
        }

        private void FadeSound(List<SoundContainer> soundcontainers, bool isIn = false, float volumeStep = 0.05f, float targetVolume = 1, float targetTime = 0.1f)
        {
            GameClient.Get<ITimerManager>().AddTimer(
                x =>
                {
                    bool stop = false;
                    foreach (SoundContainer container in soundcontainers)
                    {
                        if ((container == null) || (container.AudioSource == null))
                        {
                            break;
                        }

                        container.AudioSource.volume += volumeStep * (isIn?1:-1);

                        if ((container.AudioSource.volume == 0) && !isIn)
                        {
                            container.AudioSource.Stop();
                            container.ForceClose = true;
                            stop = true;
                        }
                        else if (isIn && (container.AudioSource.volume >= targetVolume))
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
            float volume = -1f,
            Transform parent = null,
            bool isLoop = false,
            bool isPlaylist = false,
            int clipIndex = 0,
            string clipTitle = "",
            bool isInQueue = false,
            string tag = "")
        {
            // if (isInQueue)
            // {
            // Debug.LogError("added sounds in queue " + soundType.ToString());
            // _queuedSoundElements.Enqueue(new QueuedSoundElement(() => { DoSoundContainer(soundType, volume, parent, isLoop, isPlaylist, clipIndex, clipTitle, isInQueue); }));

            // Debug.LogError(_queuedSoundElements.Count + " _queuedSoundElements count");
            // }
            // else 
            return DoSoundContainer(soundType, volume, parent, isLoop, isPlaylist, clipIndex, clipTitle, isInQueue, tag);
        }

        private SoundContainer DoSoundContainer(
            Enumerators.SoundType soundType,
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
                case Enumerators.SoundType.Background:
                case Enumerators.SoundType.Battleground:
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

            soundParam.Priority = 128;
            if (volume < 0)
            {
                soundParam.Volume = _sfxVolume;
            }
            else
            {
                soundParam.Volume = volume;
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

            container.IsInQueue = isInQueue;
            container.Tag = tag;
            container.Init(_soundsRoot, soundType, soundParam, isPlaylist, clipIndex);

            if (parent != null)
            {
                container.Container.transform.SetParent(parent);
            }

            _soundContainers.Add(container);
            return container;
        }

        private void InitializeSounds()
        {
            _gameSounds = new List<SoundTypeList>();

            SoundTypeList soundsList = null;
            int countOfTypes = Enum.GetNames(typeof(Enumerators.SoundType)).Length;

            for (int i = 0; i < countOfTypes; i++)
            {
                soundsList = new SoundTypeList();
                soundsList.SoundType = (Enumerators.SoundType)i;
                soundsList.AudioTypeClips = LoadAudioClipsByType(soundsList.SoundType);

                _gameSounds.Add(soundsList);
            }
        }

        private List<AudioClip> LoadAudioClipsByType(Enumerators.SoundType soundType)
        {
            List<AudioClip> list = null;

            string pathToSoundsLibrary = "Sounds/";

            switch (soundType)
            {
                case Enumerators.SoundType.Tutorial:
                case Enumerators.SoundType.Cards:
                case Enumerators.SoundType.OverlordAbilities:
                case Enumerators.SoundType.Spells:
                    pathToSoundsLibrary = "Sounds/" + soundType.ToString().Replace("_", string.Empty);
                    list = Resources.LoadAll<AudioClip>(pathToSoundsLibrary).ToList();
                    break;
                default:
                    list = Resources.LoadAll<AudioClip>(pathToSoundsLibrary).Where(x => x.name == soundType.ToString()).ToList();
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

        public bool IsInQueue;

        public string Tag;

        public void Init(Transform soundsContainerRoot, Enumerators.SoundType type, SoundParam soundParam, bool playlistEnabled, int soundIndex = 0)
        {
            ForceClose = false;
            CurrentSoundIndex = soundIndex;
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
                AudioSource.Stop();
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

    public class QueuedSoundElement
    {
        public Action Action;

        public SoundContainer Container;

        public QueuedSoundElement(Action action)
        {
            Action = action;
        }

        public void DoAction()
        {
            Action?.Invoke();
        }
    }
}
