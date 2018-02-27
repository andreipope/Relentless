// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

/// <summary>
/// Utility class used to play music and sound effects. It uses an object pool of audio sources
/// internally for better performance.
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    private ObjectPool objectPool;

    public static AudioManager Instance
    {
        get
        {
            return instance ?? new GameObject("AudioManager").AddComponent<AudioManager>();
        }
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        objectPool = GetComponent<ObjectPool>();
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (instance == null)
            instance = this;
    }

    public void PlaySound(AudioClip clip, bool loop = false)
    {
        if (clip != null)
            objectPool.GetObject().GetComponent<SoundFX>().Play(clip, loop);
    }
}