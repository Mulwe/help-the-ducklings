using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


[DefaultExecutionOrder(-1000)]
public class EntryPoint : MonoBehaviour
{
    // tutorial
    [SerializeField] private TextMeshProUGUI _tutorialText;
    [SerializeField] private ExitController _exit;
    [SerializeField] private GameplayManager _gameplay;
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private SoundMixerManager _soundMixerManager;

    private SceneList _sceneList = null;
    private static string _tutorialName = "Tutorial";
    private static string _level_1 = "Gameplay";

    private void Awake()
    {
        InitAndCheckLoadedScene();
        Application.targetFrameRate = 60;
        GetUninitializedObjects();
        Initialize();
    }

    private void Initialize()
    {
        if (_gameplay != null)
            _gameplay.Initialize(_tutorialText, _exit, SceneParameters.isTutorial);
        StartCoroutine(WaitInitSoundMixer(0.8f));
    }


    private void GetUninitializedObjects()
    {
        if (_gameplay == null || _spawnManager == null)
        {
            GameObject obj = GameObject.Find("Level");
            _gameplay ??= obj.GetComponent<GameplayManager>();
            _spawnManager ??= obj.GetComponent<SpawnManager>();
        }
        if (_tutorialText == null)
            FindTutorial();
    }

    private void FindTutorial()
    {
        var tmp = GameObject.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int uiLayer = LayerMask.NameToLayer("UI");
        foreach (var t in tmp)
        {
            if (t != null && t.gameObject.layer == uiLayer)
                _tutorialText = t;
        }
    }

    private IEnumerator WaitSoundMixer(float mainVolume)
    {
        while (true)
        {
            SoundMixerManager manager = GameObject.FindFirstObjectByType<SoundMixerManager>();
            if (manager != null)
            {
                _soundMixerManager = manager;
                _soundMixerManager.SetDefaultValue(mainVolume);
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator WaitInitSoundMixer(float mainVolume)
    {
        while (SoundFXManager.Instance == null)
        {
            yield return null;
        }
        yield return WaitSoundMixer(mainVolume);
        yield return new WaitForSeconds(0.1f);
        SetGameVolume();
    }

    private void SetGameVolume()
    {
        SoundFXManager.Instance.SetVolume(0.8f);
        SoundFXManager.Instance.AudioMixer.SetMusicVolume(0.1f);
        SoundFXManager.Instance.AudioMixer.SetSoundFXVolume(0.5f);
        SoundFXManager.Instance.BackgroundMusic.PlayMusic();
    }

    private void InitAndCheckLoadedScene()
    {
        Debug.Log($"{SceneManager.GetActiveScene().name} is loaded");
        SceneList sl = Resources.Load<SceneList>("Data/SceneList");
        _sceneList = sl;

        if (SceneParameters.level == null)
        {
            Debug.Log($"<color=red>Yep</color>");
            SceneParameters.level = _tutorialName;
            SceneParameters.isTutorial = true;
        }
        //reload if not tutorial
        if (!SceneManager.GetActiveScene().name.Equals("Tutorial") && sl != null && SceneParameters.isTutorial)
        {
            LoadScene(_tutorialName, sl);
        }
    }



    private void OnEnable()
    {
        GameplayManager.TutorialFinished += OnTutorialFinished;
    }

    private void OnDisable()
    {
        GameplayManager.TutorialFinished -= OnTutorialFinished;
    }

    private void OnTutorialFinished()
    {
        SceneParameters.isTutorial = false;
        SceneParameters.level = _level_1;
        //save score

        LoadScene(_level_1, _sceneList);
    }

    private void OnLevelFinished()
    {
        // next / end Game
        //level
        //extra params

        //save score
        LoadScene(_level_1, _sceneList);
    }

    private void LoadScene(string sceneName, SceneList list)
    {
        int i = 0;
        if (list != null && list.IfHasSceneReturnIndex(sceneName, out int index) && index >= 0)
        {
            i = index;
            SceneManager.LoadScene(list.GetScene(index), LoadSceneMode.Single);
        }
        else
        {
            bool result = list.IfHasSceneReturnIndex(sceneName, out int index2);
            Debug.LogError($"Scene '{sceneName}' not found. Index:{index2}");
        }
    }

}

