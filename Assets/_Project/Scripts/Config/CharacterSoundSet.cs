using UnityEngine;

[CreateAssetMenu(fileName = "SoundConfig", menuName = "Configs/Sound/CharacterSoundSet")]
public class CharacterSoundSet : ScriptableObject
{
    [Header("Sound settings:")]
    [SerializeField] private AudioClip[] _jumpSounds;
    [SerializeField] private AudioClip[] _moveSounds;
    [SerializeField] private AudioClip[] _damageSounds;
    [SerializeField] private AudioClip[] _attackSounds;
    [SerializeField] private AudioClip[] _interactionSounds;
    [SerializeField] private AudioClip[] _pickUpSounds;

    public AudioClip[] JumpSounds => _jumpSounds;
    public AudioClip[] MoveSounds => _moveSounds;
    public AudioClip[] DamageSounds => _damageSounds;
    public AudioClip[] AttackSounds => _attackSounds;
    public AudioClip[] InteractionSounds => _interactionSounds;
    public AudioClip[] PickUp => _pickUpSounds;
}
