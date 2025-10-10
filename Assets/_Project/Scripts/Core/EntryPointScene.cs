using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-900)]
public class EntryPointScene : MonoBehaviour
{
    // tutorial
    [SerializeField] private TextMeshProUGUI _tutorialText;

    [SerializeField] private ExitController _exit;
    [SerializeField] private GameplayManager _gameplay;
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private SoundMixerManager _soundMixerManager;
    [SerializeField] private PlayerController _player;

    private Coroutine _coPlayerInit;

    private void Awake()
    {
        GetUninitializedObjects();
        if (_player == null)
            MissedPlayerReference();
        else
        {
            Initialize();
        }
    }

    private void OnEnable()
    {
        GameplayManager.TutorialFinished += OnTutorialFinished;
    }

    private void MissedPlayerReference()
    {
        _coPlayerInit = StartCoroutine(WaitFor(
               () => _player != null,
               () => _player = UnityEngine.Object.FindFirstObjectByType<PlayerController>(),
               () =>
               {
                   Initialize();
                   _coPlayerInit = null;
               },
               10
           ));
    }

    private void Initialize()
    {
        if (_gameplay != null && _spawnManager != null)
        {
            _gameplay.Initialize(_player, _tutorialText, _exit, _spawnManager, SceneParameters.isTutorial);
            StartCoroutine(WaitInitSoundMixer(0.8f));
        }
        else
            Debug.LogWarning("Failed initialization");
    }

    private void GetUninitializedObjects()
    {
        if (_gameplay == null || _spawnManager == null)
        {
            GameObject obj = GameObject.Find("Level");
            _spawnManager ??= obj.GetComponent<SpawnManager>();
            _gameplay ??= obj.GetComponent<GameplayManager>();
        }
        if (_tutorialText == null)
            FindTutorial();

        if (_player == null)
        {
            TryGetComponent<PlayerController>(out PlayerController plController);
            _player = plController;
        }
    }

    private IEnumerator WaitFor(Func<bool> condition, System.Action cycleAction, System.Action afterAction, int attemps)
    {
        int t = attemps <= 0 ? 1 : attemps;
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        while (condition() != true && t > 0)
        {
            cycleAction?.Invoke();
            yield return wait;
            t--;
        }
        afterAction?.Invoke();
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
        yield return null;
        SoundFXManager.Instance.SetVolume(0.8f);
        SoundFXManager.Instance.AudioMixer.SetMusicVolume(0.1f);
        SoundFXManager.Instance.AudioMixer.SetSoundFXVolume(0.5f);
        SoundFXManager.Instance.BackgroundMusic.PlayMusic();
    }

    private void OnTutorialFinished()
    {
        SceneParameters.isTutorial = false;
        SceneParameters.level = SceneLoader.Level_1;
        //save score

        if (SceneLoader.Instance != null)
        {
            _ = SceneLoader.Instance.LoadSceneAsync(SceneParameters.level);
        }
    }

    private async void OnLevelFinished()
    {
        if (SceneManager.GetActiveScene().Equals(SceneParameters.level))
        {
            int buildIndex = SceneManager.GetActiveScene().buildIndex;
            //SceneParameters.level = SceneLoader.Instance.GetNextLevel();
        }
        // SceneParameters.level  = next
        //get current
        //save next

        await TryLoadScene(SceneParameters.level);
    }

    private async Task TryLoadScene(string sceneName)
    {
        int retries = 3;
        for (int attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                await SceneLoader.Instance.LoadSceneAsync(sceneName);
                Debug.Log("Scene loaded successfully!");
                return;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Attempt {attempt} failed: {ex.Message}");
                await Task.Delay(500);
            }
        }
    }

    private void OnDisable()
    {
        GameplayManager.TutorialFinished -= OnTutorialFinished;
        StopAllCoroutines();
        _coPlayerInit = null;
    }
}