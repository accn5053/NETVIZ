using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HoverTooltip : MonoBehaviour
{
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;

    private RectTransform _panelRect;
    private Canvas _canvas;

    void Awake()
    {
        _panelRect = tooltipPanel.GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        tooltipPanel.SetActive(false);
    }

    void OnEnable()
    {
        NodeController.OnNodeHoverEnter += ShowTooltip;
        NodeController.OnNodeHoverExit += HideTooltip;
    }

    void OnDisable()
    {
        NodeController.OnNodeHoverEnter -= ShowTooltip;
        NodeController.OnNodeHoverExit -= HideTooltip;
    }

    void Update()
    {
        if (!tooltipPanel.activeSelf) return;
        FollowCursor();
    }

    private void ShowTooltip(NodeController node)
{
    ProcessedNode data = node.Data;

    string alertLine;
    if (data.alerts == null || data.alerts.Count == 0)
    {
        alertLine = "[OK] Clean";
    }
    else
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (AlertData a in data.alerts)
        {
            if (sb.Length > 0) sb.AppendLine();
            sb.Append($"[!] {a.type}");
        }
        alertLine = sb.ToString();
    }

    tooltipText.text =
        $"<b>{data.ip}</b>\n" +
        $"Activity: {Mathf.RoundToInt(data.activityScore)} bytes\n" +
        $"{alertLine}";

    tooltipPanel.SetActive(true);
    FollowCursor();
}

    private void HideTooltip(NodeController node)
    {
        tooltipPanel.SetActive(false);
    }

    private void FollowCursor()
    {
        // Resize panel height to fit text content
        float preferredHeight = tooltipText.preferredHeight + 20f;
        float preferredWidth = tooltipText.preferredWidth + 20f;
        _panelRect.sizeDelta = new Vector2(preferredWidth, preferredHeight);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.GetComponent<RectTransform>(),
            Input.mousePosition,
            _canvas.worldCamera,
            out Vector2 localPoint
        );

        _panelRect.localPosition = localPoint + new Vector2(12f, -12f);
    }
}