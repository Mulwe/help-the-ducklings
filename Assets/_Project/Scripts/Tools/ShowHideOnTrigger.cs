using System.Collections;
using UnityEngine;

public class ShowHideOnTrigger : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _boxCollider2D;
    [SerializeField] private SpriteRenderer _spriteRender;
    private WaitForFixedUpdate _WaitForFixedUpdate = new();
    private WaitForSeconds _wait200m = new(0.2f);

    private bool _startAnimation = false;
    private bool _fadeIn = false;
    private bool _fadeOut = false;
    private bool _playerIsNear = false;

    private Color baseColor;
    private Coroutine _delayAction = null;
    private Coroutine _animation = null;

    public void InterruptAnimation(bool hide)
    {
        if (_spriteRender != null)
        {
            if ((hide && _fadeIn) || (!hide && _fadeIn))
            {
                _spriteRender.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
            }
            else if (hide && _fadeOut || !hide && _fadeOut)
            {
                _spriteRender.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
            }
            else
                _spriteRender.color = baseColor;
        }
        _startAnimation = false;
        _fadeIn = false;
        _fadeOut = false;
    }

    public void ToogleOnFadeIn()
    {
        _startAnimation = true;
        _fadeIn = true;
        _fadeOut = false;
    }
    public void ToogleOnFadeOut()
    {
        _startAnimation = true;
        _fadeIn = false;
        _fadeOut = true;
    }

    private void Awake()
    {
        _spriteRender ??= GetComponent<SpriteRenderer>();
        if (_spriteRender != null)
        {
            baseColor = _spriteRender.color;
        }
        _boxCollider2D ??= GetComponent<BoxCollider2D>();
        SpriteIsActive(false);
        InterruptAnimation(true);
        _animation = StartCoroutine(FadeAnimation(1f));
        SpriteIsActive(true);
    }

    void SpriteIsActive(bool isActive)
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
            if (_delayAction != null)
            {
                StopCoroutine(_delayAction);
                _delayAction = null;
            }
            if (!_startAnimation && GetCurrentColorAlpha() < 1f && GetCurrentColorAlpha() != -1f)
            {
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
                _delayAction = StartCoroutine(DelayStartAction(() => ToogleOnFadeOut(), null, 1f));
            }
        }
    }

    private IEnumerator DelayStartAction(System.Action action)
    {
        yield return _wait200m;
        action?.Invoke();
        _delayAction = null;
    }

    private IEnumerator DelayStartAction(System.Action first, System.Action second, float delay)
    {
        first?.Invoke();
        yield return new WaitForSeconds(delay);
        second?.Invoke();
        _delayAction = null;
    }
}
