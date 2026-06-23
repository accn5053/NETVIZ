using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnalystPanel : MonoBehaviour
{
    public GameObject panelRoot;
    public TextMeshProUGUI panelText;
    public Button closeButton;
    public ConversationsOverlay conversationsOverlay;
    public UnityEngine.UI.Button showAllButton;
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
        if (showAllButton != null)
            showAllButton.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        NodeController.OnNodeClicked += ShowPanel;
    }

    void OnDisable()
    {
        NodeController.OnNodeClicked -= ShowPanel;
    }

    private ProcessedNode _currentNode;

    private void ShowPanel(NodeController node)
    {
        _currentNode = node.Data;
        panelRoot.SetActive(true);
        panelText.text = BuildPanelText(_currentNode);
    }

    private void Hide()
    {
        panelRoot.SetActive(false);
    }

    private string BuildPanelText(ProcessedNode data)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine($"<size=15><b>{data.ip}</b></size>\n");

        // ── Alerts ─────────────────────────────────────────────────────
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

        // ── Activity ───────────────────────────────────────────────────
        sb.AppendLine("<b>Activity</b>");
        sb.AppendLine($"  Outbound bytes: {Mathf.RoundToInt(data.activityScore)}");
        sb.AppendLine();

        // ── Protocol Breakdown ─────────────────────────────────────────
        sb.AppendLine("<b>Protocol Breakdown</b>");
        ProtocolCounts p = data.protocols;
        if (p != null)
        {
            int total = p.TCP + p.UDP + p.ICMP + p.ARP + p.Other;
            if (total > 0)
            {
                if (p.TCP > 0) sb.AppendLine($"  TCP:   {p.TCP} pkts ({Mathf.RoundToInt(p.TCP * 100f / total)}%)");
                if (p.UDP > 0) sb.AppendLine($"  UDP:   {p.UDP} pkts ({Mathf.RoundToInt(p.UDP * 100f / total)}%)");
                if (p.ICMP > 0) sb.AppendLine($"  ICMP:  {p.ICMP} pkts ({Mathf.RoundToInt(p.ICMP * 100f / total)}%)");
                if (p.ARP > 0) sb.AppendLine($"  ARP:   {p.ARP} pkts ({Mathf.RoundToInt(p.ARP * 100f / total)}%)");
                if (p.Other > 0) sb.AppendLine($"  Other: {p.Other} pkts ({Mathf.RoundToInt(p.Other * 100f / total)}%)");
            }
            else
            {
                sb.AppendLine("  No protocol data");
            }
        }
        sb.AppendLine();

        // ── Conversations (top 5 only) ─────────────────────────────────
        sb.AppendLine("<b>Conversations</b>");
        if (data.peerEdges != null && data.peerEdges.Count > 0)
        {
            int shown = 0;
            foreach (var kvp in data.peerEdges)
            {
                if (shown >= 5) break;
                string peer = kvp.Key;
                EdgeData edge = kvp.Value;
                ProtocolCounts ep = edge.protocols;

                System.Text.StringBuilder protoSb = new System.Text.StringBuilder();
                if (ep != null)
                {
                    if (ep.TCP > 0) protoSb.Append($"TCP={ep.TCP} ");
                    if (ep.UDP > 0) protoSb.Append($"UDP={ep.UDP} ");
                    if (ep.ICMP > 0) protoSb.Append($"ICMP={ep.ICMP} ");
                    if (ep.ARP > 0) protoSb.Append($"ARP={ep.ARP} ");
                    if (ep.Other > 0) protoSb.Append($"Other={ep.Other} ");
                }

                string protoStr = protoSb.Length > 0
                    ? protoSb.ToString().TrimEnd()
                    : "no protocol data";

                sb.AppendLine($"  {peer} ({protoStr}, {edge.count} pkts total)");
                shown++;
            }

            if (data.peerEdges.Count > 5)
            {
                sb.AppendLine($"  ... and {data.peerEdges.Count - 5} more");
                if (showAllButton != null)
                {
                    showAllButton.gameObject.SetActive(true);
                    showAllButton.onClick.RemoveAllListeners();
                    showAllButton.onClick.AddListener(() =>
                    {
                        Debug.Log("Show All clicked");
                        if (conversationsOverlay == null)
                            Debug.LogError("conversationsOverlay is null");
                        else
                            conversationsOverlay.Show(data);
                    });
                }
            }
            else
            {
                if (showAllButton != null)
                    showAllButton.gameObject.SetActive(false);
            }
        }
        else
        {
            sb.AppendLine("  No conversations");
            if (showAllButton != null)
                showAllButton.gameObject.SetActive(false);
        }
        sb.AppendLine();

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