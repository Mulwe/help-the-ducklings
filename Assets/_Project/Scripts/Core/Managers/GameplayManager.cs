using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    //tutorial
    private TextMeshProUGUI _text;
    private ExitController _exit;
    private int _goal = 0;
    private int _amount = 0;
    private bool _levelFinished = false;

    private Coroutine _event = null;
    private Coroutine _winCondition = null;

    [Tooltip("Popup text settings")]
    private Coroutine _popupText = null;
    private float _duration = 1f;
    private bool _showMessage = false;
    private bool _isTutorial = false;

    private WaitForSeconds _waitForSecond = new WaitForSeconds(1f);
    private WaitForSeconds _waitForHalfSecond = new WaitForSeconds(0.5f);
    private WaitForSeconds _waitForTwoSeconds = new WaitForSeconds(2f);

    public static Action TutorialFinished;


    public void Initialize(TextMeshProUGUI t, ExitController exit, bool activateTutorial)
    {
        _text = t.GetComponent<TextMeshProUGUI>();
        _exit = exit;
        _isTutorial = activateTutorial;

        UpdateAmount();
        SetGoal(_amount);
        //shows first message
        SetGreetingText();
        _popupText = StartCoroutine(ShowMessage(3f, true));
        _winCondition = StartCoroutine(Win());

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
    }


    private void SetGreetingText()
    {
        if (_isTutorial)
        {
            UpdatePopUpText($"Deliver the ducks to the exit: (0/{_amount})");
        }
        else
        {
            UpdatePopUpText($"Deliver the ducks to the exit:\n(0/{_amount})");
        }
    }

    private void HandleCollected(int collectedCount)
    {
        if (_exit != null)
        {
            _event ??= StartCoroutine(UpdateTextUI
                        ($"Almost there!\nYou saved {_exit.Score} <color=yellow>ducklings</color>\n.",
                        $"Ducks left to save: <color=yellow>{_goal - _exit.Score}.</color>"));
        }
    }

    private void HandleCollectedOnTutorial(int collectedCount)
    {
        if (_exit != null)
        {
            _event ??= StartCoroutine(UpdateTextUI
                        ($"Nice job!\n.",
                        $"Ducks left to save: <color=yellow>{_goal - _exit.Score}.</color>"));
        }
    }


    private void UpdatePopUpText(string msg)
    {
        if (_text != null)
        {
            _text.text = msg;
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
        _event = null;
    }


    private void UpdateAmount()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.GetDucks() != null)
        {
            _amount = GameManager.Instance.GetDucks().Count;
        }
    }

    public void SetGoal(int newGoalScore)
    {
        if (newGoalScore < 0) return;
        _goal = newGoalScore;
    }

    IEnumerator WaitForCondition(Func<bool> condition, System.Action onAction)
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
            if (_text != null && _showMessage)
            {
                if (!_text.gameObject.activeSelf)
                {
                    _text.gameObject.SetActive(true);
                }
                _text.enabled = true;
                if (_duration != delay)
                {
                    newWait = new WaitForSeconds(_duration);
                    delay = _duration;
                }
                yield return newWait;
                _text.enabled = false;
                _showMessage = false;
            }
            yield return _waitForHalfSecond;
        }
    }

    IEnumerator Win()
    {
        yield return _waitForTwoSeconds;
        yield return WaitForCondition(() => GameManager.Instance != null,
            () =>
            {
                UpdateAmount();
                SetGoal(_amount - 1);
            });
        // Debug.Log("Start Win");
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
