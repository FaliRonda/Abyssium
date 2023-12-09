using System.Collections.Generic;
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
    EnemyChasing
}

public class AudioService : IService
{
    
    private AudioConfigSO audioConfig;
    private Dictionary<SOUND_TYPE, AudioClip> soundDictionary;
    private GameObject audioGO;

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