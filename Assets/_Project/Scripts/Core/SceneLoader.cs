using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;
    [SerializeField] private GameObject _loadingScreen;

    public static string LevelTutorial = "Tutorial";
    public static string Level_1 = "Gameplay";



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


    public void SetLoadingScreen(GameObject loadingScreen)
    {
        _loadingScreen = loadingScreen;

    }

    public void Initialize()
    {

        InitializeSceneParameters();
    }


    public async Task LoadSceneAsync(string nameScene)
    {
        if (_loadingScreen != null)
        {
            _loadingScreen.SetActive(true);
            _loadingScreen.GetComponentInParent<Canvas>().enabled = true;
        }

        var task = LoadScene(nameScene);

        await task;

        if (_loadingScreen != null)
            _loadingScreen.SetActive(false);
    }

    private static async Task LoadScene(string sceneName)
    {
        var asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;



        while (asyncOperation.progress < 0.9f)
        {
            await Task.Yield();
        }

        await Task.Delay(500); // half second
        asyncOperation.allowSceneActivation = true;
    }






    private void InitializeSceneParameters()
    {
        if (SceneParameters.level == null)
        {
            SceneParameters.level = SceneLoader.LevelTutorial;
            //SceneParameters.isTutorial true by default
        }
    }

    public bool LoadTutorial()
    {
        if (!SceneManager.GetActiveScene().name.Equals(LevelTutorial) && SceneParameters.isTutorial)
        {
            SceneParameters.level = LevelTutorial;
            _ = LoadSceneAsync(LevelTutorial);
            return true;
        }
        return false;
    }

    private bool LoadNextLevel(string levelName)
    {
        if (!SceneManager.GetActiveScene().name.Equals(levelName) && SceneParameters.level.Equals(levelName))
        {
            SceneParameters.level = Level_1;
            _ = LoadSceneAsync(levelName);
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
