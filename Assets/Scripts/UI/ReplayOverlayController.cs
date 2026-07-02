using UnityEngine;

public class ReplayOverlayController : MonoBehaviour
{
    [Header("References")]
    public GameObject replayOverlayCanvas;   // ReplayOverlayCanvas root
    public GameObject analystPanelRoot;      // AnalystPanel's root GameObject
    public UnityEngine.UI.Button closeReplayButton;

    void Awake()
    {
        if (closeReplayButton != null)
            closeReplayButton.onClick.AddListener(Close);

        // Start hidden
        if (replayOverlayCanvas != null)
            replayOverlayCanvas.SetActive(false);
    }
    [ContextMenu("Open")]
    public void Open()
    {
        if (replayOverlayCanvas != null)
            replayOverlayCanvas.SetActive(true);

        if (analystPanelRoot != null)
            analystPanelRoot.SetActive(false);
    }
    [ContextMenu("Close")]
    public void Close()
    {
        if (replayOverlayCanvas != null)
            replayOverlayCanvas.SetActive(false);

        if (analystPanelRoot != null)
            analystPanelRoot.SetActive(true);
    }
}