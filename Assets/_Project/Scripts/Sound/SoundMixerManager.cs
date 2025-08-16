using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer _audioMixer;
    private static readonly string master = "masterVolume";
    private static readonly string sfx = "sfxVolume";
    private static readonly string music = "musicVolume";

    private void Awake()
    {
        SetDefaultValue(0.5f);
    }

    public void SetMasterVolume(float level)
    {
        //логарифмическое сглаживание
        //slider от 0.001f до 1 
        // linear -> dB  
        float dB = Mathf.Log10(level) * 20f;
        _audioMixer.SetFloat(master, dB);
    }

    public void SetSoundFXVolume(float level)
    {
        float dB = Mathf.Log10(level) * 20f;
        _audioMixer.SetFloat(sfx, dB);
    }

    public void SetMusicVolume(float level)
    {
        float dB = Mathf.Log10(level) * 20f;
        _audioMixer.SetFloat(music, dB);
    }

    public AudioMixer GetAudioMixer()
    {
        return _audioMixer;
    }

    public void ClearVolume()
    {
        _audioMixer.ClearFloat(master);
        _audioMixer.ClearFloat(sfx);
        _audioMixer.ClearFloat(music);
    }

    public void SetDefaultValue(float value)
    {
        SetMasterVolume(value);
        SetSoundFXVolume(value);
        SetMusicVolume(value);
    }

    private float ConvertDbToLevel(string name)
    {
        _audioMixer.GetFloat(master, out var volumeDb);
        // dB -> linear
        return Mathf.Pow(10f, volumeDb / 20f);
    }

    public float GetMasterVolume()
    {
        return ConvertDbToLevel(master);
    }

    public float GetMusicVolume()
    {
        return ConvertDbToLevel(music);
    }

    public float GetSoundFXVolume()
    {
        return ConvertDbToLevel(sfx);
    }




}
