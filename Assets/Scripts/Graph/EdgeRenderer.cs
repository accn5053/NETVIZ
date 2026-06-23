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

    public void SetDimmed(bool dimmed)
    {
        Color c = dimmed ? new Color(0.1f, 0.1f, 0.1f, 1f) : _originalColor;
        _line.startColor = c;
        _line.endColor = c;
    }
}