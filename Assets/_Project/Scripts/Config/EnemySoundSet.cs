using UnityEngine;


[CreateAssetMenu(fileName = "SoundConfig", menuName = "Configs/Sound/EnemySoundSet")]
public class EnemySoundSet : ScriptableObject
{
    [Header("Sound settings:")]
    [SerializeField] private AudioClip[] _jumpSounds;
    [SerializeField] private AudioClip[] _damageSounds;
    [SerializeField] private AudioClip[] _attackSounds;
    [SerializeField] private AudioClip[] _interactionSounds;

    public AudioClip[] JumpSounds => _jumpSounds;
    public AudioClip[] DamageSounds => _damageSounds;
    public AudioClip[] AttackSounds => _attackSounds;
    public AudioClip[] InteractionSounds => _interactionSounds;
}
