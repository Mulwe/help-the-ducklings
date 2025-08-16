using System.Collections;
using TMPro;
using UnityEngine;


[DefaultExecutionOrder(-1000)]
public class EntryPoint : MonoBehaviour
{
    // tutorial
    [SerializeField] private TextMeshProUGUI _tutorialText;
    [SerializeField] private ExitController _exit;
    [SerializeField] private GameplayManager _gameplay;
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private SoundMixerManager _soundMixerManager;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        GetUninitializedObjects();
        Initialize();
    }

    private void Initialize()
    {
        if (_gameplay != null)
            _gameplay.Initialize(_tutorialText, _exit);
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
}

