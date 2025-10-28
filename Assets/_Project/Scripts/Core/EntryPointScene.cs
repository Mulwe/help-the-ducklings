using Cysharp.Threading.Tasks;
using System;
using System.Collections;
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

    [Range(0f, 1f)]
    [SerializeField] private float _baseSoundVolume = 0.8f;

    private Coroutine _coPlayerInit;
    private UniTask _task;

    private void Awake()
    {
        GetUninitializedObjects();
        if (_player == null)
            MissedPlayerReference();
        else
            Initialize();
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
            StartCoroutine(WaitInitSoundMixer(_baseSoundVolume));
            _gameplay.Initialize(_player, _tutorialText, _exit, _spawnManager, SceneParameters.isTutorial);
        }
        else
        {
            Debug.LogWarning("Failed initialization");
        }
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

    private IEnumerator WaitInitSoundMixer(float mainVolume)
    {
        SoundMixerManager soundMixer = null;
        WaitForSeconds wait = new WaitForSeconds(0.1f);

        while (soundMixer == null)
        {
            soundMixer = GameObject.FindFirstObjectByType<SoundMixerManager>();

            if (soundMixer != null)
            {
                _soundMixerManager = soundMixer;
                break;
            }

            yield return wait;
        }
        yield return null;

        //set base volume or set it manual through SoundFXManager
        _soundMixerManager.SetDefaultValue(mainVolume);
        SoundFXManager.Instance.BackgroundMusic.PlayMusic();
    }

    private void OnTutorialFinished()
    {
        SceneParameters.isTutorial = false;
        SceneParameters.level = SceneLoader.Level_1;
        //save score
        LoadScene(SceneParameters.level);
    }

    private void LoadScene(string levelName)
    {
        _task = SceneLoader.Instance.LoadSceneAsync(levelName);
    }

    private void OnDisable()
    {
        GameplayManager.TutorialFinished -= OnTutorialFinished;
        StopAllCoroutines();
        _coPlayerInit = null;
    }
}