using System.Collections;
using UnityEngine;

public class ShowHideOnTrigger : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _boxCollider2D;
    [SerializeField] private SpriteRenderer _spriteRender;

    [Tooltip("Fade in/out anumation duaration:")]
    [SerializeField] private float _fadeDuration = 1f;
    private WaitForFixedUpdate _WaitForFixedUpdate = new();
    private WaitForSeconds _wait200m = new(0.2f);

    private bool _startAnimation = false;
    private bool _fadeOut = false;
    private bool _playerIsNear = false;


    private Color _baseColor;
    private Coroutine _delayAction = null;
    private Coroutine _animation = null;

    public bool IsFadedOut => _fadeOut;
    public bool IsAnimationStarted => _startAnimation;

    public void ToogleOnFadeIn()
    {
        _startAnimation = true;
        _fadeOut = false;
    }

    public void ToogleOnFadeOut()
    {
        _startAnimation = true;
        _fadeOut = true;
    }

    private void Awake()
    {
        _spriteRender ??= GetComponent<SpriteRenderer>();
        if (_spriteRender != null)
        {
            _baseColor = _spriteRender.color;
        }
        _boxCollider2D ??= GetComponent<BoxCollider2D>();
        SpriteIsActive(false);
    }

    private void OnEnable()
    {
        if (_animation != null)
        {
            StopCoroutine(_animation);
            _animation = null;
        }
        _animation = StartCoroutine(FadeAnimation(_fadeDuration));
        SpriteIsActive(true);
    }

    private void SpriteIsActive(bool isActive)
    {
        if (_spriteRender != null)
            _spriteRender.enabled = isActive;
    }

    private IEnumerator FadeAnimation(float duration)
    {
        _startAnimation = true;
        UnityEngine.Color baseColor = _spriteRender.color;

        while (true)
        {
            float t = 0;
            while (t < duration && _startAnimation)
            {
                t += Time.deltaTime;
                float a = _fadeOut ? Mathf.Clamp01(1f - t / duration) : Mathf.Clamp01(t / duration);
                _spriteRender.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
                yield return _WaitForFixedUpdate;
            }
            if (_startAnimation)
            {
                _spriteRender.color = _fadeOut ? new Color(baseColor.r, baseColor.g, baseColor.b, 0f) :
                new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
                _startAnimation = false;
            }
            if (_playerIsNear && !IsInvoking(nameof(ResetToHidden)))
            {
                Invoke(nameof(ResetToHidden), 2f);
            }
            yield return _WaitForFixedUpdate;
        }
    }

    private float GetCurrentColorAlpha()
    {
        if (_spriteRender == null)
            return -1f;
        return _spriteRender.color.a;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        if (collision != null && collision.CompareTag("Player"))
        {
            _playerIsNear = true;
            CancelInvoke(nameof(ResetToHidden));
            if (collision != null && collision.CompareTag("Player") && _spriteRender != null)
            {
                if (_delayAction != null)
                {
                    StopCoroutine(_delayAction);
                    _delayAction = null;
                }
                if (!_startAnimation && GetCurrentColorAlpha() < 1f)
                    ToogleOnFadeIn();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("Player"))
        {
            if (!_startAnimation && _delayAction == null)
            {
                _playerIsNear = false;
                CancelInvoke(nameof(ResetToHidden));
                if (gameObject.activeInHierarchy)
                    _delayAction = StartCoroutine(DelayStartAction(() => ToogleOnFadeOut(), null, 1f));
            }
        }
    }
    private IEnumerator DelayStartAction(System.Action first, System.Action second, float delay)
    {
        first?.Invoke();
        yield return new WaitForSeconds(delay);
        second?.Invoke();
        _delayAction = null;
    }

    private void ResetToHidden()
    {
        if (_delayAction == null && gameObject.activeInHierarchy)
        {
            _playerIsNear = false;
            _delayAction = StartCoroutine(DelayStartAction(() => ToogleOnFadeOut(), null, 0f));
        }
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(ResetToHidden));
        if (_animation != null)
        {
            StopCoroutine(_animation);
            _animation = null;
        }

        if (_delayAction != null)
        {
            StopCoroutine(_delayAction);
            _delayAction = null;
        }
    }

}

/**
private IEnumerator DelayStartAction(System.Action action)
{
    yield return _wait200m;
    action?.Invoke();
    _delayAction = null;
}
**/
