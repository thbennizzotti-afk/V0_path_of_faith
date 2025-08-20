// Assets/Scripts/Debug/UIRaycastDoctor.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRaycastDoctor : MonoBehaviour
{
    void Start()
    {
        var ev = EventSystem.current;
        Debug.Log($"[Doctor] EventSystem: {(ev ? "OK" : "ABSENT")} | Module: {ev?.currentInputModule?.GetType().Name}");

        var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            var gr = c.GetComponent<GraphicRaycaster>();
            Debug.Log($"[Doctor] Canvas='{c.name}' mode={c.renderMode} order={c.sortingOrder}  Raycaster={(gr && gr.enabled ? "ON" : "OFF")}  activeInHierarchy={c.gameObject.activeInHierarchy}");
        }
    }
}
