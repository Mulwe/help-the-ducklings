using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private CharacterSoundSet _soundSet;

    private void Start()
    {
        if (_soundSet == null)
            Debug.Log($"{this}: Sound not init");
    }

    public void PlayJumpSounds(Transform position, float volume)
    {
        if (SoundFXManager.Instance == null || _soundSet == null) return;
        if (_soundSet.JumpSounds == null)
            Debug.LogError($"{this.name} Sound not attached");
        SoundFXManager.Instance.PlayRandomSoundFXClip(_soundSet.JumpSounds, position, volume);
    }

    public void PlayMoveSounds(Transform position, float volume)
    {
        if (SoundFXManager.Instance == null || _soundSet == null) return;
        if (_soundSet.MoveSounds == null)
            Debug.LogError($"{this.name} Sound not attached");

        SoundFXManager.Instance.PlayRandomSoundFXClip(_soundSet.MoveSounds, position, volume);
    }

    public void PlayDamageSounds(Transform position, float volume)
    {
        if (SoundFXManager.Instance == null || _soundSet == null) return;
        if (_soundSet.DamageSounds == null)
            Debug.LogError($"{this.name} Sound not attached");
        SoundFXManager.Instance.PlayRandomSoundFXClip(_soundSet.DamageSounds, position, volume);

    }

    public void PlayAttackSounds(Transform position, float volume)
    {
        if (SoundFXManager.Instance == null || _soundSet == null) return;
        if (_soundSet.AttackSounds == null)
            Debug.LogError($"{this.name} Sound not attached");

        SoundFXManager.Instance.PlayRandomSoundFXClip(_soundSet.AttackSounds, position, volume);
    }
    public void PlayInteractionSounds(Transform position, float volume)
    {
        if (SoundFXManager.Instance == null || _soundSet == null) return;
        var sounds = _soundSet.InteractionSounds;
        if (sounds == null)
            Debug.LogError($"{this.name} Sound not attached");

        SoundFXManager.Instance.PlayRandomSoundFXClip(sounds, position, volume);
    }

    public void PlayPickUpSounds(Transform position, float volume)
    {
        if (SoundFXManager.Instance == null || _soundSet == null) return;
        var sounds = _soundSet.PickUp;
        if (sounds == null)
            Debug.LogError($"{this.name} Sound not attached");

        SoundFXManager.Instance.PlayRandomSoundFXClip(sounds, position, volume);
    }
}
