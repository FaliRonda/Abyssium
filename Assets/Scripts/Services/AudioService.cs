using System.Collections.Generic;
using FMODUnity;
using Ju.Services;
using UnityEngine;

public enum SOUND_TYPE
{
    BackgroundMusic,
    ClockTikTak,
    Bell,
    Spotlight,
    AngryGod,
    PjDamaged,
    PjImpact,
    SwordAttack,
    PjDash,
    ItemGot,
    EnemyDied,
    CameraChange,
    EnemyChasing,
    BossDoorClosed,
    BossMusic,
    PjStep,
    EndMusic,
    PjHitted,
    EnemySpawn
}

public class AudioService : IService
{
    
    private AudioConfigSO audioConfig;
    private Dictionary<SOUND_TYPE, AudioClip> soundDictionary;
    private GameObject audioGO;
    private FMOD.Studio.EventInstance soundEvent;

    public void Initialize(GameObject audioGO)
    {
        InitializeSoundDictionary();
        this.audioGO = audioGO;
    }
    
    private void InitializeSoundDictionary()
    {
        audioConfig = Resources.Load<AudioConfigSO>("Conf/AudioConfig");
        
        soundDictionary = new Dictionary<SOUND_TYPE, AudioClip>
        {
            { SOUND_TYPE.BackgroundMusic, audioConfig.backgroundMusic },
            { SOUND_TYPE.ClockTikTak, audioConfig.clockTikTak },
            { SOUND_TYPE.Bell, audioConfig.bell },
            { SOUND_TYPE.Spotlight, audioConfig.spotlight },
            { SOUND_TYPE.AngryGod, audioConfig.angryGod },
            { SOUND_TYPE.PjDamaged, audioConfig.pjDamaged },
            { SOUND_TYPE.PjImpact, audioConfig.pjImpact },
            { SOUND_TYPE.SwordAttack, audioConfig.swordAttack },
            { SOUND_TYPE.PjDash, audioConfig.pjdash },
            { SOUND_TYPE.ItemGot, audioConfig.itemGot },
            { SOUND_TYPE.EnemyDied, audioConfig.enemyDied },
            { SOUND_TYPE.CameraChange, audioConfig.cameraChange },
            { SOUND_TYPE.EnemyChasing, audioConfig.enemyChasing },
            { SOUND_TYPE.BossDoorClosed, audioConfig.bossDoorClosed },
            { SOUND_TYPE.BossMusic, audioConfig.bossMusic },
            { SOUND_TYPE.PjStep, audioConfig.pjStep },
            { SOUND_TYPE.EndMusic, audioConfig.endMusic },
            { SOUND_TYPE.PjHitted, audioConfig.pjHitted },
            { SOUND_TYPE.EnemySpawn, audioConfig.enemySpawn },
        };
    }

    public void Play(SOUND_TYPE soundType, float pitch, float randomRange, float volume)
    {
        if (soundDictionary.TryGetValue(soundType, out AudioClip audioClip))
        {
            pitch += Random.Range(-randomRange, randomRange);
            
            AudioSource audioSource = new GameObject("AudioSource").AddComponent<AudioSource>();
            audioSource.transform.parent = audioGO.transform; 
            audioSource.clip = audioClip;
            audioSource.volume = volume + 0.05f;
            audioSource.pitch = pitch;

            if (soundType == SOUND_TYPE.BackgroundMusic || soundType == SOUND_TYPE.EndMusic)
            {
                audioSource.loop = true;
            }
            
            audioSource.Play();
            GameObject.Destroy(audioSource.gameObject, audioClip.length);
        }
        else
        {
            Debug.LogError("SoundType no encontrado en el diccionario.");
        }
    }
    
    public void PlayFMODAudio(string eventName, Transform parentTransform)
    {
        soundEvent = RuntimeManager.CreateInstance(eventName);
        soundEvent.set3DAttributes(RuntimeUtils.To3DAttributes(parentTransform));
        soundEvent.start();
    }

    public void StopAll()
    {
        AudioSource[] audios = audioGO.GetComponentsInChildren<AudioSource>();

        foreach (AudioSource audio in audios)
        {
            audio.Stop();
            GameObject.Destroy(audio.gameObject);
        }
    }
}