using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    private Camera _camera;
    private Vector3 _originalPos;
    private Coroutine _shakeRoutine;

    private bool _isInit = false;

    [Header("Objects for following")]
    [SerializeField] private GameObject _target;
    [Range(0.0f, 1.0f)] private float _smoothness = 0.5f;
    [SerializeField] private float _offset = 2.0f;
    [SerializeField] private float _speed = 8.0f;


    private readonly float _z = -10.0f;
    private Vector3 _cameraPosition;
    private Vector3 _targetPos => _target.transform.position; //shorthan
    private bool _isFollowingPlayer = true;

    public void StopFollowingTarget() => _isFollowingPlayer = false;

    public void KeepFollowingTarget() => _isFollowingPlayer = true;


    public void Shake(float duration = 0.2f, float magnitude = 0.1f)
    {
        if (_isInit)
        {
            if (_shakeRoutine != null)
                StopCoroutine(_shakeRoutine);

            _shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
        }
    }

    public Camera GetCamera() => _camera;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _camera = GetComponent<Camera>();
        }
        else
            Destroy(gameObject);

        _originalPos = transform.localPosition;
        _isInit = true;

        if (_target != null)
        {
            _isFollowingPlayer = true;
            _cameraPosition = new Vector3(_targetPos.x, _targetPos.y, _z);
            this.transform.position = _target.transform.position;
        }
    }

    private void OnEnable()
    {
        PlayerController.CameraLookDown += OnPlayerLookDown;
        PlayerController.CancelCameraLookDown += OnPlayerLookDefault;
    }
    private void OnDisable()
    {
        PlayerController.CameraLookDown -= OnPlayerLookDown;
        PlayerController.CancelCameraLookDown -= OnPlayerLookDefault;
    }


    private void OnPlayerLookDown()
    {
        _offset = 0f;
    }
    private void OnPlayerLookDefault()
    {
        _offset = 2f;
    }

    private void Update()
    {
        if (_target != null && _isFollowingPlayer == true)
            CameraMove(_target);
    }

    private void CameraMove(GameObject target)
    {
        float t = Mathf.Clamp01(_speed * _smoothness * Time.deltaTime);
        Vector3 TargetPosition = new Vector3(target.transform.position.x,
                                          target.transform.position.y + _offset,
                                          target.transform.position.z);
        _cameraPosition = Vector3.Lerp(_cameraPosition, TargetPosition, t);
        _cameraPosition.z = _z;
        this.transform.position = _cameraPosition;
    }


    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        _isFollowingPlayer = false;
        Vector3 basePosition = _cameraPosition;
        while (elapsed < duration)
        {
            /*
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            */
            Vector2 offset = Random.insideUnitCircle * magnitude;

            transform.position = basePosition + new Vector3(offset.x, offset.y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = basePosition;
        _shakeRoutine = null;
        _isFollowingPlayer = true;
    }

}


