using System.Collections.Generic;

[System.Serializable]
public class NodeData
{
    public string ip;
    public float x;
    public float y;
    public float activity_bytes;
}

[System.Serializable]
public class EdgeData
{
    public string src;
    public string dst;
    public int count;
}

[System.Serializable]
public class TopologyData
{
    public List<NodeData> nodes;
    public List<EdgeData> edges;
}