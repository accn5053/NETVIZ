using UnityEngine;

public class EdgeRenderer : MonoBehaviour
{
    private LineRenderer _line;
    private Color _originalColor;

    public string SrcIp { get; private set; }
    public string DstIp { get; private set; }

    private const float EDGE_HEIGHT = 0.005f;

    public void Initialise(Vector3 start, Vector3 end, string srcIp, string dstIp)
    {
        SrcIp = srcIp;
        DstIp = dstIp;

        _line = GetComponent<LineRenderer>();
        if (_line == null)
        {
            Debug.LogError("EdgeRenderer: no LineRenderer found on this GameObject.");
            return;
        }

        _originalColor = _line.startColor;

        Vector3 startBase = new Vector3(start.x, EDGE_HEIGHT, start.z);
        Vector3 endBase = new Vector3(end.x, EDGE_HEIGHT, end.z);

        _line.SetPosition(0, startBase);
        _line.SetPosition(1, endBase);
    }

    public void SetStyle(EdgeData edge, int maxCount)
    {
        // ── Thickness ──────────────────────────────────────────────────
        //float normalised = maxCount > 0
        //    ? Mathf.Log10(1 + edge.count) / Mathf.Log10(1 + maxCount)
        //    : 0f;
        //float width = Mathf.Lerp(0.02f, 0.25f, normalised);
        //_line.startWidth = width;
        //_line.endWidth = width;

        // ── Colour ─────────────────────────────────────────────────────
        ProtocolCounts p = edge.protocols;
        Color col;

        if (p == null)
        {
            col = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            int max = Mathf.Max(p.TCP, p.UDP, p.ICMP, p.ARP, p.Other);
            if (max == p.TCP) col = new Color(0.2f, 0.5f, 1.0f);  // blue
            else if (max == p.UDP) col = new Color(0.2f, 0.9f, 0.3f);  // green
            else if (max == p.ICMP) col = new Color(1.0f, 0.9f, 0.1f);  // yellow
            else if (max == p.ARP) col = new Color(1.0f, 0.5f, 0.1f);  // orange
            else col = new Color(0.5f, 0.5f, 0.5f);  // grey
        }

        _line.startColor = col;
        _line.endColor = col;
        _originalColor = col;
    }

    public void SetDimmed(bool dimmed)
    {
        Color c = dimmed ? new Color(0.1f, 0.1f, 0.1f, 1f) : _originalColor;
        _line.startColor = c;
        _line.endColor = c;
    }
}