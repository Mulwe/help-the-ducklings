using System.Collections.Generic;
using UnityEngine;


public class ObjectFabric
{
    private int _amount = 0;
    private GameObject _prefab;
    private List<GameObject> _objPool;

    private PlayerController _playerController;
    private PlayerAttachment _playerAttachment;

    public ObjectFabric(PlayerController player, GameObject prefab, int amount)
    {
        _prefab = prefab;
        _amount = amount;
        _playerController = player;
        if (player.TryGetComponent<PlayerAttachment>(out var pl))
            _playerAttachment = pl;
    }



    public void CreateCollectables(Transform[] spots, bool setActive)
    {
        if (_prefab != null && spots != null && _amount <= 0) return;

        if (_objPool != null) DeleteObjectsPool();

        _objPool ??= new List<GameObject>();
        for (int i = 0; i < _amount; i++)
        {
            GameObject obj = Object.Instantiate(_prefab, spots[i].position, Quaternion.identity, spots[i].parent);
            if (obj.TryGetComponent<DuckController>(out var duck))
            {
                duck.InitPlayer(_playerController, _playerAttachment);
            }
            obj.SetActive(setActive);

            _objPool.Add(obj);
        }
    }

    public void CreateEnemies(Transform[] spots, bool setActive)
    {
        if (_prefab != null && spots != null && _amount <= 0) return;
        if (_objPool != null) DeleteObjectsPool();

        _objPool ??= new List<GameObject>();

        List<int> indexes = new List<int>();
        for (int k = 0; k < spots.Length; k++)
            indexes.Add(k);

        ShuffleList<int>(indexes);
        for (int i = 0; i < _amount; i++)
        {
            //random spot
            GameObject obj = Object.Instantiate(_prefab, spots[indexes[i]].position, Quaternion.identity, spots[i].parent);
            obj.SetActive(setActive);
            _objPool.Add(obj);
        }
    }



    public void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }



    public List<GameObject> GetObjectsPool()
    {
        return _objPool;
    }



    public void SetActiveObjectsPool(bool isActive)
    {
        if (_objPool != null)
        {
            foreach (GameObject obj in _objPool)
            {
                if (obj != null)
                    obj.SetActive(isActive);
            }
        }
    }

    public void DeleteObjectsPool()
    {
        if (_objPool != null)
        {
            for (int i = 0; i < _objPool.Count; i++)
            {
                UnityEngine.GameObject.Destroy(_objPool[i]);
            }
            _objPool.Clear();
            _objPool = null;
        }
    }
}
