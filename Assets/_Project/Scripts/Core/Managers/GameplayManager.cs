using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    [Tooltip("Tutorial")]
    private TextMeshProUGUI _popUpText;

    [Tooltip("Goal / Win condition")]
    private ExitController _exit;
    private int _goal = 0;
    private int _amount = 0;
    private bool _levelFinished = false;

    [Tooltip("All conditions")]
    private Coroutine _coEvent = null;
    private Coroutine _coWin = null;
    private Coroutine _coStartGameplay = null;

    [Tooltip("Popup text settings")]
    private Coroutine _coPopUpMessage = null;
    private float _duration = 1f;
    private bool _showMessage = false;
    private bool _isTutorial = false;


    [Tooltip("Data")]
    private SpawnManager _spawnManager;
    private PlayerController _player;

    private WaitForSeconds _waitForSecond = new WaitForSeconds(1f);
    private WaitForSeconds _waitForHalfSecond = new WaitForSeconds(0.5f);
    private WaitForSeconds _waitForTwoSeconds = new WaitForSeconds(2f);

    public static Action TutorialFinished;


    public void Initialize(PlayerController player, TextMeshProUGUI t, ExitController exit, SpawnManager sp, bool activateTutorial)
    {
        _popUpText = t.GetComponent<TextMeshProUGUI>();
        //turn on gameObject of _popUpText if disabled
        SetupPopUp(_popUpText);

        _player = player;
        _spawnManager = sp;
        _exit = exit;
        _isTutorial = activateTutorial;

        if (_spawnManager == null)
            _spawnManager = UnityEngine.Object.FindFirstObjectByType<SpawnManager>();

        StartCoroutine(WaitForCondition(() => GameManager.Instance != null, () => GameManager.Instance.SetPlyerController(_player)));
        if (_coStartGameplay != null)
            StopCoroutine(_coStartGameplay);

        _coStartGameplay = StartCoroutine(StartGameplay());
    }
    public void SetGoal(int newGoalScore)
    {
        if (newGoalScore < 0) return;
        _goal = newGoalScore;
    }

    private IEnumerator StartGameplay()
    {
        if (_spawnManager != null)
            yield return WaitForCondition(() => _spawnManager.GetSpawners() != null, null);
        yield return AwaitOfSpawnManagerData();
        if (_amount == -1)
            _amount = 0;
        // After receiving all data
        SetGoal(_amount);
        // text on Start of level
        SetGreetingText();
        if (_coPopUpMessage != null)
            StopCoroutine(_coPopUpMessage);
        _coPopUpMessage = StartCoroutine(ShowMessage(3f, true));
        if (_coWin != null)
            StopCoroutine(_coWin);
        _coWin = StartCoroutine(Win());
        yield return null;
        _coStartGameplay = null;
    }

    private void SetupPopUp(TextMeshProUGUI popUp)
    {
        if (popUp != null)
        {
            popUp.enabled = false;
            if (!popUp.gameObject.activeInHierarchy)
                popUp.gameObject.SetActive(true);
        }
    }

    private IEnumerator AwaitOfSpawnManagerData()
    {
        int i = 0;
        while (i < 100)
        {
            int result = GetDucksAmount();
            if (result >= 0)
            {
                _amount = result;
                yield break;
            }
            yield return _waitForHalfSecond;
            i++;
        }
    }


    // -1 _spawnManager doesnt have DuckSpawner or not init
    private int GetDucksAmount()
    {
        if (_spawnManager != null && _spawnManager.GetSpawners() != null && _spawnManager.GetSpawners().Count > 0)
        {
            foreach (SpawnController controller in _spawnManager.GetSpawners())
            {
                if (controller != null)
                {
                    if (controller.GetSpawnedPrefab() != null && controller.GetSpawnedPrefab().CompareTag("Duck"))
                    {
                        return controller.GetSpawnedObjects().Count;
                    }
                }
            }
        }
        return -1;
    }

    private void OnEnable()
    {
        if (_isTutorial)
            ExitController.OnDucksCollected += HandleCollectedOnTutorial;
        else
            ExitController.OnDucksCollected += HandleCollected;
    }

    private void OnDisable()
    {
        if (_isTutorial)
            ExitController.OnDucksCollected -= HandleCollectedOnTutorial;
        else
            ExitController.OnDucksCollected -= HandleCollected;
        StopAllCoroutines();
        _coStartGameplay = null;
        _coEvent = null;
        _coWin = null;
        _coPopUpMessage = null;
    }



    private void SetGreetingText()
    {
        if (_isTutorial)
        {
            //UpdatePopUpText($"Deliver the ducks to the exit:\n(0/{_amount})");
        }
        else
        {
            UpdatePopUpText($"Ducks left:\n(0/{_amount})");
        }
    }

    private void HandleCollected(int collectedCount)
    {
        if (_exit != null)
        {
            _coEvent ??= StartCoroutine(UpdateTextUI
                        ($"Almost there!\nYou saved {_exit.Score} <color=yellow>ducklings</color>\n.",
                        $"Ducks left to save: <color=yellow>{_goal - _exit.Score}.</color>"));
        }
    }

    private void HandleCollectedOnTutorial(int collectedCount)
    {
        if (_exit != null)
        {
            _coEvent ??= StartCoroutine(UpdateTextUI
                        ($"Nice job!\n.",
                        $"Ducks left to save: <color=yellow>{_goal - _exit.Score}.</color>"));
        }
    }


    private void UpdatePopUpText(string msg)
    {
        if (_popUpText != null)
        {
            _popUpText.text = msg;
        }
    }

    private IEnumerator UpdateTextUI(string msg, string msg2)
    {
        if (!_showMessage)
        {
            ShowPopUpText(msg, 2f);
            yield return _waitForTwoSeconds;
            yield return _waitForHalfSecond;

            if (_goal - _exit.Score > 0)
            {
                ShowPopUpText(msg2, 2f);
                yield return _waitForTwoSeconds;
                yield return _waitForHalfSecond;
            }
        }
        _coEvent = null;
    }


    private void UpdateAmount()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.GetDucks() != null)
        {
            _amount = GameManager.Instance.GetDucks().Count;
        }
    }


    /// <summary>
    ///  while  condition false - wait
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="onAction"></param>
    /// <returns></returns>
    private IEnumerator WaitForCondition(Func<bool> condition, System.Action onAction)
    {
        // while  condition false - wait
        while (!condition())
        {
            yield return null;
        }
        onAction?.Invoke();
    }


    private void ShowPopUpText(string message, float duration)
    {
        _showMessage = true;
        UpdatePopUpText(message);
        _duration = duration;
    }



    private IEnumerator ShowMessage(float delay, bool showMessage)
    {
        _duration = delay;
        _showMessage = showMessage;
        WaitForSeconds newWait = new WaitForSeconds(delay);
        while (true)
        {
            if (_popUpText != null && _showMessage)
            {
                if (!_popUpText.gameObject.activeSelf)
                {
                    _popUpText.gameObject.SetActive(true);
                }
                _popUpText.enabled = true;
                if (_duration != delay)
                {
                    newWait = new WaitForSeconds(_duration);
                    delay = _duration;
                }
                yield return newWait;
                _popUpText.enabled = false;
                _showMessage = false;
            }
            yield return _waitForHalfSecond;
        }
    }

    IEnumerator Win()
    {
        yield return _waitForTwoSeconds;
        while (true)
        {

            if (_exit != null)
            {
                _levelFinished = _exit.Score == _amount || _exit.Score >= _goal;
            }
            if (_levelFinished && _isTutorial)
            {
                string msg = $"Good job!\n You're score is <color=yellow>{_exit.Score}</color>";
                ShowPopUpText(msg, 3f);
                Debug.Log("Load next level.Transition");
                // delay before loading level
                yield return _waitForTwoSeconds;
                TutorialFinished?.Invoke();
                yield break;
            }
            if (_levelFinished && !_isTutorial)
            {
                string msg = $"You're won!\n You're score is {_exit.Score}\n Thanks for playing";
                ShowPopUpText(msg, 3f);
                break;
            }
            yield return (_waitForSecond);
        }
        yield return new WaitForSeconds(2f);
        UnityEngine.Application.Quit();
    }

}
