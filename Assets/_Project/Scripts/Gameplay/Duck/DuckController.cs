using System;
using System.Collections;
using UnityEngine;

public class DuckController : MonoBehaviour
{
    //HACK: debugInfo удалить после
    [SerializeField, TextArea] private string debugInfo;

    private Rigidbody2D _rb;
    private Collider2D _collider;

    [Header("Level Exit / Delivery Point:")]
    private ExitController _exit;

    [Header("Duck sprite:")]
    [SerializeField] private SpriteRenderer _sr;
    private float _offset = 0f;

    //[Header("Player:")]
    [SerializeField] private PlayerAttachment _plAttach;
    [SerializeField] private PlayerController _player;

    [Header("Tracked parent:")]
    private Transform _followParent;
    public bool HasFollower { get; private set; }

    [Header("Following child:")]
    private Transform _followChild;
    public bool IsFollowing { get; private set; }
    public bool IsAirborne { get; private set; }

    private float _smoothTime = 0.1f;

    private Vector3 _velocity = Vector3.zero;

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

    [Tooltip("Experimental: Ducks chain to last duck instead of player. May cause wall clipping!")]
    public bool experimentalChaining = false;

    private Vector3 _startPosition;
    private Coroutine _coBehaviour = null;
    private Coroutine _coReturnToSpawn = null;

    /// <summary>
    /// External classes: object is locked, do not modify state or physics
    /// </summary>
    public bool IsLocked { get; private set; }

    private DuckAudio _duckAudio;
    private Vector3 _spawnSpot = Vector3.zero;

    [Tooltip("Dubug")]
    private bool isLogging = false;

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

    public Transform GetCurrentChild()
    {
        return this._followChild;
    }

    public void UpdateParentChildRelationship(
        bool isFollowing, Transform parent,
        bool hasFollower, Transform child)
    {
        this.IsFollowing = isFollowing;
        this.HasFollower = hasFollower;
        this._followChild = child;
        this._followParent = parent;
    }

    /// <summary>
    /// Before using this object, ensure that its parent has cleared any reference to it.
    /// Otherwise, the object won't know which parent to follow.
    /// </summary>
    public void CleanDuckParent()
    {
        this.IsAirborne = false;
        this._followParent = null;
    }

    public void CleanDuckChild()
    {
        this._followChild = null;
        this.HasFollower = false;
    }

    public void InitPlayer(PlayerController pl, PlayerAttachment plAttach)
    {
        _plAttach = plAttach;
        _player = pl;
    }

    private void Awake()
    {
        _spawnSpot = this.transform.position;
        InitComponents();
        /*
        IsFollowing = false;
        HasFollower = false;
        IsAirborne = true;
        IsBusy = false;
        */
        _duckAudio = GetComponent<DuckAudio>();
    }

    private void OnEnable()
    {
        OutOfBoundsController.OnOutOfBounds += HandleOutOfBounds;
        SetUpBehaviour();
        _coBehaviour ??= StartCoroutine(Behaviour());
    }

    private void SetUpBehaviour()
    {
        IsFollowing = false;
        HasFollower = false;
        IsLocked = false;
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

    private void OnDisable()
    {
        OutOfBoundsController.OnOutOfBounds -= HandleOutOfBounds;
        StopAllCoroutines();
        SetUpBehaviour();
        _coBehaviour = null;
        _coReturnToSpawn = null;
    }

    private IEnumerator Behaviour()
    {
        SetUpBehaviour();
        while (true)
        {
            if (IsAirborne && !IsFollowing)
            {
                debugInfo = nameof(FloatingBehaviour);
                yield return FloatingBehaviour();
            }
            else if (!IsFollowing && !IsAirborne)
            {
                _duckAudio.PlayDamageSounds(transform, 1.0f);
                debugInfo = nameof(JumpBeforeWandering);
                yield return JumpBeforeWandering();
                debugInfo = nameof(WanderBehaviour);
                yield return WanderBehaviour();
            }
            else if (IsFollowing)
            {
                this._rb.linearVelocity = Vector2.zero;
                debugInfo = nameof(FollowBehaviour);
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
            p.UpdateLink();
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
        //CheckCollisions();
    }

    private void CheckCollisions()
    {
        if (_exit != null && !IsLocked)
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
        }
    }

    private IEnumerator WanderBehaviour()
    {
        Log($"Start {nameof(WanderBehaviour)}");

        UpdateWanderingPosition();
        this._rb.bodyType = RigidbodyType2D.Kinematic;
        this._rb.simulated = true;
        float t = 0f;

        while (!IsFollowing && !IsAirborne)
        {
            t += Time.deltaTime;
            if (IsLocked && t > 4f)
            {
                this._rb.simulated = false;
                this._rb.bodyType = RigidbodyType2D.Dynamic;
                IsLocked = false;
                HandleOutOfBounds(gameObject);
                yield break;
            }
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

    private void FollowBehaviour()
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

    private IEnumerator FloatingBehaviour()
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
        if (IsFollowing || _plAttach == null || IsLocked)
        {
            return;
        }

        Vector2 position = new(_catchArea.position.x, _catchArea.position.y + offsetY);
        if (Physics2D.OverlapCircle(position, _catchRadius, playerLayer))
        {
            if (!_plAttach.hasFollower)
            {
                FollowPlayer(_plAttach.transform);
            }
            else if (_plAttach.hasFollower && _plAttach.lastFollowChild != null)
            {
                FollowDuck(_plAttach.lastFollowChild);
            }
        }
        else if (experimentalChaining)
        {
            TryConnectToLastInQueue(position);
        }
    }

    private void TryConnectToLastInQueue(Vector2 position)
    {
        if (_plAttach.hasFollower && _plAttach.lastFollowChild != null)
        {
            float dist = Vector2.Distance(position, _plAttach.lastFollowChild.position);
            if (dist <= _catchRadius)
            {
                FollowDuck(_plAttach.lastFollowChild);
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

    private void HandleOutOfBounds(GameObject outOfBoundDuck)
    {
        if (outOfBoundDuck == null || IsLocked || _coReturnToSpawn != null) return;
        if (outOfBoundDuck != this.gameObject) return;
        IsLocked = true;
        Debug.Log("OutOfBounds?");
        ReturnBackToSpawn();
    }

    private void Log(string msg)
    {
        if (isLogging)
        {
            Debug.Log(msg);
        }
    }

    //TODO:  ReturnBackToSpot
    private void ReturnBackToSpawn()
    {
        _coReturnToSpawn = StartCoroutine(ReturnToSpawnPoint());
    }

    private IEnumerator ReturnToSpawnPoint()
    {
        WaitForSeconds wait = new WaitForSeconds(0.5f);
        Color baseColor = _sr.color;
        _sr.color = new Color(_sr.color.r, _sr.color.g, _sr.color.b, 0.5f);
        //end the behaivior cycle
        if (_coBehaviour != null)
        {
            StopCoroutine(_coBehaviour);
            _coBehaviour = null;
        }

        debugInfo = nameof(MoveToSpawnThroughWalls);
        if (_rb != null)
            yield return MoveToSpawnThroughWalls();
        else
        {
            _coBehaviour ??= StartCoroutine(Behaviour());
            _coReturnToSpawn = null;
            yield break;
        }

        debugInfo = nameof(PrepareForFloating);
        //Reset object to Spawn state
        PrepareForFloating();
        _duckAudio.PlayQuackSounds(this.transform, 1.0f);

        //HACK: удалить после дебага
        /*
        Debug.Log($"Duck refs:\n" +
            $"{nameof(IsFollowing)} -> <color=Green>{IsFollowing}</color>\n" +
            $"{nameof(IsAirborne)} -> <color=Green>{IsAirborne}</color>\n" +
            $"{nameof(IsBusy)} -> <color=Green>{IsBusy}</color>\n" +
            $"{nameof(_followParent)} -> <color=cyan>{_followParent}</color>\n" +
            $"{nameof(_followChild)} -> <color=cyan>{_followChild}</color>\n" +
            $"{nameof(_followChild)} -> <color=cyan>{}</color>\n" +
            $"");
        */

        // IsBusy will automatically resets in Behaviour

        _coBehaviour ??= StartCoroutine(Behaviour());
        _sr.color = baseColor;
        _coReturnToSpawn = null;
    }

    private IEnumerator MoveToSpawnThroughWalls()
    {
        // Disable physics to move the object through walls
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.simulated = false;

        // Flip sprite to match movement direction
        FlipSpriteToMovement(transform.position, _spawnSpot);

        // Start move to spawn point
        float maxSpeedVisible = 2f;
        float maxSpeedNotVisible = 10f;

        _velocity = Vector3.zero;
        while (Vector3.Distance(transform.position, _spawnSpot) > 0.1f && IsLocked)
        {
            if (CameraController.Instance != null && CameraController.Instance.IsObjectVisible2D(this.gameObject))
                transform.position = Vector3.SmoothDamp(transform.position, _spawnSpot, ref _velocity, _smoothTime, maxSpeedVisible);
            else
                transform.position = Vector3.SmoothDamp(transform.position, _spawnSpot, ref _velocity, _smoothTime, maxSpeedNotVisible);

            yield return null;
        }

        // After reaching the spawn point, set the object's position to the spawn
        transform.position = _spawnSpot;
    }

    private void PrepareForFloating()
    {
        // Enable physics
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.simulated = false;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        // Prepare object for pickup (restore original floating behavior, as after spawn)
        this.IsAirborne = false;
        this._startPosition = this.transform.position;
    }

    private void FlipSpriteToMovement(Vector3 mainPosition, Vector3 targetPosition)
    {
        if (targetPosition.x > mainPosition.x)
            _sr.flipX = false; //right
        else
            _sr.flipX = true;  //left
    }

    private void UpdateWanderingPosition()
    {
        this._rb.simulated = false;

        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        this.IsAirborne = false;
        this._startPosition = this.transform.position;

        this._rb.simulated = true;
    }
}