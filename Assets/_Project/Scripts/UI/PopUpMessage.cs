using TMPro;
using UnityEngine;

public class PopUpMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;

    private void Awake()
    {
        if (_textMeshProUGUI == null)
            Debug.LogError("Null reference");
        else
            _textMeshProUGUI.enabled = false;
    }

    public void ActivatePopUpText(bool isActive)
    {
        if (_textMeshProUGUI != null)
            _textMeshProUGUI.enabled = isActive;
    }

    private void OnEnable()
    {
        Tutorial.ShowPopUp += HandleShowPopUp;
        Tutorial.HidePopUp += HandleHidePopUp;
    }

    private void HandleShowPopUp(string msg)
    {
        if (_textMeshProUGUI != null)
        {
            _textMeshProUGUI.text = msg;
            _textMeshProUGUI.enabled = true;
        }
    }

    private void HandleHidePopUp()
    {
        if (_textMeshProUGUI != null)
            _textMeshProUGUI.enabled = false;
    }

    private void OnDisable()
    {
        Tutorial.ShowPopUp -= HandleShowPopUp;
        Tutorial.HidePopUp -= HandleHidePopUp;
    }
}