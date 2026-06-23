using System.Collections.Generic;
using UnityEngine;

public class NodeController : MonoBehaviour
{
    // Set by GraphManager after spawning
    public ProcessedNode Data { get; private set; }

    // Height constants
    private const float MIN_HEIGHT = 0.01f;
    private const float MAX_HEIGHT = 2.0f;
    private Color _originalColor;

    // Materials — assigned by GraphManager
    private Material[] _materials;

    // Store original scale X and Z so we only modify Y
    private float _baseScaleX;
    private float _baseScaleZ;

    // Events for UI scripts to subscribe to
    public static event System.Action<NodeController> OnNodeHoverEnter;
    public static event System.Action<NodeController> OnNodeHoverExit;
    public static event System.Action<NodeController> OnNodeClicked;

    public void Initialise(ProcessedNode data, float normalisedActivity, Material[] materials)
    {
        Data = data;
        _materials = materials;
        _baseScaleX = transform.localScale.x;
        _baseScaleZ = transform.localScale.z;

        SetHeight(normalisedActivity);
        SetColor(data.dominantAlertType);
        _originalColor = GetComponent<Renderer>().material.color;

        gameObject.name = data.ip;
    }

    // normalisedActivity is 0..1 across all nodes in the scene
    private void SetHeight(float normalisedActivity)
    {
        float height = Mathf.Lerp(MIN_HEIGHT, MAX_HEIGHT, normalisedActivity);
        transform.localScale = new Vector3(_baseScaleX, height, _baseScaleZ);

        // Shift up so the cylinder base sits on Y=0
        transform.position = new Vector3(
            transform.position.x,
            height,         // Unity cylinders pivot at centre, so offset by height
            transform.position.z
        );
    }

    private void SetColor(string alertType)
    {
        Material mat = alertType switch
        {
            "Port Scan" => GetMaterial("NodeMat_PortScan"),
            "Brute Force" => GetMaterial("NodeMat_BruteForce"),
            "SYN Flood" => GetMaterial("NodeMat_SYNFlood"),
            "DDoS Fan-In" => GetMaterial("NodeMat_DDoS"),
            "ARP Spoofing" => GetMaterial("NodeMat_ARPSpoofing"),
            "Ping Sweep" => GetMaterial("NodeMat_PingSweep"),
            "Large Outbound Transfer" => GetMaterial("NodeMat_LargeOutbound"),
            "DNS Tunneling" => GetMaterial("NodeMat_DNSTunneling"),
            _ => GetMaterial("NodeMat_Clean"),
        };

        if (mat != null)
            GetComponent<Renderer>().material = mat;
    }

    private Material GetMaterial(string name)
    {
        if (_materials == null) return null;
        foreach (Material m in _materials)
            if (m != null && m.name == name)
                return m;
        Debug.LogWarning($"NodeController: material '{name}' not found.");
        return null;
    }

    // ── Mouse events ───────────────────────────────────────────────────
    private void OnMouseEnter()
    {
        OnNodeHoverEnter?.Invoke(this);
    }

    private void OnMouseExit()
    {
        OnNodeHoverExit?.Invoke(this);
    }

    private void OnMouseDown()
    {
        OnNodeClicked?.Invoke(this);
    }

    public void RestoreColor()
    {
        GetComponent<Renderer>().material.color = _originalColor;
    }
}