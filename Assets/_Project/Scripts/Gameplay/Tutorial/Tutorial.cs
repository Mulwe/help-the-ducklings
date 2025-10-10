using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private GameObject _exit;
    private GameObject _collectable;

    private CinemachineCamera _camera;
    private Transform _baseTarget;

    public static Action<string> ShowPopUp;
    public static Action HidePopUp;

    /*
     * collect ducks
     * heres the exit
     * */

    private void Awake()
    {
        _camera = UnityEngine.Object.FindFirstObjectByType<CinemachineCamera>();
        if (_camera != null)
        {
            _baseTarget = _camera.Follow;
        }

        StartCoroutine(StartTutorial());
    }

    private IEnumerator StartTutorial()
    {
        yield return InitCollectable();
        yield return ShowTip(_collectable, "Get all the <color=yellow>ducks</color> to open the exit!", 3f);
        yield return ShowTip(_exit, "This is the exit..", 2f);
        yield return ShowTip(_exit, "Bring the ducks here!", 1.5f);
        // yield return ShowTip();
        yield return null;
        TargetCameraOnObject(_baseTarget.gameObject);
    }

    private IEnumerator ShowTip(GameObject target, string msg, float dwellTimeSeconds)
    {
        WaitForSeconds wait = new(dwellTimeSeconds);
        if (target != null)
        {
            TargetCameraOnObject(target);                // Focus on target
            ShowPopUp?.Invoke(msg);                     // show popup text and send message
            yield return wait;                          //delay
            HidePopUp?.Invoke();                        // hide popup text
        }
    }

    private void TargetCameraOnObject(GameObject obj)
    {
        if (obj != null && obj.activeInHierarchy && _camera != null)
        {
            _camera.Follow = obj.transform;
            _camera.LookAt = obj.transform;
        }
    }

    private IEnumerator InitCollectable()
    {
        yield return WaitForCondition(
                        () => _collectable != null,
                        () =>
                        {
                            DuckController dc = FindFirstObjectByType<DuckController>();
                            if (dc != null)
                                _collectable = dc.gameObject;
                        },
                        null,
                        10
                        );
    }

    private IEnumerator WaitForCondition(Func<bool> condition, System.Action preAction, System.Action afterAction, int attemps)
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        attemps = attemps > 0 ? attemps : 1;
        while (attemps > 0 && condition() == false)
        {
            preAction?.Invoke();
            attemps--;
            yield return wait;
        }
        afterAction?.Invoke();
        yield return null;
    }
}