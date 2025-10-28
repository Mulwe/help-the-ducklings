using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;
    [SerializeField] private GameObject _loadingScreen;
    [SerializeField] private Camera _loadingCamera;

    private UniTask _task;

    public static string LevelTutorial = "Tutorial";
    public static string Level_1 = "Gameplay";

    public static Action OnSceneUnloaded;
    public static Action OnSceneLoaded;

    private void Awake()
    {
        if (Instance == null)
        {
            ResetSceneParameters();
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetLoadingScreen(GameObject loadingScreen) => _loadingScreen = loadingScreen;

    public void Initialize()
    {
        InitializeSceneParameters();
    }

    // Async and single threaded (UniTask don't block the main thread)
    // crucial for WebGL support
    public async UniTask LoadSceneAsync(string sceneName)
    {
        _loadingScreen.SetActive(true);
        _loadingScreen.GetComponentInParent<Canvas>().enabled = true;

        var task = LoadScene(sceneName);

        await task;
        await UniTask.Delay(10);

        _loadingScreen.SetActive(false);
    }

    // Async and single threaded (UniTask don't block the main thread)
    // crucial for WebGL support
    private static async UniTask LoadScene(string sceneName)
    {
        OnSceneUnloaded?.Invoke();
        var asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;

        while (asyncOperation.progress < 0.9f)
            await UniTask.Yield();

        await UniTask.Delay(500); // half second

        asyncOperation.allowSceneActivation = true;
        OnSceneLoaded?.Invoke();
    }

    private void InitializeSceneParameters()
    {
        //SceneParameters.isTutorial true by default
        SceneParameters.level ??= SceneLoader.LevelTutorial;
    }

    //first run
    public bool LoadTutorialFirstRun()
    {
        if (!SceneManager.GetActiveScene().name.Equals(LevelTutorial) && SceneParameters.isTutorial)
        {
            SceneParameters.level = LevelTutorial;
            _task = LoadSceneAsync(LevelTutorial);
            return true;
        }
        return false;
    }

    private void ResetSceneParameters()
    {
        SceneParameters.isTutorial = true;
        SceneParameters.level = LevelTutorial;
        SceneParameters.playerScore = 0;
    }

    private void OnDestroy()
    {
        SceneParameters.isTutorial = false;
        SceneParameters.level = null;
        SceneParameters.playerScore = 0;
    }
}