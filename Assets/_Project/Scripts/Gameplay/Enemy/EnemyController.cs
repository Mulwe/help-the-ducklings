using System;
using System.Collections;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy:")]
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Collider2D _col;
    [Header("Kick Settings")]
    [Tooltip("KickPower: how hard the kick will push the player")]
    [SerializeField] private float _kickPower = 30f;
    [Tooltip("Minimum force for jump.")]
    [SerializeField] private float _jumpForce = 20f;
    private float _defaultGravity;


    [Header("Vision distance:")]
    public float visionDistance = 10f;

    [Header("Alertness Timeout:")]
    [SerializeField] private float _alertedSec = 10f;
    private Coroutine _maintainAlert;
    private bool _alertTimerActive = false;

    [Header("Move Force:")]
    [SerializeField] private float _moveForce = 5f;
    private float _defaultMoveForce = 5f;               //rewrited in Awake
    private float _flipThreshold = 0.01f;            //Sprite flip;

    //[Header("Layers:")]
    private static LayerMask _ownMask;
    private static LayerMask _duckMask;
    private static LayerMask _playerMask;
    private static LayerMask _groundLayer;
    private static LayerMask _wallLayer;

    private PlayerController _playerRef;
    public bool _isPatrolling { get; private set; }
    public bool _isChasing { get; private set; }
    public bool _isAttacking { get; private set; }

    private bool _isPlayerInView;

    [Header("Jump Settings")]
    [SerializeField] private float _jumpStartDistance = 2f;
    private bool _canMove = false;
    private bool _startJumping = false;
    private bool _isGrounded = false;

    [Header("GroundCheck")]
    public Transform groundPos;
    public Vector2 groundSize = new Vector2(0.5f, 0.05f);
    private float _groundLength;

    private Coroutine _mainCoroutine;

    private EnemyAudio _enemyAudio;
    private Coroutine _audioRoutine;
    private readonly WaitForSeconds _waitShort = new(0.2f);
    private readonly WaitForFixedUpdate _waitForFixedUpdate = new();
    private readonly WaitForSeconds _wait5Sec = new(5f);


    bool IsLogging = false;


    private void Awake()
    {
        InitComponents();
        InitSettings();
        InitLayerMasks();
        _groundLength = groundSize.magnitude;
        _enemyAudio = GetComponent<EnemyAudio>();
        if (_rb != null && _sr != null && _col != null)
        {
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.simulated = true;
            _defaultGravity = _rb.gravityScale;
            _defaultMoveForce = _moveForce;
            _mainCoroutine = StartCoroutine(StartPatrol());
        }
        else
            Debug.LogError($"{this}: not init");
    }

    private void OnEnable()
    {
        InitSettings();
        _mainCoroutine ??= StartCoroutine(StartPatrol());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _mainCoroutine = null;
    }
    private void InitLayerMasks()
    {
        _ownMask.value = 1 << this.gameObject.layer;
        _duckMask = LayerMask.GetMask("Duck");
        _playerMask = LayerMask.GetMask("Player");
        _groundLayer = LayerMask.GetMask("Ground");
        _wallLayer = LayerMask.GetMask("Wall");
    }

    private void InitComponents()
    {
        _playerRef = GameObject.FindFirstObjectByType<PlayerController>();
        _rb ??= GetComponent<Rigidbody2D>();
        _sr ??= GetComponentInChildren<SpriteRenderer>();
        _col ??= GetComponent<Collider2D>();
        if (groundPos == null)
            groundPos = transform.Find("GroundCheck");
    }
    private void InitSettings()
    {
        _isPatrolling = false;
        _isChasing = false;
        _isAttacking = false;
        _startJumping = false;
        _canMove = true;
    }

    private IEnumerator StartPatrol()
    {
        Coroutine viewZone = StartCoroutine(VisionCheckLoop());
        Coroutine groundCheck = StartCoroutine(GroundChecker());


        if (_audioRoutine == null)
        {
            _audioRoutine = StartCoroutine(PlaySoundState(
                () => _enemyAudio.PlayInteractionSounds(transform, 1.0f),
                () => _enemyAudio.PlayDamageSounds(transform, 1.0f)
                ));
        }

        _isPatrolling = true;
        while (true)
        {
            if (_isPatrolling)
            {
                Log("isPatrolling");
                float offset = UnityEngine.Random.Range(0.2f, 10f);
                float distance = UnityEngine.Random.Range(-offset, offset);
                Vector2 startPos = _rb.position;
                Vector2 targetPos = new(startPos.x + distance, startPos.y);
                yield return Move(targetPos);

                float waitTime = UnityEngine.Random.Range(1f, 2.5f);
                float timer = 0f;
                while (timer < waitTime && _isPatrolling && !_isChasing)
                {
                    timer += Time.fixedDeltaTime;
                    yield return _waitForFixedUpdate;
                }
            }
            else if (_isChasing)
            {
                Log("isChasing");
                _enemyAudio.PlayDamageSounds(transform, 1.0f);
                yield return KeepChasing();
            }
            else if (_isAttacking)
            {
                Log("isAttacking");
            }
            yield return _waitForFixedUpdate;
        }
    }

    private void Log(string msg)
    {
        if (IsLogging)
            Debug.Log(msg);
    }

    private IEnumerator KeepChasing()
    {
        float isAlerted = _alertedSec;
        float t = 0f;
        _canMove = true;

        while (_isChasing && t < isAlerted)
        {
            t += Time.fixedDeltaTime;
            if (_playerRef != null)
            {
                if (_isGrounded && _canMove && !_startJumping)
                {
                    Vector2 currentPos = _rb.position;
                    Vector2 targetPos = new Vector2(_playerRef.transform.position.x, currentPos.y);

                    MoveRbToPosition(targetPos, currentPos);
                }
                else if (_isGrounded && _startJumping)
                {
                    yield return Jump();
                }
            }
            yield return _waitForFixedUpdate;
        }
        CancelAlertness();
        _canMove = false;
    }


    IEnumerator VisionCheckLoop()
    {

        float time = 0f;
        float delay = 1.5f;
        while (true)
        {
            time += Time.deltaTime;
            CheckLineOfSight();

            if (_isPlayerInView || _alertTimerActive)
                SwitchToAggressive();
            else
                SwitchToPassive();

            if (time > delay)
            {
                if (_isPlayerInView || _alertTimerActive)
                    _enemyAudio.PlayDamageSounds(transform, 1.0f);
                else
                    _enemyAudio.PlayInteractionSounds(transform, 1.0f);
                time = 0f;
                delay = UnityEngine.Random.Range(1.5f, 3.5f);
            }
            yield return _waitShort;
        }
    }

    private IEnumerator PlaySoundState(System.Action passiveSound, System.Action aggressiveSound)
    {
        // Debug.Log($"PlaySoundState Starts");
        while (true)
        {
            //Debug.Log($"{this.name} make sound");
            if (_isPlayerInView || _alertTimerActive)
                aggressiveSound?.Invoke();
            else
                passiveSound?.Invoke();
            yield return _wait5Sec;
        }
    }


    IEnumerator GroundChecker()
    {
        while (true)
        {
            if (IsOnGround())
                _rb.gravityScale = _defaultGravity;
            else
                _rb.gravityScale = _defaultGravity * 2.5f;
            yield return _waitShort;
        }
    }

    private void SwitchToAggressive()
    {
        if (!_isChasing)
        {
            _moveForce = _defaultMoveForce * 1.5f;
            _isChasing = true;
            _isPatrolling = false;
            UpdateSpriteFlipTowardsPlayer();
        }
    }

    private void SwitchToPassive()
    {
        if (!_isPatrolling)
        {
            _moveForce = _defaultMoveForce;
            _isChasing = false;
            _isPatrolling = true;
            _canMove = true;
        }
    }



    //check is player on line & if there the obstackles
    private bool CheckLineOfSight()
    {
        Vector2 origin = transform.position;
        Vector2 direction = (_sr.flipX) ? Vector2.left : Vector2.right;
        int obstaclesCounter = 0;
        bool meetPlayer = false;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, visionDistance, _playerMask | _wallLayer | _groundLayer);

        foreach (var hit in hits)
        {
            Vector2 vectorToHit = direction * hit.distance;
            // Debug.DrawRay(origin, vectorToHit, Color.green, 1f);

            int bitMask = 1 << hit.collider.gameObject.layer;

            if ((bitMask & (_wallLayer.value | _groundLayer.value)) != 0)
            {
                obstaclesCounter++;
                float distanceToObstacle = hit.distance;
                bool isNearObstacles = ShouldStartJumpingAdaptive(hit.distance);

                if (_isChasing && _isGrounded && isNearObstacles && _alertTimerActive)
                {
                    if (!_startJumping)
                    {
                        _startJumping = true;
                        break;
                    }
                }
            }
            else if ((bitMask & _playerMask) != 0)
            {
                meetPlayer = true;
                PlayerAhead(hit, obstaclesCounter > 0);
                break;
            }
        }
        if (!meetPlayer && _maintainAlert == null)
            CancelAlertness();
        return meetPlayer;
    }



    private void PlayerAhead(RaycastHit2D hit, bool obstaclesInFront)
    {
        UpdateSpriteFlipTowardsPlayer();
        if (obstaclesInFront)
        {
            if (!_startJumping) _startJumping = true;
        }
        // player already found, update alerted state
        // if found player second time, reset timer
        if (_maintainAlert == null)
        {
            _alertTimerActive = false;
            _maintainAlert = StartCoroutine(UpdateAlertness(
                     _alertedSec,
                    () => _isPlayerInView = true,
                    () => _isPlayerInView = false,
                    ShouldReset: () => _alertTimerActive));
        }
        if (_maintainAlert != null && !_alertTimerActive)
        {
            _alertTimerActive = true;
        }
    }


    private IEnumerator DisableMovementForSeconds(float delay)
    {
        _canMove = false;
        yield return new WaitForSeconds(delay);
        _canMove = true;
    }

    private bool ShouldStartJumpingAdaptive(float distanceToObstacle)
    {
        float baseDistance = 1.5f;

        float speedMultiplier = Mathf.Clamp01(_moveForce / _defaultMoveForce);
        float adaptiveDistance = baseDistance * (1f + speedMultiplier);

        return distanceToObstacle < adaptiveDistance;
    }

    private IEnumerator StepBackBeforeJump()
    {
        float stepBackDistance = 0.5f;

        Vector2 stepBackDirection = _sr.flipX ? Vector2.right : Vector2.left;
        Vector2 targetPosition = _rb.position + stepBackDirection * stepBackDistance;

        RaycastHit2D backCheck = Physics2D.Raycast(_rb.position, stepBackDirection,
                                                  stepBackDistance, _wallLayer | _groundLayer);
        if (backCheck.collider == null)
        {
            float stepSpeed = 8f;
            while (Vector2.Distance(_rb.position, targetPosition) > 0.1f)
            {
                Vector2 newPos = Vector2.MoveTowards(_rb.position, targetPosition,
                                                   stepSpeed * Time.fixedDeltaTime);
                _rb.MovePosition(newPos);
                yield return _waitForFixedUpdate;
            }
        }
        else
        {
            //Log("Can't step back - obstacle behind");
        }

        yield return new WaitForSeconds(0.1f); // pause before jump
    }

    private IEnumerator Jump()
    {
        Coroutine coroutineDelay = null;
        if (_rb != null && _startJumping)
        {
            Vector2 direction = CalcDirection(_rb);

            coroutineDelay = StartCoroutine(DisableMovementForSeconds(10f));
            yield return StepBackBeforeJump();

            _rb.linearVelocity = Vector2.zero;

            //jump
            if (_enemyAudio != null)
                _enemyAudio.PlayJumpSounds(transform, 1.0f);
            this._rb.AddForce(direction * UnityEngine.Random.Range(_jumpForce, _jumpForce * 1.2f), ForceMode2D.Impulse);
            yield return new WaitUntil(() => this._rb.linearVelocityY <= 0f);

            float controlDuration = 1.5f;
            float startTime = Time.fixedTime;
            float horizontalForce = 5f;
            //extra force to set the direction of jump
            while (!_isGrounded && (Time.fixedTime - startTime) < controlDuration)
            {
                _rb.AddForce(Vector2.left * horizontalForce, ForceMode2D.Force);
                yield return new WaitForFixedUpdate();
            }

            StopCoroutine(coroutineDelay);
            StartCoroutine(DisableMovementForSeconds(0.01f));
            _startJumping = false;
        }
    }


    private Vector2 CalcDirection(Rigidbody2D rb)
    {

        Vector2 rayDirection = _sr.flipX ? Vector2.left : Vector2.right;
        RaycastHit2D obstacleHit = Physics2D.Raycast(transform.position, rayDirection,
                                                   _jumpStartDistance, _wallLayer | _groundLayer);

        if (obstacleHit.collider != null)
        {
            return CalculateJumpOverObstacle(obstacleHit);
        }
        else
        {
            return CalcDirection_Adaptive();
        }
    }

    private Vector2 CalculateJumpOverObstacle(RaycastHit2D obstacle)
    {
        float obstacleHeight = obstacle.collider.bounds.max.y - transform.position.y;
        float obstacleDistance = obstacle.distance;

        float horizontalDirection = _sr.flipX ? -1f : 1f;

        float minVerticalSpeed = Mathf.Sqrt(2f * Physics2D.gravity.magnitude * (obstacleHeight + 1f));

        float horizontalSpeed = (obstacleDistance + 1f) / (minVerticalSpeed / Physics2D.gravity.magnitude);

        Vector2 jumpVector = new Vector2(horizontalDirection * horizontalSpeed, minVerticalSpeed);
        return jumpVector.normalized;
    }

    private Vector2 CalcDirection_Adaptive()
    {
        Vector2 toPlayer = _playerRef.transform.position - transform.position;
        float horizontalDirection = Mathf.Sign(toPlayer.x);
        float distance = Mathf.Abs(toPlayer.x);
        float verticalComponent = Mathf.Clamp(distance * 0.3f, 0.8f, 1.5f);

        return new Vector2(horizontalDirection, verticalComponent).normalized;
    }


    private void MoveRbToPosition(Vector2 targetPos, Vector2 currentPos)
    {
        float fallThresholdY = _rb.position.y;
        Vector2 dir = (targetPos - currentPos);

        UpdateSpriteFlip(dir);

        float horizontalDistance = Mathf.Abs(dir.x);
        if (horizontalDistance <= 0.05f)
        {
            if (_canMove) _rb.MovePosition(targetPos);
            return;
        }

        if (IsFalling(currentPos, fallThresholdY))
        {
            _canMove = false;
            return;
        }

        //Ограничиваем максимальное движение за кадр так как двигаем объект без участия Rigidbody
        float maxDistanceThisFrame = _moveForce * Time.fixedDeltaTime;
        Vector2 movement = dir.normalized * maxDistanceThisFrame;
        if (horizontalDistance < maxDistanceThisFrame)
        {
            //движение только по X
            movement = new Vector2(dir.x, 0);
        }
        else
        {
            //ограничиваем движение по X, Y тот же
            movement = new Vector2(Mathf.Sign(dir.x) * maxDistanceThisFrame, 0);
        }

        if (_canMove) _rb.MovePosition(currentPos + movement);
    }

    private bool IsFalling(Vector2 currentPos, float fallThresholdY)
    {
        return currentPos.y < fallThresholdY - 0.1f;
    }

    private bool IsOnGround()
    {
        if (Physics2D.OverlapBox(groundPos.position, groundSize, 0, _groundLayer))
        {
            return _isGrounded = true;
        }
        return _isGrounded = false;
    }

    private void CancelAlertness()
    {
        if (_maintainAlert != null)
        {
            StopCoroutine(_maintainAlert);
            _maintainAlert = null;
        }
        _isPlayerInView = false;
        _alertTimerActive = false;
        _isChasing = false;
    }

    private IEnumerator UpdateAlertness(float delay, System.Action setTrue, System.Action setFalse, Func<bool> ShouldReset)
    {
        yield return ResetCountDown(delay, setTrue, setFalse, ShouldReset);
        yield return null;
        _alertTimerActive = false;
        _maintainAlert = null;
    }

    private IEnumerator ResetCountDown(float delay, System.Action setTrue, System.Action setFalse, Func<bool> ShouldReset)
    {
        float timer = delay;
        setTrue?.Invoke();
        while (timer > 0f)
        {
            if (ShouldReset())
                timer = delay;
            timer -= Time.deltaTime;
            yield return null;
        }
        setFalse?.Invoke();
    }

    private IEnumerator Move(Vector2 targetPos)
    {
        Vector2 lastPosition = _rb.position;
        float fallThresholdY = _rb.position.y;
        int stuckFrames = 0;

        _canMove = true;

        while (_canMove && _isGrounded)
        {
            Vector2 currentPos = _rb.position;
            Vector2 direction = targetPos - currentPos;
            float sqrDistance = direction.sqrMagnitude;


            UpdateSpriteFlip(direction);


            if (IsFalling(currentPos, fallThresholdY))
            {
                //break if falling  
                yield break;
            }

            if (sqrDistance <= 0.05f)
            {
                //break if reached the distance  
                _rb.MovePosition(targetPos);
                break;
            }

            Vector2 newPos = currentPos + _moveForce * Time.fixedDeltaTime * direction.normalized;
            _rb.MovePosition(newPos);

            // игрок может загнать врага в стену и оптимизированный sqrDistance не даст выйти из цикла
            // так как событие sqrDistance <= 0.05f никогда не наступит. Проверяем кадры  
            //stuck detection - more than 20 frames on the same spot - stucked
            if (Vector2.SqrMagnitude(_rb.position - lastPosition) < 0.001f)
            {
                stuckFrames++;
                if (stuckFrames > 20)
                {
                    _canMove = false;
                    break;
                }
            }
            else
                stuckFrames = 0;

            lastPosition = _rb.position;
            yield return _waitForFixedUpdate;
        }
        _canMove = false;
    }

    private void UpdateSpriteFlip(Vector2 movementDirection)
    {
        if (Mathf.Abs(movementDirection.x) > _flipThreshold)
        {
            if (_sr.flipX != movementDirection.x < 0f)
            {
                _sr.flipX = movementDirection.x < 0f;
            }
        }
    }

    private void UpdateSpriteFlipTowardsPlayer()
    {
        if (_playerRef != null)
        {
            float directionToPlayer = _playerRef.transform.position.x - transform.position.x;

            if (Mathf.Abs(directionToPlayer) > _flipThreshold)
            {
                bool shouldFlipLeft = directionToPlayer < 0f;
                if (_sr.flipX != shouldFlipLeft)
                {
                    _sr.flipX = shouldFlipLeft;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null) return;

        if (collision.gameObject.CompareTag("Duck"))
        {
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            _rb.angularVelocity = 0f;
            Debug.Log("Duck Collision");
            DuckController duck = collision.gameObject.GetComponent<DuckController>();
            if (duck != null && duck.IsFollowing)
            {
                //send duck comand
                duck.Detach();
            }
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            if (_playerRef == null) GetPlayerReference(collision);

            if (_playerRef != null)
            {
                //hit player & shake camera
                Vector2 directionOfForce = (_playerRef.transform.position - transform.position).normalized;
                directionOfForce.y += 0.7f;
                _playerRef.GetKickDamage(_kickPower, directionOfForce);
                CameraController.Instance.Shake();
            }
            _rb.linearVelocityX = 0f;
            _isPlayerInView = true;
            SwitchToAggressive();
        }
    }


    private void GetPlayerReference(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerController>(out var plController))
        {
            _playerRef = plController;
        }
    }

    private void OnDrawGizmos()
    {
        if (groundPos != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(groundPos.position, groundSize);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundPos != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(groundPos.position, groundSize);
            Gizmos.color = Color.green;
            if (_sr != null)
            {
                Vector3 start = groundPos.position;
                Vector3 direction = _sr.flipX ? Vector3.left : Vector3.right;
                Vector3 end = groundPos.position + direction * _groundLength;
                Gizmos.DrawLine(start, end);
            }

            //Debug.Log($"<color=green>isGrounded: </color>{_isGrounded}");

        }
    }
}
