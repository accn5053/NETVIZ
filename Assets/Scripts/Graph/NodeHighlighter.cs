using System.Collections.Generic;
using UnityEngine;

public class NodeHighlighter : MonoBehaviour
{
    public GraphManager graphManager;
    public Renderer floorRenderer;

    private NodeController _selected;
    private Color _floorOriginalColor;
    
    void Start()
    {
        if (floorRenderer != null)
            _floorOriginalColor = floorRenderer.material.color;
    }

    void OnEnable()
    {
        NodeController.OnNodeClicked += HandleNodeClicked;
    }

    void OnDisable()
    {
        NodeController.OnNodeClicked -= HandleNodeClicked;
    }

    private void HandleNodeClicked(NodeController clicked)
    {
        if (_selected == clicked)
        {
            ResetAll();
            return;
        }

        // If switching from one node to another, restore first
        if (_selected != null)
            ResetAll();

        _selected = clicked;
        HashSet<string> neighbours = clicked.Data.neighbours;
        string clickedIp = clicked.Data.ip;

        foreach (var kvp in graphManager.NodeMap)
        {
            bool isRelevant = kvp.Key == clickedIp ||
                              neighbours.Contains(kvp.Key);

            Renderer r = kvp.Value.GetComponent<Renderer>();
            Color c = r.material.color;
            float brightness = isRelevant ? 1f : 0.15f;
            r.material.color = new Color(c.r * brightness, c.g * brightness, c.b * brightness, 1f);
        }

        foreach (EdgeRenderer edge in graphManager.Edges)
        {
            bool isConnected = edge.SrcIp == clickedIp ||
                               edge.DstIp == clickedIp;
            edge.SetDimmed(!isConnected);
        }

        if (floorRenderer != null)
            floorRenderer.material.color = new Color(0.1f, 0.1f, 0.1f, 1f);
    }

    public void ResetAll()
    {
        _selected = null;

        // Restore nodes
        foreach (var kvp in graphManager.NodeMap)
            kvp.Value.RestoreColor();

        // Restore edges
        foreach (EdgeRenderer edge in graphManager.Edges)
            edge.SetDimmed(false);

        // Restore floor
        if (floorRenderer != null)
            floorRenderer.material.color = _floorOriginalColor;
    }
}