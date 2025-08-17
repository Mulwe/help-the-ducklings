using System;
using System.Collections;
using UnityEngine;


public class ExitController : MonoBehaviour
{
    [SerializeField] private PlayerController _player;
    [SerializeField] private PlayerAttachment _pl;
    public int Score { get; private set; } = 0;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask exitLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask duckLayer;

    public static Action<GameObject> OnDuckReachedExit;
    public static Action<int> OnDucksCollected;
    private PlayerAudio _playerAudio;

    private Coroutine _soundRepeatPlay;
    private int _queue = 0;

    public void DetachLastOne()
    {
        if (_player == null || _pl == null || !_pl.hasFollower) return;

        int count = 0;
        int atmps = 0;

        if (_pl.lastfollowChild != null)
        {
            while (atmps < 100)
            {
                if (_pl.followChild == null) break;
                GameObject obj = _pl.DetachLastInLine()?.gameObject;
                if (obj != null)
                {
                    // лучше скрыть вместо GameObject.Destroy(obj);
                    OnDuckReachedExit?.Invoke(obj);
                    count++;
                    _queue++;
                }
                atmps++;
            }
        }
        if (_soundRepeatPlay == null && _playerAudio != null)
        {
            _soundRepeatPlay = StartCoroutine(PlayQueueOfSounds());
        }
        Score += count;
    }

    private void Awake()
    {
        Score = 0;
        GetPlayerSounds(null);
        if (this.TryGetComponent<BoxCollider2D>(out var colider))
        {
            if (colider.contactCaptureLayers == 0 && playerLayer.value != 0)
                colider.contactCaptureLayers = playerLayer;
        }
    }

    private void GetPlayerSounds(PlayerAudio playerAudio)
    {
        if (playerAudio == null && _player != null)
        {
            _playerAudio = _player.GetComponent<PlayerAudio>();
        }
        else if (playerAudio != null)
        {
            _playerAudio = playerAudio;
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Duck"))
        {

            DetachLastOne();
        }
    }

    private IEnumerator PlayQueueOfSounds()
    {
        yield return new WaitForSeconds(0.1f);
        yield return UpdatePoints(_queue);
        _queue = _queue > 5 ? _queue = 5 : _queue;
        float delay = 0.1f;
        SoundFXManager.Instance.SetSoundFxPitch(1f * _queue);
        while (_queue > 0)
        {
            if (_playerAudio != null)
            {
                yield return RepeatAction(
                    () => _playerAudio.PlayPickUpSounds(transform, 1.0f), delay);
                _queue--;
            }
        }
        SoundFXManager.Instance.SetSoundFxPitch(1f);
        _queue = 0;
        _soundRepeatPlay = null;
    }


    private IEnumerator RepeatAction(System.Action action, float delay)
    {
        action?.Invoke();
        yield return new WaitForSeconds(delay);
    }



    //update UI update points

    private IEnumerator UpdatePoints(int queueAmount)
    {
        if (queueAmount > 0)
        {
            OnDucksCollected?.Invoke(queueAmount);
        }
        yield return null;
    }

}