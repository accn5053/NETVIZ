using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConversationsOverlay : MonoBehaviour
{
    public GameObject overlayRoot;
    public TextMeshProUGUI conversationsText;
    public Button closeButton;

    void Awake()
    {
        overlayRoot.SetActive(false);
        closeButton.onClick.AddListener(Hide);
    }

    public void Show(ProcessedNode data)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"<size=15><b>All Conversations — {data.ip}</b></size>\n");

        if (data.peerEdges != null && data.peerEdges.Count > 0)
        {
            foreach (var kvp in data.peerEdges)
            {
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
            }
        }
        else
        {
            sb.AppendLine("  No conversations found.");
        }

        conversationsText.text = overlayRoot.activeSelf ? conversationsText.text : sb.ToString();
        conversationsText.text = sb.ToString();
        overlayRoot.SetActive(true);
    }

    public void Hide()
    {
        overlayRoot.SetActive(false);
    }
}