using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("List of all spawners:")]
    [SerializeField] private List<SpawnController> _list;


    public List<SpawnController> GetSpawners()
    {
        return _list;
    }

    public void Initialize()
    {
        /*
        foreach (SpawnController controller in _list)
        {
            if (controller != null)
                Debug.Log($"{controller.GetName()} initilized");
        }
        */
    }

    private void Awake()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        _list?.Clear();
    }


}
