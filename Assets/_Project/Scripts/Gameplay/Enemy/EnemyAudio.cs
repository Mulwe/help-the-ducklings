using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
    [SerializeField] private NPCSoundSet _soundSet;

    private void Start()
    {
        if (_soundSet == null)
            Debug.Log($"{this}: Sound not init");
    }

    public void PlayJumpSounds(Transform position, float volume)
    {
        if (SoundFXManager.Instance == null || _soundSet == null) return;
        var sounds = _soundSet.JumpSounds;
        if (sounds == null)
            Debug.LogError($"{this.name} Sound not attached");
        SoundFXManager.Instance.PlayRandomSoundFXClip(sounds, position, volume);
    }


    public void PlayDamageSounds(Transform position, float volume)
    {
        if (SoundFXManager.Instance == null || _soundSet == null) return;
        var sounds = _soundSet.DamageSounds;
        if (sounds == null)
            Debug.LogError($"{this.name} Sound not attached");
        SoundFXManager.Instance.PlayRandomSoundFXClip(sounds, position, volume);
    }

    public void PlayInteractionSounds(Transform position, float volume)
    {
        if (SoundFXManager.Instance == null || _soundSet == null) return;
        var sounds = _soundSet.InteractionSounds;
        if (sounds == null)
            Debug.LogError($"{this.name} Sound not attached");

        SoundFXManager.Instance.PlayRandomSoundFXClip(sounds, position, volume);
    }
}
