using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SimpleFileBrowser;

public class ProcessedNode
{
    public string ip;
    public float x;
    public float y;
    public float activityScore;
    public List<AlertData> alerts;
    public string dominantAlertType;
    public HashSet<string> neighbours;
    public Dictionary<string, EdgeData> peerEdges;
    public ProtocolCounts protocols;
    public float outboundBytes;
    public float inboundBytes;
    public double firstSeen;
    public double lastSeen;
    public int uniquePeers;
    public List<PortEntry> topPorts;
}

public class GraphDataLoader : MonoBehaviour
{
    public List<ProcessedNode> Nodes { get; private set; }
    public List<EdgeData> Edges { get; private set; }
    public List<AlertData> Alerts { get; private set; }

    public bool IsLoaded { get; private set; } = false;

    private static readonly string[] AlertPriority = new string[]
    {
        "Ping Sweep",
        "ARP Spoofing",
        "DNS Tunneling",
        "Large Outbound Transfer",
        "DDoS Fan-In",
        "Brute Force",
        "SYN Flood",
        "Port Scan"
    };

    void Start()
    {
        StartCoroutine(OpenFolderPicker());
    }

    private IEnumerator OpenFolderPicker()
    {
        FileBrowser.SetFilters(false);

        yield return FileBrowser.WaitForLoadDialog(
            FileBrowser.PickMode.Folders,
            allowMultiSelection: false,
            initialPath: null,
            initialFilename: null,
            title: "Select PCAP Output Folder",
            loadButtonText: "Select"
        );

        if (!FileBrowser.Success)
        {
            Debug.LogError("GraphDataLoader: no folder selected.");
            yield break;
        }

        string folderPath = FileBrowser.Result[0];
        Debug.Log($"GraphDataLoader: loading from {folderPath}");

        LoadFromFolder(folderPath);
    }

    private void LoadFromFolder(string folderPath)
    {
        TopologyData topology = LoadJson<TopologyData>(
            Path.Combine(folderPath, "topology.json"));

        List<AlertData> alerts = LoadJsonArray<AlertData>(
            Path.Combine(folderPath, "alerts.json"));

        if (topology == null || alerts == null)
        {
            Debug.LogError("GraphDataLoader: topology.json or alerts.json failed to load.");
            return;
        }

        Alerts = alerts;
        Edges = topology.edges;

        // ── 1. Build alert lookup ──────────────────────────────────────
        Dictionary<string, List<AlertData>> alertMap =
            new Dictionary<string, List<AlertData>>();
        foreach (AlertData alert in alerts)
        {
            if (alert.source == null) continue;
            if (!alertMap.ContainsKey(alert.source))
                alertMap[alert.source] = new List<AlertData>();
            alertMap[alert.source].Add(alert);
        }

        // ── 2. Build neighbour lookup ──────────────────────────────────
        Dictionary<string, HashSet<string>> neighbourMap =
            new Dictionary<string, HashSet<string>>();
        foreach (EdgeData edge in topology.edges)
        {
            if (!neighbourMap.ContainsKey(edge.src))
                neighbourMap[edge.src] = new HashSet<string>();
            if (!neighbourMap.ContainsKey(edge.dst))
                neighbourMap[edge.dst] = new HashSet<string>();
            neighbourMap[edge.src].Add(edge.dst);
            neighbourMap[edge.dst].Add(edge.src);
        }

        // Build per-node peer edge lookup
        Dictionary<string, Dictionary<string, EdgeData>> peerEdgeMap =
            new Dictionary<string, Dictionary<string, EdgeData>>();
        foreach (EdgeData edge in topology.edges)
        {
            if (!peerEdgeMap.ContainsKey(edge.src))
                peerEdgeMap[edge.src] = new Dictionary<string, EdgeData>();
            if (!peerEdgeMap.ContainsKey(edge.dst))
                peerEdgeMap[edge.dst] = new Dictionary<string, EdgeData>();
            peerEdgeMap[edge.src][edge.dst] = edge;
            peerEdgeMap[edge.dst][edge.src] = edge;
        }

        // ── 3. Build ProcessedNode list ────────────────────────────────
        Nodes = new List<ProcessedNode>();
        foreach (NodeData n in topology.nodes)
        {
            List<AlertData> nodeAlerts = alertMap.ContainsKey(n.ip)
                ? alertMap[n.ip]
                : new List<AlertData>();

            ProcessedNode pn = new ProcessedNode
            {
                ip = n.ip,
                x = n.x,
                y = n.y,
                activityScore = n.activity_bytes,
                alerts = nodeAlerts,
                dominantAlertType = GetDominantAlert(nodeAlerts),
                neighbours = neighbourMap.ContainsKey(n.ip)
                        ? neighbourMap[n.ip]
                        : new HashSet<string>(),
                protocols = n.protocols,
                peerEdges = peerEdgeMap.ContainsKey(n.ip)
        ? peerEdgeMap[n.ip]
        : new Dictionary<string, EdgeData>(),
                outboundBytes = n.outbound_bytes,
                inboundBytes = n.inbound_bytes,
                firstSeen = n.first_seen,
                lastSeen = n.last_seen,
                uniquePeers = n.unique_peers,
                topPorts = n.top_ports
            };
            Nodes.Add(pn);
        }

        IsLoaded = true;
        Debug.Log($"GraphDataLoader: loaded {Nodes.Count} nodes, " +
                    $"{Edges.Count} edges, {Alerts.Count} alerts.");
    }

    private string GetDominantAlert(List<AlertData> nodeAlerts)
    {
        if (nodeAlerts == null || nodeAlerts.Count == 0) return null;

        int bestIndex = -1;
        string bestType = null;
        foreach (AlertData a in nodeAlerts)
        {
            int idx = System.Array.IndexOf(AlertPriority, a.type);
            if (idx > bestIndex)
            {
                bestIndex = idx;
                bestType = a.type;
            }
        }
        return bestType;
    }

    private T LoadJson<T>(string path) where T : class
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"GraphDataLoader: file not found: {path}");
            return null;
        }
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<T>(json);
    }

    private List<T> LoadJsonArray<T>(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"GraphDataLoader: file not found: {path}");
            return null;
        }
        string json = File.ReadAllText(path);
        string wrapped = "{\"items\":" + json + "}";
        JsonWrapper<T> wrapper = JsonUtility.FromJson<JsonWrapper<T>>(wrapped);
        return wrapper?.items;
    }

    [System.Serializable]
    private class JsonWrapper<T>
    {
        public List<T> items;
    }
}