using UnityEngine;
using PathOfFaith.Fondation.Core;
using TMPro;

public class CamModeHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private string textFollow = "Mode: Suivi";
    [SerializeField] private string textFree   = "Mode: Libre";

    void OnEnable()  { EventBus.OnCamModeChanged += OnCamModeChanged; }
    void OnDisable() { EventBus.OnCamModeChanged -= OnCamModeChanged; }

    void Start() { if (label) label.text = textFollow; }

    private void OnCamModeChanged(Events.CamModeChanged e)
    {
        if (!label) return;
        label.text = (e.NewMode == Events.CamModeChanged.Mode.Free) ? textFree : textFollow;
    }
}
