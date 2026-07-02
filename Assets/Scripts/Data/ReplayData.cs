using System.Collections.Generic;

[System.Serializable]
public class PortScanEvidenceEvent
{
    public float timestamp;
    public string src_ip;
    public string dst_ip;
    public int dst_port;
    public int tcp_flags;
}

[System.Serializable]
public class SynFloodEvidenceEvent
{
    public float timestamp;
    public string src_ip;
    public string dst_ip;
    public int dst_port;
    public int tcp_flags;
}

[System.Serializable]
public class DnsTunnelingEvidenceEvent
{
    public float timestamp;
    public string src_ip;
    public string dst_ip;
    public string dns_query;
    public bool suspicious;
    public float entropy;
    public string subdomain;
    public List<string> suspicious_reasons;
}

[System.Serializable]
public class PortScanThresholds
{
    public int unique_ports;
    public int min_syns;
}

[System.Serializable]
public class SynFloodThresholds
{
    public int min_syns;
    public float syn_synack_ratio;
}

[System.Serializable]
public class DnsTunnelingThresholds
{
    public int min_total_queries;
    public int min_suspicious;
    public float entropy_min;
    public int label_max;
    public int full_query_max;
}

[System.Serializable]
public class PortScanFinalValues
{
    public int unique_ports;
    public int syn_count;
}

[System.Serializable]
public class SynFloodFinalValues
{
    public int syn_count;
    public int synack_count;
    public float ratio;
}

[System.Serializable]
public class DnsTunnelingFinalValues
{
    public int total_queries;
    public int suspicious_count;
}

[System.Serializable]
public class PortScanRunningState
{
    public int unique_ports;
    public int syn_count;
}

[System.Serializable]
public class SynFloodRunningState
{
    public int syn_count;
    public int synack_count;
    public float ratio;
}

[System.Serializable]
public class DnsTunnelingRunningState
{
    public int total_queries;
    public int suspicious_count;
}

[System.Serializable]
public class ReplayEdge
{
    public string src;
    public string dst;
    public int event_count;
    public List<int> ports;          // Port Scan only
    public List<string> queries;     // DNS Tunneling only
}

[System.Serializable]
public class ReplayFrame
{
    public int time_bucket;
    public bool gate_passed;

    public List<PortScanEvidenceEvent> portScanEvents;
    public List<SynFloodEvidenceEvent> synFloodEvents;
    public List<DnsTunnelingEvidenceEvent> dnsTunnelingEvents;

    public PortScanRunningState portScanRunningState;
    public SynFloodRunningState synFloodRunningState;
    public DnsTunnelingRunningState dnsTunnelingRunningState;

    public List<ReplayEdge> edges;
}

[System.Serializable]
public class ReplayData
{
    public string detector_type;
    public bool evidence_capped;
    public int evidence_cap;

    public PortScanThresholds portScanThresholds;
    public SynFloodThresholds synFloodThresholds;
    public DnsTunnelingThresholds dnsTunnelingThresholds;

    public PortScanFinalValues portScanFinalValues;
    public SynFloodFinalValues synFloodFinalValues;
    public DnsTunnelingFinalValues dnsTunnelingFinalValues;

    public List<ReplayFrame> frames;
}