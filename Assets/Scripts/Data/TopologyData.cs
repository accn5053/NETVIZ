using System.Collections.Generic;

[System.Serializable]
public class NodeData
{
    public string ip;
    public float x;
    public float y;
    public float activity_bytes;
    public ProtocolCounts protocols;
}

[System.Serializable]
public class ProtocolCounts
{
    public int TCP;
    public int UDP;
    public int ICMP;
    public int ARP;
    public int Other;
}

[System.Serializable]
public class EdgeData
{
    public string src;
    public string dst;
    public int count;
    public ProtocolCounts protocols;
}

[System.Serializable]
public class TopologyData
{
    public List<NodeData> nodes;
    public List<EdgeData> edges;
}