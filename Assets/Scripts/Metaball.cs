using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class Metaball : MonoBehaviour
{
    [SerializeField] private float threshold = 0.5f;
    [SerializeField] private float smoothness = 0.1f;
    [SerializeField] private Color metaballColor = Color.white;

    private Material metaballMaterial;
    private List<Transform> metaballChildren = new List<Transform>();
    private static readonly int Positions = Shader.PropertyToID("_Positions");
    private static readonly int Count = Shader.PropertyToID("_Count");
    private static readonly int Threshold = Shader.PropertyToID("_Threshold");
    private static readonly int Smoothness = Shader.PropertyToID("_Smoothness");

    private void Awake()
    {
        // Get all child transforms
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Rigidbody2D>() != null)
            {
                metaballChildren.Add(child);
            }
        }

        // Create material instance
        metaballMaterial = new Material(Shader.Find("Custom/Metaball"));
        GetComponent<SpriteRenderer>().material = metaballMaterial;
        GetComponent<SpriteRenderer>().color = metaballColor;
    }

    private void Update()
    {
        UpdateMetaballShader();
    }

    private void UpdateMetaballShader()
    {
        Vector4[] positions = new Vector4[metaballChildren.Count];
        for (int i = 0; i < metaballChildren.Count; i++)
        {
            Vector3 pos = metaballChildren[i].position;
            positions[i] = new Vector4(pos.x, pos.y, pos.z, 0);
        }

        metaballMaterial.SetInt(Count, positions.Length);
        metaballMaterial.SetVectorArray(Positions, positions);
        metaballMaterial.SetFloat(Threshold, threshold);
        metaballMaterial.SetFloat(Smoothness, smoothness);
    }

    public void AddMetaballChild(Transform child)
    {
        if (!metaballChildren.Contains(child))
        {
            metaballChildren.Add(child);
        }
    }

    public void RemoveMetaballChild(Transform child)
    {
        if (metaballChildren.Contains(child))
        {
            metaballChildren.Remove(child);
        }
    }
}