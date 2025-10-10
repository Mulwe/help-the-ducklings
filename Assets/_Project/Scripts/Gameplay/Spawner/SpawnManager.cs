using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("List of all spawners:")]
    [SerializeField] private List<SpawnController> _list;
    public bool IsInit { get; private set; }

    public List<SpawnController> GetSpawners()
    {
        return _list;
    }

    public void Initialize()
    {
        if (_list != null)
        {
            foreach (SpawnController controller in _list)
            {
                if (controller != null && controller.GetName() != null)
                {
                    //Debug.Log($"{controller.GetName()} initilized");
                }
            }
            IsInit = true;
        }

    }


    /// <summary>
    /// Returns the <see cref="SpawnController"/> with the specified tag; 
    /// returns <c>null</c> if not found.
    /// </summary>
    /// <param name="tag">The tag of the <see cref="SpawnController"/> to search for.</param>
    /// <returns>The matching <see cref="SpawnController"/> instance, or <c>null</c> if not found.</returns>

    public SpawnController GetSpawnController(string tag)
    {
        if (_list == null) return null;

        foreach (SpawnController controller in _list)
        {
            if (controller != null && controller.GetSpawnedPrefab() != null)
            {

                if (controller.GetSpawnedPrefab().CompareTag(tag))
                {
                    return controller;
                }
            }
        }
        return null;
    }



    // update the amount
    private void OnEnable()
    {
        if (_list != null && _list.Count > 0)
        {
            IsInit = true;
        }
    }
    private void OnDisable()
    {
        IsInit = false;
    }

    private void Awake()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        _list?.Clear();
        _list = null;
        IsInit = false;
    }
}
