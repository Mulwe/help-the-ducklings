using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    [Tooltip("Prefab to spawn:")]
    [SerializeField] private GameObject _prefab;

    [Tooltip("Position to spawn:")]
    [SerializeField] private Transform[] _spots;

    [Tooltip("Amount:")]
    [SerializeField] private int _amount;

    private ObjectFabric _objFabric;
    private string _name;

    public List<GameObject> GetSpawnedObjects()
    {
        if (_objFabric != null)
            return _objFabric.GetObjectsPool();
        return null;
    }

    public string GetName()
    {
        return _name;
    }

    public string GetPrefabName()
    {
        if (_prefab != null)
            return _prefab.name;
        else
            return null;
    }

    public GameObject GetSpawnedPrefab()
    {
        if (_prefab != null)
            return _prefab;
        else
            return null;
    }

    public bool IsThisObjectFromPool(GameObject obj)
    {
        if (_objFabric != null)
        {
            List<GameObject> list = _objFabric.GetObjectsPool();
            if (list != null && list.Count > 0 && list.Contains(obj))
            {
                return true;
            }
        }
        else
            Debug.LogWarning($"{this.name}: _objFabric not init");
        return false;
    }

    private void IsOutOfBounds(GameObject spawnedObject)
    {
        if (spawnedObject != null && spawnedObject.gameObject != null && spawnedObject.gameObject.activeInHierarchy)
        {
            if (IsThisObjectFromPool(spawnedObject.gameObject))
            {
                // return back to scene
                // or update all game conditions
            }
        }
    }

    private void Initialized()
    {
        _objFabric?.SetActiveObjectsPool(true);
    }

    private void OnValidate()
    {
        if (_spots != null && _spots.Length < _amount)
        {
            Debug.LogError($"{this}: Add more spots for spawn. Spots:[{_spots.Length}], Amount: [{_amount}]");
        }
    }

    private void OnEnable()
    {
    }

    private void Awake()
    {
        if (_prefab != null)
        {
            PlayerController player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            _objFabric = new ObjectFabric(player, _prefab, _amount);
            if (_prefab.CompareTag("Duck"))
                _objFabric.CreateCollectables(_spots, false);
            if (_prefab.CompareTag("Enemy"))
                _objFabric.CreateEnemies(_spots, false);
            _name = $"Spwaner of {_prefab.name}";
            Initialized();
            CleanSpots();
        }
    }

    private void CleanSpots()
    {
        for (int i = 0; i < _spots.Length; i++)
        {
            if (_spots[i] != null)
            {
                UnityEngine.Object.Destroy(_spots[i].gameObject);
                _spots[i] = null;
            }
        }
    }

    private void OnDisable()
    {
    }

    private void OnDestroy()
    {
        _objFabric?.DeleteObjectsPool();
        _spots = null;
    }
}