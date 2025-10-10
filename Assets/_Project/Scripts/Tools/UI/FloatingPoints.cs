using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingPoints : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target;
    [SerializeField] private float _animationDuration = 0.5f;

    public Vector3 worldOffset = Vector3.zero;

    private TextMeshProUGUI _popupText;
    private RectTransform _popupRect;
    private Camera _mainCamera;

    private Coroutine _trackingCoroutine;
    private Coroutine _singleAnimation;


    private void Awake()
    {
        _popupText = GetComponent<TextMeshProUGUI>();
        _popupRect = _popupText.rectTransform;
        _popupText.enabled = false;
    }

    void Start()
    {
        _mainCamera = Camera.main;
        GetTragetOffset();

    }

    private void OnEnable()
    {
        ExitController.OnDucksCollected += OnDuckCollected;
    }

    private void OnDisable()
    {
        ExitController.OnDucksCollected -= OnDuckCollected;
    }

    private void UpdateText(int points)
    {
        if (_popupText != null)
        {
            _popupText.text = $"+{points}";
        }

    }

    private void OnDuckCollected(int points)
    {
        UpdateText(points);
        StartAnimation();
    }

    private void StartAnimation()
    {
        if (_singleAnimation != null)
        {
            StopCoroutine(_singleAnimation);
        }
        _singleAnimation = StartCoroutine(StartAnimatePoints());
    }

    private void GetTragetOffset()
    {
        if (_target == null)
        {
            _popupText.color = UnityEngine.Color.gray;
            worldOffset.y = 2f;
            return;
        }
        if (_target.TryGetComponent<SpriteRenderer>(out var sr))
        {
            worldOffset.y = sr.bounds.extents.y;
        }
        else if (_target.TryGetComponent<BoxCollider2D>(out var bc))
        {

            //worldOffset.y = bc.bounds.extents.y;
            worldOffset.y = bc.bounds.size.y;
            // Debug.Log($"worldOffset BoxCollider2D {worldOffset.y}");
        }
    }

    public IEnumerator StartAnimatePoints()
    {
        yield return new WaitForSeconds(0.2f);
        Vector3 screenPos = GetScreenPosition();
        if (IsVisible(screenPos))
        {
            _popupText.enabled = true;
            yield return AnimateToFinalPos(GetScreenPosition());
            yield return new WaitForSeconds(0.1f);
            _popupText.enabled = false;
        }
        else
        {
            _popupText.enabled = false;
        }
        _singleAnimation = null;
        yield return null;
    }

    public Coroutine StartTracking(ref Coroutine trackingCoroutine)
    {
        if (trackingCoroutine != null)
            StopCoroutine(trackingCoroutine);
        trackingCoroutine = StartCoroutine(TrackAndAnimate());
        return trackingCoroutine;
    }


    public void StopTracking()
    {
        if (_trackingCoroutine != null)
        {
            StopCoroutine(_trackingCoroutine);
            _trackingCoroutine = null;
        }
    }

    private Vector3 GetScreenPosition()
    {
        Vector3 worldPos = _target.position;
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(worldPos);
        return screenPos;
    }



    private IEnumerator TrackAndAnimate()
    {
        while (_target != null)
        {
            Vector3 screenPos = GetScreenPosition();

            if (IsVisible(screenPos))
            {
                _popupText.enabled = true;
                yield return StartCoroutine(AnimateToFinalPos(screenPos));
            }
            else
            {
                _popupText.enabled = false;
                yield return null;
            }
        }
        _trackingCoroutine = null;
    }

    private IEnumerator AnimateToFinalPos(Vector3 startPos)
    {
        Vector3 finalPos = _mainCamera.WorldToScreenPoint(_target.position + worldOffset);
        float elapsed = 0f;

        while (elapsed < _animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / _animationDuration;

            _popupRect.position = Vector3.Lerp(startPos, finalPos, progress);

            yield return null;
        }
        _popupRect.position = finalPos;
    }

    private bool IsVisible(Vector3 screenPos)
    {
        return (screenPos.x >= 0 &&
                screenPos.x <= Screen.width &&
                screenPos.y >= 0 &&
                screenPos.y <= Screen.height);
    }

    private void OnDestroy()
    {
        StopTracking();
    }

}
