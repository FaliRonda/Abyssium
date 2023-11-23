using UnityEngine;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Conf/Audio Configuration")]
public class AudioConfigSO : ScriptableObject
{
    public AudioClip backgroundMusic;
    public AudioClip clockTikTak;
}
