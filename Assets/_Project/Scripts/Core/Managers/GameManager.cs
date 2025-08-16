using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Deactivate / activate objects behind camera:")]
    [SerializeField] private bool _enableActivityMonitoring = true;
    [SerializeField] private float _checkInterval = 0.2f;
    [SerializeField] private float _cullingDistance = 0f;

    public static GameManager Instance;
    private ActivityManager _am;
    private List<DuckController> _ducks;
    private List<EnemyController> _enemies;
    private List<DuckController> _collected;

    private Coroutine _waitCameraInit = null;


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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        ExitController.OnDuckReachedExit += HandleCollectedDuck;
        if (CameraController.Instance != null)
        {
            SetupActivityMonitor();
        }
        else
            _waitCameraInit = StartCoroutine(WaitForCameraInit());
    }

    private void HandleCollectedDuck(GameObject obj)
    {
        if (obj.TryGetComponent<DuckController>(out var collectedDuck))
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

    private void SetupActivityMonitor()
    {
        if (CameraController.Instance != null && _am != null)
        {
            InitLists();
            _am.SetTargetCamera(CameraController.Instance.GetCamera());
            _am.SetCheckInterval(_checkInterval);
            _am.SetCullingDistance(_cullingDistance);
            if (_enableActivityMonitoring)
            {
                //Debug.Log("ActivityManager started");
                _am.StartMonitoring();
            }
            else
                Debug.Log("ActivityManager disabled");
        }
        else
        {
            Debug.LogWarning("CameraController not found, ActivityManager disabled");
        }
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
        SetupActivityMonitor();
        _waitCameraInit = null;
    }

    private void InitLists()
    {
        DuckController[] duckControllers = FindObjectsByType<DuckController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        EnemyController[] enemiesControllers = FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        _am.AddDucks(duckControllers);
        _am.AddEnemies(enemiesControllers);
        _ducks = GetDucks();
        _enemies = GetEnemies();
        Debug.Log($"Initialized: <color=yellow>{_ducks.Count}</color> ducks, <color=red>{_enemies.Count}</color> enemies");
    }



    private bool AddObject<T>(T component, ref List<T> list) where T : Object
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

    private bool ContainsComponent<T>(T component, List<T> list) where T : Object
    {
        return list.Any(x => x == component);
    }

    private bool DeleteContainedObjectFromList<T>(T toDelete, List<T> fromList) where T : Object
    {
        if (toDelete == null || fromList == null) return false;
        if (!ContainsComponent(toDelete, fromList))
        {
            fromList.Remove(toDelete);
            return true;
        }
        return false;
    }

    private int GetCollectedCount()
    {
        if (_collected != null) return _collected.Count;
        return 0;
    }

    private void OnDestroy()
    {
        _am?.OnDispose();
        _collected?.Clear();
        _collected = null;
    }

    private void OnDisable()
    {
        ExitController.OnDuckReachedExit -= HandleCollectedDuck;
        TurnOffActivityMonitor();
    }
}
