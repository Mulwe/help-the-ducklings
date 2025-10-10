using System.Linq;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;

    [Header("Sound FX - GameObject to spawn:")]
    [SerializeField] private AudioSource _soundFXObject;
    [SerializeField] private SoundMusicManager _background;
    [SerializeField] private SoundMixerManager _mixer;

    public SoundMusicManager BackgroundMusic => _background;
    public SoundMixerManager AudioMixer => _mixer;
    public AudioSource AudioSource => _soundFXObject;

    private float _volume;
    private bool _overrided;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _overrided = false;
            _volume = 1.0f;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawn, float volume)
    {
        //spawn in GameObject
        if (audioClip == null)
        {
            return;
        }
        AudioSource audioSource = Instantiate(_soundFXObject, spawn.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayRandomSoundFXClip(AudioClip[] audioClip, Transform spawn, float volume)
    {
        if (audioClip == null || audioClip.Count() == 0)
        {
            return;
        }

        int rnd = UnityEngine.Random.Range(0, audioClip.Length);

        AudioSource audioSource = Instantiate(_soundFXObject, spawn.position, Quaternion.identity);
        audioSource.clip = audioClip[rnd];
        if (!_overrided)
        {
            audioSource.volume = volume;
            _volume = volume;
        }
        else
            audioSource.volume = _volume;
        audioSource.Play();
        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }

    public void ChangeVolume(float volume)
    {
        if (volume == 1.0f)
            _overrided = false;
        else
            _overrided = true;
        _volume = volume;
    }

    public void SetVolume(float volume)
    {
        ChangeVolume(volume);
    }

    public float GetVolume()
    {
        return _volume;
    }

    public void SetSoundFxPitch(float volume)
    {
        _soundFXObject.pitch = volume;
    }

    public float GetSoundFxPitch()
    {
        return _soundFXObject.pitch;
    }

    public void ResetSoundFxPitch()
    {
        _soundFXObject.pitch = 1f;
    }

    private void OnDestroy()
    {
        AudioSource[] _allClips = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource clip in _allClips)
        {
            if (clip != null && clip.gameObject != null)
                Destroy(clip.gameObject);
        }
    }
}