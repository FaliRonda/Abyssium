﻿using System.Collections.Generic;
using Ju.Services;
using UnityEngine;

public enum SOUND_TYPE
{
    BackgroundMusic,
    ClockTikTak
}

public class AudioService : IService
{
    
    private AudioConfigSO audioConfig;
    private Dictionary<SOUND_TYPE, AudioClip> soundDictionary;
    private bool dictionaryInitialized;

    public void InitializeSoundDictionary()
    {
        audioConfig = Resources.Load<AudioConfigSO>("Conf/AudioConfig");
        
        soundDictionary = new Dictionary<SOUND_TYPE, AudioClip>
        {
            { SOUND_TYPE.BackgroundMusic, audioConfig.backgroundMusic },
            { SOUND_TYPE.ClockTikTak, audioConfig.clockTikTak }
        };
    }

    public void Play(SOUND_TYPE soundType, float pitch, float volume)
    {
        if (!dictionaryInitialized)
        {
            InitializeSoundDictionary();
            dictionaryInitialized = true;
        }
        if (soundDictionary.TryGetValue(soundType, out AudioClip audioClip))
        {
            AudioSource audioSource = new GameObject("AudioSource").AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.Play();
            GameObject.Destroy(audioSource.gameObject, audioClip.length);
        }
        else
        {
            Debug.LogWarning("SoundType no encontrado en el diccionario.");
        }
    }
}