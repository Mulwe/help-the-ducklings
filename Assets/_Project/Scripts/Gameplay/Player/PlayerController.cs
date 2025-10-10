using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private bool isFacingRight = true; // false если по default повернут в влево

    [Header("Moving")]
    [SerializeField] private float _speed = 8.0f;
    private Vector2 _direction = Vector2.zero;
    private bool _playerControlEnabled = true;

    [Header("Jumping")]
    public float JumpPower = 10f;
    public int maxJumps = 2;
    private int _jumpsRemaining;

    [Header("Trigger layers:")]
    [SerializeField] private LayerMask _groundLayer;

    public Transform groundPos;
    public Vector2 groundSize = new Vector2(0.5f, 0.05f);
    private bool _isGrounded;

    [Header("Gravity")]
    public float FallMultiplier = 2.5f;
    public float LowJumpMultiplier = 2f;
    private readonly float _originalGravityScale = 3f; //3 - 5f
    public float MaxFallSpeed = 18f;

    [Header("WallCheck")]
    [SerializeField] private LayerMask _wallCheckLayer;

    public Transform wallCheckPos;
    public Vector2 _wallCheckSize = new Vector2(0.5f, 0.05f);

    [Header("WallSlide")]
    public float wallSlideSpeed = 2f;
    private bool isWallSliding;
    private bool isWallJumping;
    private float wallJumpDirection;
    private float wallJumpTime = 0.5f;
    private float wallJumpTimer;
    public Vector2 wallJumpPower = new Vector2(5f, 10f);

    public static event Action CancelCameraLookDown, CameraLookDown;

    private Coroutine _playerControlsCamera = null;
    private PlayerAudio _playerAudio;

    public void OnJump(InputAction.CallbackContext context)
    {
        if (_rb == null) return;
        if (context.performed && _jumpsRemaining > 0)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, JumpPower);
            _jumpsRemaining--;
            _playerAudio.PlayJumpSounds(transform, 1.0f);
        }
        else if (context.canceled && _jumpsRemaining > 0)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * 0.5f);
            _jumpsRemaining--;
        }

        //Wall jumping
        if (context.performed && wallJumpTimer > 0f)
        {
            isWallJumping = true;
            _rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            wallJumpTimer = 0;

            //Force Flip
            if (transform.localScale.x != wallJumpDirection)
            {
                MakeFlip();
            }
            // Запланировать автоматическую отмену прыжка от стены
            // через wallJumpTime + 0.1 секунды
            Invoke(nameof(CancelWallJump), wallJumpTime + 0.1f);
        }
    }

    public float GetPlayerSpeed()
    {
        return _speed;
    }

    public bool FacingRight()
    {
        return isFacingRight;
    }

    public void OnMoving(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        _direction.x = input.x;

        if (input.y < 0f && input.x == 0f)
            _playerControlsCamera ??= StartCoroutine(ResetCountDown(3f, CameraLookDown, CancelCameraLookDown));
    }

    private IEnumerator ResetCountDown(float delay, System.Action setTrue, System.Action setFalse)
    {
        float timer = delay;
        setTrue?.Invoke();

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        setFalse?.Invoke();
        _playerControlsCamera = null;
    }

    private void Awake()
    {
        _ = isWallSliding;
        _jumpsRemaining = maxJumps;
        _groundLayer = LayerMask.GetMask("Ground");
        _rb = GetComponent<Rigidbody2D>();

        if (_rb != null)
            DefaultGravity();

        _playerControlEnabled = true;
        _playerAudio = this.GetComponent<PlayerAudio>();
    }

    private void Update()
    {
        if (_playerControlEnabled)
        {
            IsOnGround();
            Gravity();
            ProcessWallSlide();
            ProcessWallJump();

            if (!isWallJumping)
            {
                ApplyMovement();
                Flip();
            }
        }
    }

    private void ApplyMovement()
    {
        _rb.linearVelocity = new Vector2(_direction.x * _speed, _rb.linearVelocityY);
    }

    private void Gravity()
    {
        if (_rb.linearVelocity.y < 0)
        {
            _rb.gravityScale = _originalGravityScale * FallMultiplier;
            _rb.linearVelocity = new Vector2(
                                        _rb.linearVelocity.x,
                                        Mathf.Max(_rb.linearVelocity.y, -MaxFallSpeed));
        }
        else
        {
            DefaultGravity();
        }
    }

    private bool OnWall()
    {
        return Physics2D.OverlapBox(wallCheckPos.position, _wallCheckSize, 0, _wallCheckLayer);
    }

    private void ProcessWallSlide()
    {
        if (!_isGrounded && OnWall() && _direction.x != 0)
        {
            isWallSliding = true;
            _rb.linearVelocity = new Vector2(_rb.linearVelocityX, Mathf.Max(_rb.linearVelocityY, -wallSlideSpeed));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void ProcessWallJump()
    {
        if (isWallJumping)
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpTimer = wallJumpTime;

            // Если нужно отменить уже запланированный вызов раньше
            CancelInvoke(nameof(CancelWallJump));
            //CancelInvoke("CancelWallJump");
        }
        else if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }
    }

    private void CancelWallJump()
    {
        isWallJumping = false;
    }

    private void IsOnGround()
    {
        if (Physics2D.OverlapBox(groundPos.position, groundSize, 0, _groundLayer))
        {
            _isGrounded = true;
            _jumpsRemaining = maxJumps;
            return;
        }
        _isGrounded = false;
    }

    private void Flip()
    {
        if (isFacingRight && _direction.x < 0 || !isFacingRight && _direction.x > 0)
            MakeFlip();
    }

    private void MakeFlip()
    {
        isFacingRight = !isFacingRight;

        Vector3 ls = transform.localScale;
        ls.x *= -1f;
        transform.localScale = ls;
    }

    private void DefaultGravity()
    {
        _rb.gravityScale = _originalGravityScale;
    }

    public void GetKickDamage(float forceStrength, Vector2 dir)
    {
        Vector2 direction = _rb.linearVelocity.normalized;

        if (dir.x < 0)
            direction = -direction;

        if (_playerControlEnabled)
            StartCoroutine(DisableControlsTemporarily(0.3f));
        _rb.AddForce(dir * forceStrength, ForceMode2D.Impulse);
        _playerAudio.PlayDamageSounds(transform, 1.0f);
    }

    private IEnumerator DisableControlsTemporarily(float duration)
    {
        _playerControlEnabled = false;
        yield return new WaitForSeconds(duration);
        _playerControlEnabled = true;
    }

    private void OnDrawGizmos()
    {
        if (groundPos != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(groundPos.position, groundSize);
        }

        if (wallCheckPos != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(wallCheckPos.position, _wallCheckSize);
        }
    }
}