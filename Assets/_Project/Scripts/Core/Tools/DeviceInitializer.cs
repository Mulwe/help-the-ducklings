using UnityEngine;
using UnityEngine.InputSystem;

public class DeviceInitializer : MonoBehaviour
{
    private void Awake()
    {
        if (Keyboard.current != null && !Keyboard.current.enabled)
            InputSystem.EnableDevice(Keyboard.current);
    }
}