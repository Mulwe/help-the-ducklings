using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class BootstrapEntryPoint : MonoBehaviour
{
    private IEnumerator Initialiaze()
    {
        Camera.main.backgroundColor = Color.black;
        var loadingDuration = 1f;
        while (loadingDuration > 0f)
        {
            loadingDuration -= Time.deltaTime;
            yield return null;
        }

        // как инициализируется сам загружает нужную сцену
        while (SceneLoader.Instance == null)
        {
            yield return null;
        }

        SceneLoader.Instance.Initialize();
        SceneLoader.Instance.LoadTutorialFirstRun();
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        StartCoroutine(Initialiaze());
    }
}