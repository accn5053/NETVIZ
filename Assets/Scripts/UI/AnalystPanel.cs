using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnalystPanel : MonoBehaviour
{
    public GameObject panelRoot;
    public TextMeshProUGUI panelText;
    public Button closeButton;
    public NodeHighlighter nodeHighlighter;

    void Awake()
    {
        panelRoot.SetActive(false);
        closeButton.onClick.AddListener(() =>
        {
            Hide();
            if (nodeHighlighter != null)
                nodeHighlighter.ResetAll();
        });
    }

    void OnEnable()
    {
        NodeController.OnNodeClicked += ShowPanel;
    }

    void OnDisable()
    {
        NodeController.OnNodeClicked -= ShowPanel;
    }

    private void ShowPanel(NodeController node)
    {
        ProcessedNode data = node.Data;
        panelText.text = BuildPanelText(data);
        panelRoot.SetActive(true);
    }

    private void Hide()
    {
        panelRoot.SetActive(false);
    }

    private string BuildPanelText(ProcessedNode data)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine($"<size=15><b>{data.ip}</b></size>\n");
        sb.AppendLine($"<b>Activity</b>");
        sb.AppendLine($"  Outbound bytes: {Mathf.RoundToInt(data.activityScore)}");
        sb.AppendLine();

        if (data.alerts == null || data.alerts.Count == 0)
        {
            sb.AppendLine("<color=#00FF00>No alerts detected</color>");
        }
        else
        {
            sb.AppendLine($"<b>Alerts ({data.alerts.Count})</b>");
            foreach (AlertData alert in data.alerts)
            {
                string color = GetAlertColor(alert.type);
                sb.AppendLine($"\n<color={color}><b>{alert.type}</b></color>");
                sb.AppendLine($"  Target: {alert.target}");
                sb.AppendLine($"  {alert.details}");
            }
        }

        return sb.ToString();
    }

    private string GetAlertColor(string alertType)
    {
        return alertType switch
        {
            "Port Scan" => "#FF4444",
            "Brute Force" => "#CC0000",
            "SYN Flood" => "#FF8C00",
            "DDoS Fan-In" => "#FF6400",
            "ARP Spoofing" => "#FFDC00",
            "Ping Sweep" => "#C8C800",
            "Large Outbound Transfer" => "#FF00FF",
            "DNS Tunneling" => "#00DCDC",
            _ => "#FFFFFF",
        };
    }
}