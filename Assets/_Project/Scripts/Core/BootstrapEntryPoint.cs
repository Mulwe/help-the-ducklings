using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class BootstrapEntryPoint : MonoBehaviour
{

    private IEnumerator Initialiaze()
    {

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
        SceneLoader.Instance.LoadTutorial();
    }

    private void Awake()
    {
        StartCoroutine(Initialiaze());
    }


}
