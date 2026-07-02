using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnalystPanel : MonoBehaviour
{
    public GameObject panelRoot;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI panelText;
    public Transform alertsContainer;
    public GameObject alertButtonPrefab;
    public Button closeButton;
    // ConversationsOverlay removed
    public NodeHighlighter nodeHighlighter;
    public ReplayOverlayController replayOverlayController;

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

    private ProcessedNode _currentNode;

    private void ShowPanel(NodeController node)
    {
        _currentNode = node.Data;
        panelRoot.SetActive(true);
        titleText.text = BuildTitleText(_currentNode);
        BuildAlertButtons(_currentNode);
        panelText.text = BuildPanelText(_currentNode);

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRoot.GetComponent<RectTransform>());
    }

    private void Hide()
    {
        panelRoot.SetActive(false);
    }
    private string BuildTitleText(ProcessedNode data)
    {
        return data.ip;
    }

    private void BuildAlertButtons(ProcessedNode data)
    {
        // Snapshot children first — mutating the container while iterating over it is unsafe
        List<Transform> oldChildren = new List<Transform>();
        foreach (Transform child in alertsContainer)
            oldChildren.Add(child);

        foreach (Transform child in oldChildren)
        {
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        if (data.alerts == null || data.alerts.Count == 0)
            return;

        foreach (AlertData alert in data.alerts)
        {
            GameObject btnObj = Instantiate(alertButtonPrefab, alertsContainer);
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            string color = GetAlertColor(alert.type);

            btnText.text = $"<color={color}><b>{alert.type}</b></color>\n" +
                            $"Target: {alert.target}\n" +
                            $"{alert.details}";
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null && replayOverlayController != null)
            {
                btn.onClick.AddListener(() => replayOverlayController.Open());
            }
        }
    }
    private string BuildPanelText(ProcessedNode data)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();


        // ── Activity ───────────────────────────────────────────────────
        sb.AppendLine("<b>Traffic</b>");
        sb.AppendLine($"  Inbound:  {FormatBytes(data.inboundBytes)}");
        sb.AppendLine($"  Outbound: {FormatBytes(data.outboundBytes)}");
        if (data.inboundBytes > 0 && data.outboundBytes > 0)
        {
            float ratio = data.outboundBytes / data.inboundBytes;
            sb.AppendLine($"  Ratio (Out/In): {ratio:F1}x");
        }
        sb.AppendLine();

        // ── Timeline ───────────────────────────────────────────────────
        sb.AppendLine("<b>Activity Window</b>");
        sb.AppendLine($"  First seen: {FormatTimestamp(data.firstSeen)}");
        sb.AppendLine($"  Last seen:  {FormatTimestamp(data.lastSeen)}");
        double duration = data.lastSeen - data.firstSeen;
        sb.AppendLine($"  Duration:   {FormatDuration(duration)}");
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
            else sb.AppendLine("  No protocol data");
        }
        sb.AppendLine();

        // ── Top Ports ──────────────────────────────────────────────────
        sb.AppendLine("<b>Top Ports Contacted</b>");
        if (data.topPorts != null && data.topPorts.Count > 0)
        {
            foreach (PortEntry pe in data.topPorts)
                sb.AppendLine($"  :{pe.port} — {pe.count} pkts");
        }
        else sb.AppendLine("  No port data");
        sb.AppendLine();

        // ── Conversations ──────────────────────────────────────────────
        sb.AppendLine("<b>Conversations</b>");
        sb.AppendLine($"  Unique peers: {data.uniquePeers}");
        if (data.peerEdges != null && data.peerEdges.Count > 0)
        {
            int shown = 0;
            int total = 0;
            foreach (var kvp in data.peerEdges)
                total += kvp.Value.count;

            foreach (var kvp in data.peerEdges)
            {
                if (shown >= 5) break;
                string peer = kvp.Key;
                EdgeData edge = kvp.Value;
                int pct = total > 0 ? Mathf.RoundToInt(edge.count * 100f / total) : 0;
                sb.AppendLine($"  {peer} — {edge.count} pkts ({pct}%)");
                shown++;
            }
            if (data.peerEdges.Count > 5)
                sb.AppendLine($"  ... and {data.peerEdges.Count - 5} more");
        }
        else sb.AppendLine("  No conversations");

        return sb.ToString();
    }

    private string FormatBytes(float bytes)
    {
        if (bytes >= 1_000_000) return $"{bytes / 1_000_000f:F2} MB";
        if (bytes >= 1_000) return $"{bytes / 1_000f:F1} KB";
        return $"{bytes:F0} B";
    }

    private string FormatTimestamp(double unixTs)
    {
        System.DateTime dt = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)
            .AddSeconds(unixTs).ToLocalTime();
        return dt.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private string FormatDuration(double seconds)
    {
        if (seconds < 60) return $"{seconds:F1}s";
        if (seconds < 3600) return $"{seconds / 60:F1}min";
        return $"{seconds / 3600:F1}hr";
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