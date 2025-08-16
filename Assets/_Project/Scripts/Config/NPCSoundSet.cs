using UnityEngine;

[CreateAssetMenu(fileName = "NPCSoundSet", menuName = "Configs/Sound/NPCSoundSet")]
public class NPCSoundSet : ScriptableObject
{
    [Header("NPC Sound settings:")]
    [SerializeField] private AudioClip[] _jumpSounds;
    [SerializeField] private AudioClip[] _damageSounds;
    [SerializeField] private AudioClip[] _interactionSounds;

    public AudioClip[] JumpSounds => _jumpSounds;

    public AudioClip[] DamageSounds => _damageSounds;

    public AudioClip[] InteractionSounds => _interactionSounds;


}
