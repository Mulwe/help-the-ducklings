using System;
using System.Collections;
using System.Net.NetworkInformation;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    private Camera _camera;
    private bool _isInit = false;

    [Header("Objects for following")]
    [SerializeField] private GameObject _target;

    [SerializeField] private float _offset = 2.0f;
    private readonly float _z = -10.0f;
    private bool _cameraFreeToControl = true;

    [Tooltip("Cinemachine")]
    private CinemachineBrain _brain;

    private CinemachineCamera _currentCinemachineCamera;
    private CinemachineCamera _transitionCamera;
    private CinemachineFollow _cinemachineFollow;
    private Vector3 _baseFollowOffset = new Vector3(0, 0, -10.0f);
    public bool IsCameraUnderControl => !_cameraFreeToControl;

    private Coroutine _coShaking;
    private Coroutine _coCurrentCameraInit;

    public static Action<MonoBehaviour> PlayerReferenceRequested;

    public Camera GetCamera() => _camera;

    public void SetCameraControl(bool isCameraControlled)
    {
        _cameraFreeToControl = !isCameraControlled;
    }

    public bool IsCameraControlled()
    {
        return (!_cameraFreeToControl);
    }

    public void StopFollowingTarget()
    {
        _currentCinemachineCamera.Follow = null;
    }

    public void KeepFollowingTarget()
    {
        if (_target == null)
            PlayerReferenceRequested?.Invoke(this);

        if (_target != null)
            SetCinemachineFollowTarget(_target.transform);
        else
            Debug.Log($"{this.name}: Target not found. Can't keep following target");
    }

    public void SetTarget(GameObject newTarget)
    {
        _target = newTarget;
    }

    public void Shake()
    {
        if (_isInit)
            ShakeCinemachineCamera(0.5f, 4f, 4f);
    }

    public void OnSceneLoaded()
    {
        //every loaded scene => parse currentActiveCinemachineCamera and its components
        _coCurrentCameraInit ??= StartCoroutine(RepeatAction(
            () => _currentCinemachineCamera == null,
            () =>
            {
                //repeatingAction
                _currentCinemachineCamera = GetAndUpdateCurrentCamera();
            },
            () =>
            {
                //postAction
                //init Camera componets + target
                InitCameraComponents();

                //Initialize _cinemachineFollow and followOffset
                if (!InitializeCinemachineFollowComponentAndOffset(_currentCinemachineCamera, out _cinemachineFollow, out _baseFollowOffset))
                    Debug.LogError("Failed to Initialize Cinemachine FollowComponent And FollowOffset");

                if (GetCurrentFollowTarget() != null)
                    _target = GetCurrentFollowTarget().gameObject;
                else
                    Debug.LogError("Failed to GetCurrentFollowTarget");

                _coCurrentCameraInit = null;
            },
            1f
            ));
    }

    public void OnSceneChanged()
    {
        _target = null;
        _currentCinemachineCamera = null;
        _cinemachineFollow = null;
        _transitionCamera = null;
        _cameraFreeToControl = true;

        if (_coCurrentCameraInit != null)
        {
            StopCoroutine(_coCurrentCameraInit);
            _coCurrentCameraInit = null;
        }
    }

    public bool IsObjectVisible2D(GameObject target)
    {
        if (_cinemachineFollow == null || _currentCinemachineCamera == null || _target == null)
            InitCameraComponents();

        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(_camera);
        Renderer renderer = target.GetComponent<Renderer>();

        if (renderer != null)
        {
            Bounds targetBounds = renderer.bounds;
            if (GeometryUtility.TestPlanesAABB(frustumPlanes, targetBounds))
                return true;
        }
        return false;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitSetups();
        }
        else
            Destroy(gameObject);
    }

    private void SetCinemachineFollowTarget(Transform newTarget)
    {
        if (_brain != null)
        {
            if (_currentCinemachineCamera == null)
                _currentCinemachineCamera = _brain.ActiveVirtualCamera as CinemachineCamera;

            if (_currentCinemachineCamera != null)
            {
                _currentCinemachineCamera.Follow = newTarget;
                _target = newTarget.gameObject;
                Debug.Log($"{this.name}: Camera target changed");
            }
        }
    }

    private Transform GetCurrentFollowTarget()
    {
        if (_brain != null)
        {
            _currentCinemachineCamera = _brain.ActiveVirtualCamera as CinemachineCamera;
            if (_currentCinemachineCamera != null)
                return _currentCinemachineCamera.Follow;
        }
        return null;
    }

    private void InitSetups()
    {
        _camera = GetComponent<Camera>();
        _brain = GetComponent<CinemachineBrain>();
        _currentCinemachineCamera = null;
        _coShaking = null;

        if (_camera != null)
            _camera.backgroundColor = Color.black;

        if (_camera == null)
        {
            _camera = this.gameObject.AddComponent<Camera>();
            _camera.transform.position = new Vector3(0f, 0f, -10f);
            _camera.orthographic = true;
            _camera.orthographicSize = 5f;
        }

        if (_brain == null)
        {
            _brain = this.gameObject.AddComponent<CinemachineBrain>();
            _brain.UpdateMethod = CinemachineBrain.UpdateMethods.LateUpdate;
            _brain.BlendUpdateMethod = CinemachineBrain.BrainUpdateMethods.LateUpdate;
            _brain.DefaultBlend.Style = CinemachineBlendDefinition.Styles.EaseInOut;
        }

        if (_camera != null && _brain != null)
            _isInit = true;
    }

    private void OnEnable()
    {
        PlayerController.CameraLookDown += OnPlayerLookDown;
        PlayerController.CancelCameraLookDown += OnPlayerLookDefault;

        SceneLoader.OnSceneLoaded += OnSceneLoaded;
        SceneLoader.OnSceneUnloaded += OnSceneChanged;
    }

    private void OnDisable()
    {
        PlayerController.CameraLookDown -= OnPlayerLookDown;
        PlayerController.CancelCameraLookDown -= OnPlayerLookDefault;

        SceneLoader.OnSceneLoaded -= OnSceneLoaded;
        SceneLoader.OnSceneUnloaded -= OnSceneChanged;
    }

    private IEnumerator RepeatAction(Func<bool> condition, System.Action repeatAction, System.Action postAction, float duration)
    {
        WaitForFixedUpdate wait = new WaitForFixedUpdate();
        float t = 0f;
        if (condition())
        {
            while (condition() && t < duration)
            {
                t += Time.fixedDeltaTime;
                repeatAction?.Invoke();
                yield return wait;
            }
        }
        postAction?.Invoke();
    }

    private void SetCurrentCameraPriority(CinemachineCamera newCamera, int priority)
    {
        if (newCamera != null)
        {
            _currentCinemachineCamera = newCamera;
            _currentCinemachineCamera.Priority = priority;
        }
    }

    //Create new camera for smooth transition
    private void OnPlayerLookDown()
    {
        if (_cinemachineFollow == null || _currentCinemachineCamera == null || _target == null)
            InitCameraComponents();

        if (_cameraFreeToControl)
        {
            if (_target != null)
            {
                Vector3 newOffset = new(_baseFollowOffset.x, _baseFollowOffset.y - _offset, _baseFollowOffset.z);
                ActivateTransitionCamera(newOffset);
                _cameraFreeToControl = false;
            }
        }
    }

    private void ActivateTransitionCamera(Vector3 newOffset)
    {
        if (_transitionCamera == null)
            _transitionCamera = GetOrAddCamera(_target);

        if (_transitionCamera != null)
        {
            _currentCinemachineCamera = GetAndUpdateCurrentCamera();

            //_transitionCamera priority should be higher then current
            _transitionCamera.Priority = _currentCinemachineCamera.Priority + 1;
            _transitionCamera.Prioritize();
            _transitionCamera.enabled = true;
        }
        else
            Debug.Log("ActivateTransitionCamera failed");
    }

    private void DeactivateTransitionCamera()
    {
        if (_transitionCamera == null)
            _transitionCamera = GetOrAddCamera(_target);

        _transitionCamera.enabled = false;

        // ‚ÂÌÛÚ¸Òˇ Í ·‡ÁÓ‚ÓÈ Í‡ÏÂÂ
        _currentCinemachineCamera.enabled = true;
        _currentCinemachineCamera.Prioritize();
    }

    private void OnPlayerLookDefault()
    {
        DeactivateTransitionCamera();
        _cameraFreeToControl = true;
        //Debug.Log("Camera: Look Default");
    }

    private CinemachineCamera GetAndUpdateCurrentCamera()
    {
        if (_brain != null)
        {
            _currentCinemachineCamera = _brain.ActiveVirtualCamera as CinemachineCamera;
            if (_currentCinemachineCamera != null)
                return _currentCinemachineCamera;
        }
        return null;
    }

    private bool ChangeCurrentFollowTarget(Transform target)
    {
        if (_brain != null)
        {
            _currentCinemachineCamera = _brain.ActiveVirtualCamera as CinemachineCamera;
            if (_currentCinemachineCamera != null)
            {
                _currentCinemachineCamera.Follow = target;
                return true;
            }
        }
        return false;
    }

    private CinemachineCamera GetOrAddCamera(GameObject target)
    {
        if (target != null)
        {
            CinemachineCamera transitionCamera = target.GetComponentInChildren<CinemachineCamera>();
            GameObject objCamera;
            //none of created cameras
            if (transitionCamera == null)
            {
                objCamera = new GameObject("TransitionCamera");
                objCamera.transform.parent = target.transform;
                objCamera.transform.SetParent(target.transform, false); // reset position
                /*objCamera.transform.localPosition = new Vector3(0, -_offset, 0);     //localPosition ÍÓÓ‰ËÌ‡Ú˚ ÓÚÌÓÒËÚÂÎ¸ÌÓ Ó‰ËÚÂÎˇ*/
                transitionCamera = objCamera.AddComponent<CinemachineCamera>();
                if (_currentCinemachineCamera != null)
                {
                    CopyLensSettings(transitionCamera, _currentCinemachineCamera.Lens);
                }
            }
            if (transitionCamera != null)
            {
                transitionCamera.enabled = false;
                transitionCamera.Priority = -1;
                transitionCamera.Follow = target.transform;
                CinemachineFollow cinemachineFollow = transitionCamera.gameObject.AddComponent<CinemachineFollow>();
                if (cinemachineFollow != null)
                {
                    cinemachineFollow.FollowOffset = new Vector3(0, -_offset, _z);
                }
                return transitionCamera;
            }
        }
        return null;
    }

    private void CopyLensSettings(CinemachineCamera cam, LensSettings newLensSettings)
    {
        if (cam != null)
        {
            cam.Lens.OrthographicSize = newLensSettings.OrthographicSize;
            cam.Lens.NearClipPlane = newLensSettings.NearClipPlane;
            cam.Lens.FarClipPlane = newLensSettings.FarClipPlane;
            cam.Lens.Dutch = newLensSettings.Dutch;
        }
    }

    private void ShakeCinemachineCamera(float shakeDuration, float amplitudeGain, float frequencyGain)
    {
        _currentCinemachineCamera = _brain.ActiveVirtualCamera as CinemachineCamera;
        if (_currentCinemachineCamera != null && _coShaking == null)
        {
            if (_currentCinemachineCamera.TryGetComponent(out CinemachineBasicMultiChannelPerlin noise))
            {
                //Debug.Log("Shake Camera");
                noise.AmplitudeGain = amplitudeGain;
                noise.FrequencyGain = frequencyGain;
                if (_coShaking != null)
                {
                    StopCoroutine(_coShaking);
                }
                _coShaking = StartCoroutine(
                            DelayedAction(() =>
                            {
                                noise.AmplitudeGain = 0f;
                                noise.FrequencyGain = 0f;
                                _coShaking = null;
                            },
                            shakeDuration));
            }
        }
    }

    private IEnumerator DelayedAction(System.Action delayedAction, float delay)
    {
        yield return new WaitForSeconds(delay);
        delayedAction?.Invoke();
    }

    private IEnumerator DelayedAction(System.Action preAction, System.Action afterAction, float delay)
    {
        preAction?.Invoke();
        yield return new WaitForSeconds(delay);
        afterAction?.Invoke();
    }

    private void InitCameraComponents()
    {
        if (_currentCinemachineCamera == null && _brain != null)
            _currentCinemachineCamera = _brain.ActiveVirtualCamera as CinemachineCamera;

        if (_cinemachineFollow == null && _currentCinemachineCamera != null)
            _cinemachineFollow = _currentCinemachineCamera.GetComponent<CinemachineFollow>();
        if (_target == null)
        {
            Transform newTarget = GetCurrentFollowTarget();
            if (newTarget != null)
                ChangeCurrentFollowTarget(newTarget);
        }
    }

    private bool InitializeCinemachineFollowComponentAndOffset(CinemachineCamera cinemachine—amera, out CinemachineFollow cinemachineFollow, out Vector3 followOffset)
    {
        if (cinemachine—amera != null)
        {
            if (cinemachine—amera.TryGetComponent<CinemachineFollow>(out CinemachineFollow tmp))
            {
                cinemachineFollow = tmp;
                followOffset = tmp.FollowOffset;
                return true;
            }
        }
        cinemachineFollow = null;
        followOffset = Vector3.zero;
        return false;
    }

    private bool GetCinemachineFollowOffset(CinemachineCamera cinemachine—amera, out Vector3 followOffset)
    {
        if (cinemachine—amera != null)
        {
            if (cinemachine—amera.TryGetComponent<CinemachineFollow>(out CinemachineFollow tmp))
            {
                followOffset = tmp.FollowOffset;
                return true;
            }
        }
        followOffset = Vector3.zero;
        return false;
    }
}