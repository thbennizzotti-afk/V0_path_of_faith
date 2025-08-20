// Assets/Scripts/Fondation/UI/Pause/PauseCursorGuard.cs
using UnityEngine;

public class PauseCursorGuard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Time.timeScale == 0f)   // en pause
        {
            if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;
            if (!Cursor.visible) Cursor.visible = true;
        }
    }
}
