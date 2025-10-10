using UnityEditor;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class AlwaysStartScene
{
    static AlwaysStartScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Scene active = SceneManager.GetActiveScene();
            string scenePath = "Assets/_Project/Scenes/Bootstrap.unity";
            if (active.path != scenePath)
            {
                SceneManager.LoadScene("Bootstrap");
            }
        }
    }
}
