using UnityEngine;

public class SoundMusicManager : MonoBehaviour
{
    private AudioSource _audio;

    private void Start()
    {
        if (_audio == null)
        {
            _audio = GetComponent<AudioSource>();
            StopMusic();
        }
    }

    public void StopMusic()
    {
        if (_audio != null && _audio.isPlaying)
            _audio.Stop();
    }

    public void PlayMusic()
    {
        if (_audio != null && !_audio.isPlaying)
            _audio.Play();
    }

    public void PauseMusic()
    {
        if (_audio != null)
            _audio.Pause();
    }
}
