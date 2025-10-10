using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Deactivate / activate objects behind camera:")]
    [SerializeField] private bool _enableActivityMonitoring = true;
    [SerializeField] private float _intervalCheck = 0.2f;
    [SerializeField] private float _cullingDistance = 0f;

    public static GameManager Instance;

    private SpawnManager _sp;
    private ActivityManager _am;
    private List<DuckController> _ducks;
    private List<EnemyController> _enemies;
    private List<DuckController> _collected;

    private Coroutine _waitCameraInit = null;
    private Coroutine _startMonitor = null;

    private PlayerController _playerController;

    public void SetPlyerController(PlayerController playerController)
    {
        _playerController = playerController;
    }

    public List<int> GetDuckIndicesByCondition(System.Func<DuckController, bool> condition)
    {
        var indices = new List<int>();
        for (int i = 0; i < _ducks.Count; i++)
        {
            if (condition(_ducks[i]))
                indices.Add(i);
        }
        return indices;
    }

    public List<int> GetEnemyIndicesByCondition(System.Func<EnemyController, bool> condition)
    {
        var indices = new List<int>();
        for (int i = 0; i < _enemies.Count; i++)
        {
            if (condition(_enemies[i]))
                indices.Add(i);
        }
        return indices;
    }

    public void SetSpecificDuckActive(int index, bool active)
    {
        if (index >= 0 && index < _ducks.Count)
        {
            _am.ChangeDuckActivity(_ducks[index], active);
        }
    }

    public void SetSpecificEnemyActive(int index, bool active)
    {
        if (index >= 0 && index < _enemies.Count)
        {
            _am.ChangeEnemyActivity(_enemies[index], active);
        }
    }

    public int GetCollectedCount()
    {
        if (_collected != null) return _collected.Count;
        return 0;
    }

    private void EnableAllObjects(bool isActive)
    {
        _am.ChangeDucksActivity(_ducks, isActive);
        _am.ChangeEnemiesActivity(_enemies, isActive);
    }

    public void SetDucksActivity(bool isActive)
    {
        _am.ChangeDucksActivity(_ducks, isActive);
    }

    public void SetEnemiesActivity(bool isActive)
    {
        _am.ChangeEnemiesActivity(_enemies, isActive);
    }

    public int GetActiveDucksCount()
    {
        return _ducks.FindAll(duck => duck != null && duck.gameObject.activeSelf).Count;
    }

    public int GetActiveEnemiesCount()
    {
        return _enemies.FindAll(enemy => enemy != null && enemy.gameObject.activeSelf).Count;
    }

    public List<DuckController> GetDucks()
    {
        return _am.GetDucks<DuckController>();
    }

    public List<EnemyController> GetEnemies()
    {
        return _am.GetEnemies<EnemyController>();
    }

    public void TurnOffActivityMonitor()
    {
        _enableActivityMonitoring = false;
        _am?.StopMonitoring();
    }

    public void ChangeActivityMonitor(bool isActive)
    {
        _enableActivityMonitoring = isActive;
        if (!_enableActivityMonitoring)
            _am?.StopMonitoring();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _am = new ActivityManager(this);
            _sp = UnityEngine.Object.FindFirstObjectByType<SpawnManager>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void HandlePlayerReferenceRequest(MonoBehaviour sender)
    {
        if (sender != null && sender is CameraController cam)
        {
            if (_playerController != null && _playerController.gameObject.activeInHierarchy)
                cam.SetTarget(_playerController.gameObject);
        }
        // etc. send to sender reference
    }

    private void OnEnable()
    {
        ExitController.OnDuckReachedExit += HandleCollectedDuck;
        CameraController.PlayerReferenceRequested += HandlePlayerReferenceRequest;
        if (CameraController.Instance != null)
        {
            if (_startMonitor != null)
                StopCoroutine(_startMonitor);
            _startMonitor = StartCoroutine(SetupActivityMonitor());
        }
        else
            _waitCameraInit = StartCoroutine(WaitForCameraInit());
    }

    private void OnDisable()
    {
        ExitController.OnDuckReachedExit -= HandleCollectedDuck;
        CameraController.PlayerReferenceRequested -= HandlePlayerReferenceRequest;
        TurnOffActivityMonitor();
        _waitCameraInit = null;
        _startMonitor = null;
    }

    private void HandleCollectedDuck(GameObject obj)
    {
        if (obj.TryGetComponent<DuckController>(out DuckController collectedDuck))
        {
            DeleteContainedObjectFromList(collectedDuck, _ducks);
            DeactivateAndHide(collectedDuck.gameObject);
            AddObject(collectedDuck, ref _collected);
            //Debug.Log($"Player already collected <color=yellow>{_collected.Count}</color> ducks");
        }
    }

    private void DeactivateAndHide(GameObject obj)
    {
        obj.SetActive(false);
        Vector3 newPos = new(-99, -99, -99);
        obj.transform.position = newPos;
    }

    private IEnumerator SetupActivityMonitor()
    {
        if (CameraController.Instance != null && _am != null)
        {
            AddDucksAndEnemiesFromSceneManualy();
            if (_ducks.Count == 0 || _enemies.Count == 0)
            {
                if (!_sp.IsInit)
                    yield return new WaitForSeconds(0.1f);
                yield return GetDataFromSpawnManager();
            }
            if (_ducks.Count == 0)
                Debug.LogError("GetDataFromSpawnManager zero ducks");
            Debug.Log($"Initialized: <color=yellow>{_ducks.Count}</color> ducks, <color=red>{_enemies.Count}</color> enemies");

            // Init ActivityManager Settings
            _am.SetTargetCamera(CameraController.Instance.GetCamera());
            _am.SetCheckInterval(_intervalCheck);
            _am.SetCullingDistance(_cullingDistance);

            if (_enableActivityMonitoring)
            {
                _am.StartMonitoring();
            }
            else
                Debug.Log("ActivityManager disabled");
        }
        else
        {
            Debug.LogWarning("CameraController not found, ActivityManager disabled");
        }
        yield return null;
        _startMonitor = null;
    }

    private IEnumerator GetDataFromSpawnManager()
    {
        if (_sp != null && _am != null)
        {
            _ducks?.Clear();
            _enemies?.Clear();
            _am.CleanDucks();
            _am.CleanEnemies();
            for (int i = 0; i < 10; i++)
            {
                List<DuckController> ducks =
                    GetControllers<DuckController>(_sp, "Duck");
                List<EnemyController> enemies =
                    GetControllers<EnemyController>(_sp, "Enemy");

                if (ducks != null)
                {
                    _am.AddDucks<DuckController>(ducks.ToArray());
                    _ducks = GetDucks();
                }

                if (enemies != null)
                {
                    _am.AddEnemies<EnemyController>(enemies.ToArray());
                    _enemies = GetEnemies();
                }

                if ((_ducks != null || _enemies != null) && i > 1)
                    yield break;
                yield return null;
            }
        }
        else
            Debug.LogError("Failed to get data from SpawnManager");
        yield return null;
    }

    //TODO: если несколько спаунеров складывать общее количество
    // проход по всем
    // проход только по одному
    private List<T> GetControllers<T>(SpawnManager sp, string tag)
    {
        SpawnController spawnController = sp.GetSpawnController(tag);

        if (spawnController == null)
        {
            return null;
        }
        List<GameObject> list = spawnController.GetSpawnedObjects();
        List<T> returnList = new List<T>();
        if (list != null)
        {
            foreach (GameObject obj in list)
            {
                if (obj != null && obj.TryGetComponent<T>(out T controller))
                {
                    returnList.Add(controller);
                }
            }
            if (returnList.Count == list.Count)
                return returnList;
        }
        return null;
    }

    private IEnumerator WaitForCameraInit()
    {
        while (CameraController.Instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        while (CameraController.Instance.GetCamera() == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        // if player on scene send to camera
        if (_playerController != null)
            CameraController.Instance.SetTarget(_playerController.gameObject);
        if (_startMonitor != null)
            StopCoroutine(_startMonitor);
        _startMonitor = StartCoroutine(SetupActivityMonitor());
        _waitCameraInit = null;
    }

    public void RewriteDucksAndEnemies(DuckController[] ducks, EnemyController[] enemies)
    {
        _am.AddDucks(ducks);
        _am.AddEnemies(enemies);
        _ducks = GetDucks();
        _enemies = GetEnemies();
    }

    private void AddDucksAndEnemiesFromSceneManualy()
    {
        DuckController[] duckControllers = FindObjectsByType<DuckController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        EnemyController[] enemiesControllers = FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        _am.AddDucks(duckControllers);
        _am.AddEnemies(enemiesControllers);
        _ducks = GetDucks();
        _enemies = GetEnemies();
    }

    private bool AddObject<T>(T component, ref List<T> list)
    {
        if (component == null) return false;
        list ??= new List<T>();
        if (!ContainsComponent(component, list))
        {
            list.Add(component);
            return true;
        }
        return false;
    }

    // NOTE:
    // - "==" cannot be used for generic T, since not all types overload it.
    // - x.Equals(component) also cannot guarantee correct comparison if x is null.
    // - Therefore, we use static Equals(x, component), which safely handles null
    //   and works for both UnityEngine.Object (with custom null semantics)
    //   and regular .NET types.
    private bool ContainsComponent<T>(T component, List<T> list)
    {
        return list.Any(x => Equals(x, component));
    }

    private bool DeleteContainedObjectFromList<T>(T toDelete, List<T> fromList)
    {
        if (toDelete == null || fromList == null) return false;
        if (!ContainsComponent(toDelete, fromList))
        {
            fromList.Remove(toDelete);
            return true;
        }
        return false;
    }

    private void OnDestroy()
    {
        _am?.OnDispose();
        _collected?.Clear();
        _collected = null;
    }
}