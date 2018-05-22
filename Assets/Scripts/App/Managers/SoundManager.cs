using UnityEngine;
using System;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using System.Linq;

namespace GrandDevs.CZB
{
    public class SoundManager : ISoundManager, IService
    {

        private List<SoundTypeList> _gameSounds;
        private List<SoundContainer> _soundContainers,
                                     _containersToRemove;
        private Transform _soundsRoot;

        private float _sfxVolume;

        public void Dispose()
        {

        }

        public void Init()
        {
            _sfxVolume = 1;
            _soundsRoot = new GameObject("SoundContainers").transform;
            _soundsRoot.gameObject.AddComponent<AudioListener>();
            MonoBehaviour.DontDestroyOnLoad(_soundsRoot);

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
                    foreach (var container in _soundContainers)
                    {
                        if (!container.forceClose)
                        {
                            if (container.container == null)
                            {
                                container.forceClose = true;
                                continue;
                            }
                            if (container.audioSource.isPlaying) continue;

                            if (container.isPlaylist)
                            {
                                if (container.PlayNextSound())
                                    continue;
                            }
                        }

                        container.Dispose();
                        _containersToRemove.Add(container);
                    }
                }

                if (_containersToRemove.Count > 0)
                {
                    foreach (var container in _containersToRemove)
                        _soundContainers.Remove(container);
                    _containersToRemove.Clear();
                }
            }
        }

        public void PlaySound(Enumerators.SoundType soundType, float volume = -1f, bool isLoop = false, bool dropOldBackgroundMusic = false)
        {
            PlaySound(soundType, 128, volume, null, isLoop, false, dropOldBackgroundMusic);
        }
        public void PlaySound(Enumerators.SoundType soundType, string clipTitle, float volume = -1f, bool isLoop = false )
        {
            CreateSound(soundType, volume, null, isLoop, false, 0, clipTitle);
        }

        public void PlaySound(Enumerators.SoundType soundType, int clipIndex, float volume = -1f, bool isLoop = false )
        {
            CreateSound(soundType, volume, null, isLoop, false, clipIndex);
        }

        public void PlaySound(Enumerators.SoundType soundType, int priority = 128, float volume = -1f, Transform parent = null, bool isLoop = false,
                             bool isPlaylist = false, bool dropOldBackgroundMusic = false)
        {
            if (dropOldBackgroundMusic)
                StopBackroundMusic();

            CreateSound(soundType, volume, parent, isLoop, isPlaylist);
        }

        private void StopBackroundMusic()
        {
            var oldContainers = _soundContainers.FindAll(x => x.soundParameters.isBackground);

            foreach (var oldCotainer in oldContainers)
            {
                oldCotainer.audioSource.Stop();
                oldCotainer.forceClose = true;
            }
        }

        private void CreateSound(Enumerators.SoundType soundType, float volume = -1f, Transform parent = null, bool isLoop = false,
                             bool isPlaylist = false, int clipIndex = 0, string clipTitle = "")
        {
            SoundParam soundParam = new SoundParam();
            SoundContainer container = new SoundContainer();
            SoundTypeList soundTypeList = _gameSounds.Find(x => x.soundType == soundType);

            soundParam.isBackground = soundType.ToString().Contains("BACKGROUND") ? true : false;

            soundParam.audioClips = soundTypeList.audioTypeClips;
            if (!string.IsNullOrEmpty(clipTitle))
                soundParam.audioClips = soundParam.audioClips.Where((clip) => clip.name.Contains(clipTitle)).ToList();

            // small hack to ignore missing audio clips
            if (soundParam.audioClips.Count == 0)
                return;

            soundParam.isLoop = isLoop;
            soundParam.isMute = false;
            soundParam.playOnAwake = false;

            soundParam.priority = 128;
            if (volume < 0)
                soundParam.volume = _sfxVolume;
            else
                soundParam.volume = volume;

            soundParam.startPosition = 0f;

            container.Init(_soundsRoot, soundType, soundParam, isPlaylist, clipIndex);

            if (parent != null)
                container.container.transform.SetParent(parent);

            _soundContainers.Add(container);
        }

        public void SetMusicVolume(float value)
        {
            //GameClient.Get<IPlayerManager>().GetPlayerData.volumeMusic = value;
            //GameClient.Get<IDataManager>().SavePlayerData();

            var containers = _soundContainers.FindAll(x => x.soundParameters.isBackground);

            if (containers != null)
            {
                foreach (var container in containers)
                {
                    container.soundParameters.volume = value;
                    container.audioSource.volume = value;
                }
            }
        }

        public void SetSoundVolume(float value)
        {
            //GameClient.Get<IPlayerManager>().GetPlayerData.volumeSound = value;
            //GameClient.Get<IDataManager>().SavePlayerData();
            _sfxVolume = value;
            var containers = _soundContainers.FindAll(x => !x.soundParameters.isBackground);

            if (containers != null)
            {
                foreach (var container in containers)
                {
                    container.soundParameters.volume = value;
                    container.audioSource.volume = value;
                }
            }
        }

        public void StopPlaying(Enumerators.SoundType soundType, int id = 0)
        {
            SoundContainer container;
            if (id == 0)
                container = _soundContainers.Find(x => x.soundType == soundType);
            else
                container = _soundContainers.Find(x => x.soundType == soundType && x.GetHashCode() == id);

            if (container != null)
            {
                container.audioSource.Stop();
                container.forceClose = true;
            }
        }

        public void StopPlaying(List<AudioClip> clips, int id = 0)
        {
            SoundContainer container;
            if (id == 0)
                container = _soundContainers.Find(x => x.soundParameters.audioClips == clips);
            else
                container = _soundContainers.Find(x => x.soundParameters.audioClips == clips && x.GetHashCode() == id);

            if (container != null)
            {
                container.audioSource.Stop();
                container.forceClose = true;
            }
        }

        public void TurnOffSound()
        {
            foreach (var container in _soundContainers)
            {
                container.audioSource.Stop();
                container.forceClose = true;
            }
        }

        private void InitializeSounds()
        {
            _gameSounds = new List<SoundTypeList>();

            SoundTypeList soundsList = null;
            int countOfTypes = Enum.GetNames(typeof(Enumerators.SoundType)).Length;

            for (int i = 0; i < countOfTypes; i++)
            {
                soundsList = new SoundTypeList();
                soundsList.soundType = (Enumerators.SoundType)i;
                soundsList.audioTypeClips = LoadAudioClipsByType(soundsList.soundType);

                _gameSounds.Add(soundsList);
            }
        }

        private List<AudioClip> LoadAudioClipsByType(Enumerators.SoundType soundType)
        {
            List<AudioClip> list = null;

            string pathToSoundsLibrary = "Sounds/";

            switch (soundType)
            {
                case Enumerators.SoundType.TUTORIAL:
                case Enumerators.SoundType.CARDS:
                    pathToSoundsLibrary = "Sounds/" + soundType.ToString();
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
        public Enumerators.SoundType soundType;
        public List<AudioClip> audioTypeClips;

        public SoundTypeList()
        {
            audioTypeClips = new List<AudioClip>();
        }
    }

    public class SoundContainer
    {
        public Enumerators.SoundType soundType;
        public AudioSource audioSource;
        public GameObject container;
        public SoundParam soundParameters;

        public bool isPlaylist;
        public bool forceClose;
        public int currentSoundIndex;

        public SoundContainer() { }

        public void Init(Transform soundsContainerRoot, Enumerators.SoundType type, SoundParam soundParam, bool playlistEnabled, int soundIndex = 0)
        {
            forceClose = false;
            currentSoundIndex = soundIndex;
            soundParameters = soundParam;
            isPlaylist = playlistEnabled;
            soundType = type;
            container = new GameObject("AudioClip " + soundType.ToString());
            container.transform.SetParent(soundsContainerRoot, false);
            audioSource = container.AddComponent<AudioSource>();

            audioSource.clip = soundParam.audioClips[currentSoundIndex];
            audioSource.volume = soundParam.volume;
            audioSource.loop = soundParam.isLoop;
            audioSource.time = soundParam.startPosition;
            audioSource.mute = soundParam.isMute;
            audioSource.playOnAwake = soundParam.playOnAwake;
            audioSource.priority = soundParam.priority;
            audioSource.spatialBlend = 1f;

            audioSource.Play();
        }

        public bool PlayNextSound()
        {
            currentSoundIndex++;

            if (currentSoundIndex < soundParameters.audioClips.Count)
            {
                audioSource.Stop();
                audioSource.clip = soundParameters.audioClips[currentSoundIndex];
                audioSource.Play();
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            if (container != null)
            {
                audioSource.Stop();
                MonoBehaviour.Destroy(container);
            }
        }
    }

    public class SoundParam
    {
        public float volume;
        public float startPosition;

        public bool isLoop;
        public bool isMute;
        public bool playOnAwake;
        public bool isBackground;

        public int priority;

        public List<AudioClip> audioClips;

        public SoundParam() { }
    }
}