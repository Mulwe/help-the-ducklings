using UnityEditor;
using UnityEngine;




[CreateAssetMenu(fileName = "SceneList", menuName = "Data/SceneList")]
public class SceneList : ScriptableObject
{
    [Tooltip("All scenes:")]
    [SerializeField] private SceneAsset[] _sceneAssets;


    public int SceneCount => _sceneAssets.Length;
    // public string[] Scenes => _scenes;

    public string[] ScenesNames
    {
        get
        {
            string[] names = new string[_sceneAssets.Length];
            for (int i = 0; i < _sceneAssets.Length; i++)
            {
                names[i] = _sceneAssets[i].name;

            }
            return names;
        }
    }

    public string GetScene(int index)
    {
        if (index < 0 || index >= _sceneAssets.Length)
            return null;
        return _sceneAssets[index].name;
    }




    public bool IfHasSceneReturnIndex(string sceneName, out int index)
    {
        index = -1;
        int i = 0;
        foreach (var scene in _sceneAssets)
        {

            if (scene.name.Equals(sceneName))
            {
                index = i;
                return true;
            }
            i++;
        }
        return false;
    }

    public bool HasScene(string sceneName)
    {
        foreach (var scene in _sceneAssets)
        {
            if (scene.name.Equals(sceneName)) return true;
        }
        return false;
    }
}
