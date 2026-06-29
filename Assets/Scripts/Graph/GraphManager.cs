using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphManager : MonoBehaviour
{
    [Header("References")]
    public GraphDataLoader dataLoader;
    public GameObject nodePrefab;
    public GameObject edgePrefab;

    [Header("Materials")]
    public Material[] nodeMaterials;

    [Header("Layout")]
    public float layoutScale = 10f;

    public Dictionary<string, NodeController> NodeMap { get; private set; }
    public List<EdgeRenderer> Edges { get; private set; }

    void Start()
    {
        NodeMap = new Dictionary<string, NodeController>();
        StartCoroutine(SpawnAfterLoad());
    }

    private IEnumerator SpawnAfterLoad()
    {
        // Wait until GraphDataLoader finishes loading (user may take time to pick folder)
        while (!dataLoader.IsLoaded)
            yield return null;

        if (nodePrefab == null)
        {
            Debug.LogError("GraphManager: nodePrefab is not assigned.");
            yield break;
        }

        SpawnNodes();
        SpawnEdges();
    }

    private void SpawnNodes()
    {
        List<ProcessedNode> nodes = dataLoader.Nodes;
        if (nodes == null || nodes.Count == 0)
        {
            Debug.LogError("GraphManager: no nodes available from dataLoader.");
            return;
        }

        float maxActivity = 0f;
        foreach (ProcessedNode n in nodes)
            if (n.activityScore > maxActivity)
                maxActivity = n.activityScore;

        foreach (ProcessedNode n in nodes)
        {
            Vector3 position = new Vector3(
                n.x * layoutScale,
                0f,
                n.y * layoutScale
            );

            GameObject go = Instantiate(nodePrefab, position, Quaternion.identity);
            go.transform.parent = transform;

            float normActivity = maxActivity > 0f
                ? n.activityScore / maxActivity
                : 0f;

            NodeController nc = go.GetComponent<NodeController>();
            nc.Initialise(n, normActivity, nodeMaterials);

            NodeMap[n.ip] = nc;
        }

        Debug.Log($"GraphManager: spawned {NodeMap.Count} nodes.");
    }

    private void SpawnEdges()
    {
        if (dataLoader.Edges == null) return;

        // Deduplicate: treat A→B and B→A as the same edge
        System.Collections.Generic.HashSet<string> drawn =
            new System.Collections.Generic.HashSet<string>();
        int maxCount = 0;
        foreach (EdgeData edge in dataLoader.Edges)
            if (edge.count > maxCount)
                maxCount = edge.count;

        int spawnedCount = 0;
        Edges = new List<EdgeRenderer>();

        foreach (EdgeData edge in dataLoader.Edges)
        {
            // Build a canonical key: always alphabetically smaller IP first
            string key = string.Compare(edge.src, edge.dst) < 0
                ? edge.src + "|" + edge.dst
                : edge.dst + "|" + edge.src;

            if (drawn.Contains(key)) continue;
            drawn.Add(key);

            // Both endpoints must exist as spawned nodes
            if (!NodeMap.ContainsKey(edge.src) || !NodeMap.ContainsKey(edge.dst))
            {
                Debug.LogWarning($"GraphManager: skipping edge {edge.src}→{edge.dst}, " +
                                 "one or both nodes not found.");
                continue;
            }

            Vector3 startPos = NodeMap[edge.src].transform.position;
            Vector3 endPos = NodeMap[edge.dst].transform.position;

            GameObject go = Instantiate(edgePrefab, Vector3.zero, Quaternion.identity);
            go.transform.parent = transform;
            go.name = $"Edge_{edge.src}_{edge.dst}";

            EdgeRenderer er = go.GetComponent<EdgeRenderer>();
            er.Initialise(startPos, endPos, edge.src, edge.dst);
            er.SetStyle(edge, maxCount);
            Edges.Add(er);
            spawnedCount++;
        }

        Debug.Log($"GraphManager: spawned {spawnedCount} edges.");
    }
}