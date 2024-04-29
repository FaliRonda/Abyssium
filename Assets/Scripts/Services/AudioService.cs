using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Ju.Services;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

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
    private EventInstance soundEvent;
    private bool waitBetweenSameAudio;
    private string lastEventName;
    private List<Sequence> infiniteAudioSequences;
    private EventInstance backgroundMusic;
    private float originalBackgroundVolume;
    private List<EventInstance> fmodAudios;

    public void Initialize(GameObject audioGO)
    {
        InitializeSoundDictionary();
        this.audioGO = audioGO;
        infiniteAudioSequences = new List<Sequence>();
        fmodAudios = new List<EventInstance>();
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

    public void PlayUnityAudio(SOUND_TYPE soundType, float pitch, float randomRange, float volume)
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

    public void StopAllUnityAudios()
    {
        AudioSource[] audios = audioGO.GetComponentsInChildren<AudioSource>();

        foreach (AudioSource audio in audios)
        {
            audio.Stop();
            GameObject.Destroy(audio.gameObject);
        }
    }
    
    public EventInstance PlayFMODAudio(string eventName, Transform parentTransform)
    {
        soundEvent = RuntimeManager.CreateInstance(eventName);
        soundEvent.set3DAttributes(RuntimeUtils.To3DAttributes(parentTransform));
        
        if (lastEventName != eventName || !waitBetweenSameAudio)
        {
            lastEventName = eventName;

            waitBetweenSameAudio = true;
            Sequence waitBetweenSameAudioSequence = DOTween.Sequence();
            waitBetweenSameAudioSequence
                .AppendInterval(0.1f)
                .AppendCallback(() => { waitBetweenSameAudio = false; });
        
            soundEvent.start();

            if (eventName.Contains("DemoScene_Music"))
            {
                backgroundMusic = soundEvent;
                backgroundMusic.getVolume(out originalBackgroundVolume);
            }
        }

        fmodAudios.Add(soundEvent);
        
        return soundEvent;
    }

    public bool FMODAudioIsPlaying(EventInstance eventInstance)
    {
        eventInstance.getPlaybackState(out var playbackState);
        return playbackState != PLAYBACK_STATE.STOPPED;
    }

    public void PlayInfiniteFMODAudio(string eventName, Transform transform, float delay)
    {
        EventInstance instance = new EventInstance();
        Sequence infiniteAudioSequence = DOTween.Sequence();
        infiniteAudioSequences.Add(infiniteAudioSequence);
        
        infiniteAudioSequence
            .AppendCallback(() =>
            {
                instance = PlayFMODAudio(eventName, transform);
            })
            .AppendInterval(delay)
            .SetLoops(-1)
            .OnKill((() => instance.stop(STOP_MODE.ALLOWFADEOUT)));
    }
    
    public void StopAllInfiniteFMODAudio()
    {
        foreach (Sequence sequence in infiniteAudioSequences)
        {
            sequence.Kill();
        }
    }

    public void ResetFMODBackgroundVolume()
    {
        backgroundMusic.getVolume(out float currentVolume);
        DOTween.To(() => currentVolume, x =>
        {
            backgroundMusic.setVolume(x);
        }, originalBackgroundVolume, 0.5f);
    }

    public void UpdateFMODBackgroundVolume(float factor)
    {
        DOTween.To(() => originalBackgroundVolume, x =>
        {
            backgroundMusic.setVolume(x);
        }, originalBackgroundVolume * factor, 0.5f);
    }

    public void StopAllFMODAudios()
    {
        foreach (EventInstance fmodAudio in fmodAudios)
        {
            fmodAudio.stop(STOP_MODE.ALLOWFADEOUT);
        }
        
        fmodAudios.Clear();
    }
}