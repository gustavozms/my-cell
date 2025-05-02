using UnityEngine;
using System.Collections.Generic;
using System;

public enum WallType
{
    InnerWall,
    OuterWall
}

public class Cell : MonoBehaviour
{
    [Header("Membrane Settings")]
    [SerializeField] private GameObject wallNodePrefab;
    [SerializeField] private int nodeCount = 20;
    [SerializeField] private float radius = 5f;
    [SerializeField] private float perimeterJointFrequency = 100f;
    [SerializeField] private float centerJointFrequency = 2f;

    private List<GameObject> nodes = new List<GameObject>();
    private List<SpringJoint2D> perimeterJoints = new List<SpringJoint2D>();
    private List<SpringJoint2D> centerJoints = new List<SpringJoint2D>();

    void Start()
    {
        SetupCellularMembrane(WallType.OuterWall, Vector2.zero, radius, nodeCount);
    }

    public void SetupCellularMembrane(WallType wall, Vector2 center, float radius, int count)
    {
        ClearExistingMembrane();
        SetupPerimeter(wall, center, radius, count);
        SetupPerimeterJoints();
        SetupCenterJoints();
    }

    private void SetupPerimeter(WallType wall, Vector2 center, float radius, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = Mathf.PI * 2f / count * i;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Vector2 position = center + offset;

            GameObject node = Instantiate(wallNodePrefab, position, Quaternion.identity, transform);
            node.name = wall == WallType.InnerWall ? "NucleusEnvelope" : "CellMembrane";

            node.transform.rotation = Quaternion.Euler(0, 0, (360f / count) * i);

            nodes.Add(node);
        }
    }

    private void SetupPerimeterJoints()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            int nextIndex = (i + 1) % nodes.Count;
            GameObject nodeA = nodes[i];
            GameObject nodeB = nodes[nextIndex];

            SpringJoint2D joint = nodeA.AddComponent<SpringJoint2D>();
            joint.connectedBody = nodeB.GetComponent<Rigidbody2D>();

            joint.anchor = new Vector2(0, 0.2f);
            joint.connectedAnchor = new Vector2(0, -0.2f);

            joint.dampingRatio = 0f;
            joint.frequency = perimeterJointFrequency;
            joint.autoConfigureDistance = false;

            Vector2 worldAnchorA = nodeA.transform.TransformPoint(joint.anchor);
            Vector2 worldAnchorB = nodeB.transform.TransformPoint(joint.connectedAnchor);
            joint.distance = Vector2.Distance(worldAnchorA, worldAnchorB);

            perimeterJoints.Add(joint);
        }
    }

    private void SetupCenterJoints()
    {

        for (int i = 0; i < nodes.Count / 2; i++)
        {
            int oppositeIndex = (i + (nodes.Count / 2)) % nodes.Count;
            GameObject nodeA = nodes[i];
            GameObject nodeB = nodes[oppositeIndex];

            SpringJoint2D joint = nodeA.AddComponent<SpringJoint2D>();
            joint.connectedBody = nodeB.GetComponent<Rigidbody2D>();

            joint.anchor = new Vector2(0, 0);
            joint.connectedAnchor = new Vector2(0, 0);

            joint.dampingRatio = 0f;
            joint.frequency = centerJointFrequency;
            joint.autoConfigureConnectedAnchor = false;

            Vector2 worldAnchorA = nodeA.transform.TransformPoint(joint.anchor);
            Vector2 worldAnchorB = nodeB.transform.TransformPoint(joint.connectedAnchor);
            joint.distance = Vector2.Distance(worldAnchorA, worldAnchorB);

            centerJoints.Add(joint);
        }
    }

    private void ClearExistingMembrane()
    {
        foreach (var joint in perimeterJoints) if (joint) Destroy(joint);
        foreach (var joint in centerJoints) if (joint) Destroy(joint);
        foreach (var node in nodes) if (node) Destroy(node);

        perimeterJoints.Clear();
        centerJoints.Clear();
        nodes.Clear();
    }
}