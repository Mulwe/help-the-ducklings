using System.Collections;
using UnityEngine;

public class DuckController : MonoBehaviour
{
    [SerializeField] private AudioClip[] _jumpSounds;
    [SerializeField] private AudioClip[] _quackSounds;
    private Rigidbody2D _rb;
    private Collider2D _collider;

    [Header("Duck sprite:")]
    [SerializeField] private SpriteRenderer _sr;
    private float _offset = 0f;

    [Header("Level Exit / Delivery Point:")]
    [SerializeField] private ExitController _exit;

    [Header("Player:")]
    [SerializeField] private PlayerAttachment _plAttach;
    [SerializeField] private PlayerController _player;

    [Header("Tracked Target:")]
    private Transform _followParent;
    public bool IsFollowing { get; private set; }

    [Header("Following setting:")]
    private Transform _followChild;
    public bool HasFollower { get; private set; }

    //[SerializeField, Range(0.05f, 1f)] 
    private float _smoothTime = 0.1f;
    private Vector3 _velocity = Vector3.zero;
    public bool IsAirborne { get; private set; }

    [Header("Catch Area settings:")]
    [SerializeField] private Transform _catchArea;
    [SerializeField] private float _catchRadius = 0.75f;
    public float offsetY = -0.15f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask duckLayer;

    [Header("Wandering walking:")]
    private float _wanderAmplitude = 1f;
    private float _wanderSpeed = 2f;
    private float _jumpForce = 5f;

    private Vector3 _startPosition;
    private Coroutine _coroutine;

    [Tooltip("Dubug")]
    private bool isLogging = false;

    private DuckAudio _duckAudio;



    /// <summary>
    /// Resets internal following-related flags and references.
    /// <para>
    /// This method <b>only updates internal properties</b> and <b>does not perform a full cleanup</b> of the object.
    /// It is intended to be used internally within the class logic.
    /// If called from outside, make sure to also update any related objects to maintain consistency.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Not recommended to call this method directly from external code unless proper synchronization
    /// with other related objects is handled.
    /// </remarks>
    public void CleanDuckFollower()
    {
        this.IsFollowing = false;
        this.HasFollower = false;
        this._followChild = null;
    }

    public Transform GetCurrentParent()
    {
        return this._followParent;
    }

    public void UpdateParentChildRelation(
        bool isFollowing, Transform parent,
        bool hasFollower, Transform child)
    {
        this.IsFollowing = isFollowing;
        this.HasFollower = hasFollower;
        this._followChild = child;
        this._followParent = parent;
    }

    public void CleanDuckParrent()
    {
        this.CancelIsAirbornAndSavePosition();
        this._followParent = null;
    }

    public void InitPlayer(PlayerController pl, PlayerAttachment plAttach)
    {
        _plAttach = plAttach;
        _player = pl;
    }

    private void Awake()
    {
        InitComponents();
        IsFollowing = false;
        HasFollower = false;
        IsAirborne = true;
        _duckAudio = GetComponent<DuckAudio>();
    }

    private void OnEnable()
    {
        SetUpBehaviour();
        _coroutine ??= StartCoroutine(Behaviour());
    }
    private void SetUpBehaviour()
    {
        IsFollowing = false;
        HasFollower = false;
        if (_rb != null)
        {
            IsAirborne = true;
            _rb.simulated = false;
        }
        else
            IsAirborne = false;
    }

    private void InitComponents()
    {
        if (_exit == null)
            _exit = UnityEngine.Object.FindFirstObjectByType<ExitController>();
        if (_collider == null)
            _collider = GetComponent<Collider2D>();
        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();

        InitCatchArea();
        CalcBounds();
        InitPlayerReferences();
    }


    void OnDisable()
    {
        StopAllCoroutines();
        _coroutine = null;
    }

    IEnumerator Behaviour()
    {
        SetUpBehaviour();
        while (true)
        {
            if (IsAirborne && !IsFollowing)
            {
                yield return FloatingBehaviour();
            }
            else if (!IsFollowing && !IsAirborne)
            {
                _duckAudio.PlayDamageSounds(transform, 1.0f);
                yield return JumpBeforeWandering();
                yield return WanderBehaviour();
            }
            else if (IsFollowing)
            {
                this._rb.linearVelocity = Vector2.zero;
                FollowBehaviour();
            }
            yield return null;
        }

    }

    private void InitCatchArea()
    {
        if (_catchArea == null && this.transform.childCount > 0)
        {
            _catchArea = this.transform.Find("CatchArea").transform;
        }
        if (_catchArea == null)
        {
            GameObject obj = new GameObject("CatchArea");
            this._catchArea = obj.transform;
        }
        if (duckLayer.value == 0)
            duckLayer = 1 << this.gameObject.layer;
    }

    private void InitPlayerReferences()
    {
        if (_player == null)
            _player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        if (_plAttach == null && _player.TryGetComponent<PlayerAttachment>(out var playerAttach))
            _plAttach = playerAttach;
        if (_plAttach != null && playerLayer.value == 0)
            playerLayer = 1 << _plAttach.gameObject.layer;
    }

    private void FollowPlayer(Transform playerTarget)
    {

        if (playerTarget == null && _plAttach == null) return;
        PlayerAttachment p = _plAttach;

        if (_plAttach == null)
            playerTarget.gameObject.TryGetComponent<PlayerAttachment>(out p);

        if (p != null)
        {
            p.UpdateLinq();
            bool playerAlreadyHasDuck = !p.hasFollower;
            if (playerAlreadyHasDuck)
            {
                //add first Duck
                AddDuck(p, p.transform, true, false, playerAlreadyHasDuck);
            }
            else
            {
                //add duck to other ducks
                AddDuck(p, p.followChild, true, false, playerAlreadyHasDuck);
            }
            _duckAudio.PlayQuackSounds(transform, 0.8f);
        }
        else
            Log("Failed to follow player");
    }

    private void AddDuck(PlayerAttachment playerA, Transform toFollow,
        bool isFollowing, bool isAirborne, bool playerHasDucks)
    {
        this._followParent = toFollow;
        this.IsFollowing = isFollowing;
        this.IsAirborne = isAirborne;
        this.HasFollower = false;

        if (playerHasDucks)
            playerA.CreateFollowChild(this.transform);
        else
            playerA.UpdateLastInLine(this.transform);
    }

    public void FollowDuck(Transform lastDuckTarget)
    {
        if (lastDuckTarget == null) return;

        // если target duck
        lastDuckTarget.gameObject.TryGetComponent<DuckController>(out DuckController lead);
        if (lead != null && !lead.HasFollower)
        {
            lead.HasFollower = true;
            lead._followChild = this.transform;

            // setup: this duck follow lead
            this.IsAirborne = false;
            this._followParent = lead.transform;

            this.IsFollowing = true;
            this.HasFollower = false;

            if (_plAttach != null)
                _plAttach.UpdateLastInLine(this.transform);
            _duckAudio.PlayQuackSounds(transform, 0.8f);

        }
        else
            Log("Failed to follow");
    }



    private void CalcBounds()
    {
        if (_sr != null)
        {
            this._offset = _sr.bounds.size.x;
        }
    }

    private void FixedUpdate()
    {
        CatchAreaCheck();
        CheckCollisions();
    }

    private void CheckCollisions()
    {
        if (_exit != null)
        {
            var exitCollider = _exit.GetComponent<Collider2D>();
            if (_collider != null && _collider.bounds.Intersects(exitCollider.bounds))
            {
                _exit.DetachLastOne();
            }
        }
    }

    public void Detach()
    {
        if (_plAttach != null)
            _plAttach.DetachLastInLine();
    }

    private IEnumerator JumpBeforeWandering()
    {
        if (_rb != null && _collider != null)
        {
            //_collider.isTrigger = true;

            this._rb.simulated = true;
            Vector2 direction = _rb.linearVelocity.normalized;

            if (direction == Vector2.zero)
            {
                float defaultAngleDeg = 110f;
                float angleRad = defaultAngleDeg * Mathf.Deg2Rad;
                direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            }
            else
            {
                direction = new Vector2(-direction.x, direction.y);
                float angle = Mathf.Atan2(direction.y, direction.x);
                direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
            }

            this._rb.AddForce(direction * _jumpForce, ForceMode2D.Impulse);
            _duckAudio.PlayJumpSounds(transform, 0.8f);
            yield return new WaitUntil(() => this._rb.linearVelocityY < 0f);
            yield return new WaitUntil(() => this._rb.linearVelocityY >= 0f);
            //_collider.isTrigger = false;
            CancelIsAirbornAndSavePosition();
        }
    }

    private IEnumerator WanderBehaviour()
    {
        Log($"Start {nameof(WanderBehaviour)}");
        this._rb.bodyType = RigidbodyType2D.Kinematic;
        this._rb.simulated = true;
        while (!IsFollowing && IsAirborne == false)
        {
            yield return new WaitForFixedUpdate();
            Vector2 currentPos = _rb.position;

            float offsetX = Mathf.Sin(Time.time * _wanderSpeed) * _wanderAmplitude;
            Vector2 targetPos = new Vector2(_startPosition.x + offsetX, _startPosition.y);
            Vector2 direction = (targetPos - currentPos).normalized;
            _rb.MovePosition(targetPos);
            if (direction.x < 0)
                _sr.flipX = true;
            else
                _sr.flipX = false;
        }
        this._rb.simulated = false;
        this._rb.bodyType = RigidbodyType2D.Dynamic;
        Log($"Exit Wandering");
    }

    //cancel Airborn, save position
    public void CancelIsAirbornAndSavePosition()
    {
        //duck.isFollowing = false;
        this.IsAirborne = false;
        this._startPosition = this.transform.position;
    }

    void FollowBehaviour()
    {
        //следуем за объектом.
        //Turnoff check Collider (но стены должно огибать) and follow
        // if (_rb != null)        this._rb.simulated = false;
        Log($"Start {nameof(FollowBehaviour)}");

        if (_player == null)
        {
            Debug.LogError("Player not init");
            return;
        }
        if (IsFollowing && _followParent != null)
        {
            this._rb.simulated = false;
            Vector3 targetPos;
            if (!_player.FacingRight())
            {
                targetPos = new Vector3(this._followParent.position.x + _offset, _player.transform.position.y);
                _sr.flipX = true;
            }
            else
            {
                targetPos = new Vector3(this._followParent.position.x - _offset, _player.transform.position.y);
                _sr.flipX = false;
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPos,
                ref _velocity, _smoothTime);
        }
    }

    IEnumerator FloatingBehaviour()
    {
        Vector2 originalPos = _rb.position;
        this._rb.simulated = true;
        this.IsAirborne = true; // it  would never be true if it under control

        Log($"Start {nameof(FloatingBehaviour)}");

        while (this.IsAirborne)
        {
            float t = 0f;
            float rnd = UnityEngine.Random.Range(2f, 7f);
            while (t < rnd && this.IsAirborne)
            {
                t += Time.deltaTime;
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
                _rb.MovePosition(originalPos);
                yield return new WaitForFixedUpdate();
            }
            if (this.IsAirborne)
                _sr.flipX = !_sr.flipX;
        }
        this.IsAirborne = false;
        this._rb.simulated = false;
    }

    private void CatchAreaCheck()
    {
        if (IsFollowing || _plAttach == null)
        {
            return;
        }

        Vector2 v = new(_catchArea.position.x, _catchArea.position.y + offsetY);
        if (Physics2D.OverlapCircle(v, _catchRadius, playerLayer))
        {
            if (!_plAttach.hasFollower)
            {
                FollowPlayer(_plAttach.transform);
            }
            else if (_plAttach.hasFollower && _plAttach.lastfollowChild != null)
            {
                FollowDuck(_plAttach.lastfollowChild);
            }
        }
        else if (_plAttach.hasFollower && _plAttach.lastfollowChild != null)
        {
            float dist = Vector2.Distance(v, _plAttach.lastfollowChild.position);
            if (dist <= _catchRadius)
            {
                FollowDuck(_plAttach.lastfollowChild);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_catchArea != null)
        {
            Gizmos.color = Color.green;
            Vector2 v = new Vector2(_catchArea.position.x, _catchArea.position.y + offsetY);
            Gizmos.DrawWireSphere(v, _catchRadius);
        }
    }


    private void Log(string msg)
    {
        if (isLogging)
        {
            Debug.Log(msg);
        }
    }
}
