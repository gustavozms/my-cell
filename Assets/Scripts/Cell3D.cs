using UnityEngine;

public class Cell3D : MonoBehaviour
{
    [SerializeField] GameObject wallNode; // Prefab for the node
    [SerializeField] int nodeCount = 21; // Number of nodes in each circle
    [SerializeField] float sphereRadius = 16f; // Radius of the sphere
    [SerializeField] float springStrength = 100f; // Strength of the spring joints
    [SerializeField] float springDamping = 0.5f; // Damping of the spring joints

    void Start()
    {
        SetupCellularMembrane();
    }

    void SetupCellularMembrane()
    {
        // Create a parent object to hold all the nodes
        GameObject nodeContainer = new GameObject("NodeContainer");
        nodeContainer.transform.parent = this.transform;

        // Calculate the number of circles based on nodeCount
        int circleCount = Mathf.CeilToInt(nodeCount / 2f); // Adjust for even distribution

        // Array to store the created nodes
        GameObject[,] nodes = new GameObject[circleCount, nodeCount];

        // Calculate the angle increments
        float horizontalAngleIncrement = 360f / nodeCount; // Angle between nodes in a circle
        float verticalAngleIncrement = 180f / (circleCount + 1); // Angle between circles

        // Create the nodes in a spherical formation
        for (int circleIndex = 0; circleIndex < circleCount; circleIndex++)
        {
            float verticalAngle = (circleIndex + 1) * verticalAngleIncrement; // Angle from the top

            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                float horizontalAngle = nodeIndex * horizontalAngleIncrement * Mathf.Deg2Rad;

                // Calculate the position of the node using spherical coordinates
                Vector3 nodePosition = new Vector3(
                    Mathf.Sin(verticalAngle * Mathf.Deg2Rad) * Mathf.Cos(horizontalAngle) * sphereRadius,
                    Mathf.Cos(verticalAngle * Mathf.Deg2Rad) * sphereRadius,
                    Mathf.Sin(verticalAngle * Mathf.Deg2Rad) * Mathf.Sin(horizontalAngle) * sphereRadius
                );

                // Instantiate the node
                GameObject node = Instantiate(wallNode, nodePosition, Quaternion.identity, nodeContainer.transform);
                nodes[circleIndex, nodeIndex] = node;

                // Add a Rigidbody to the node (required for SpringJoint)
                Rigidbody rb = node.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = node.AddComponent<Rigidbody>();
                }
                rb.useGravity = false; // Disable gravity for better control

                // Connect the node to the previous node in the same circle (if not the first circle)
                if (nodeIndex > 0)
                {
                    ConnectNodesWithSpring(nodes[circleIndex, nodeIndex - 1], node);
                }

                // Connect the node to the node in the previous circle (if it exists)
                if (circleIndex > 0)
                {
                    ConnectNodesWithSpring(nodes[circleIndex - 1, nodeIndex], node);
                }
            }

            // Connect the last node in the circle to the first node to close the circle
            ConnectNodesWithSpring(nodes[circleIndex, nodeCount - 1], nodes[circleIndex, 0]);
        }

        // Add the top and bottom nodes
        AddPoleNodes(nodeContainer, nodes, circleCount);
    }

    void AddPoleNodes(GameObject nodeContainer, GameObject[,] nodes, int circleCount)
    {
        // Add the top node (0°)
        GameObject topNode = Instantiate(wallNode, new Vector3(0, sphereRadius, 0), Quaternion.identity, nodeContainer.transform);
        Rigidbody topRb = topNode.GetComponent<Rigidbody>();
        if (topRb == null)
        {
            topRb = topNode.AddComponent<Rigidbody>();
        }
        topRb.useGravity = false;

        // Add the bottom node (180°)
        GameObject bottomNode = Instantiate(wallNode, new Vector3(0, -sphereRadius, 0), Quaternion.identity, nodeContainer.transform);
        Rigidbody bottomRb = bottomNode.GetComponent<Rigidbody>();
        if (bottomRb == null)
        {
            bottomRb = bottomNode.AddComponent<Rigidbody>();
        }
        bottomRb.useGravity = false;

        // Connect the top node to the first circle
        for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
        {
            ConnectNodesWithSpring(topNode, nodes[0, nodeIndex]);
        }

        // Connect the bottom node to the last circle
        for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
        {
            ConnectNodesWithSpring(bottomNode, nodes[circleCount - 1, nodeIndex]);
        }
    }

    void ConnectNodesWithSpring(GameObject nodeA, GameObject nodeB)
    {
        // Add a SpringJoint to nodeA and connect it to nodeB
        SpringJoint spring = nodeA.AddComponent<SpringJoint>();
        spring.connectedBody = nodeB.GetComponent<Rigidbody>();
        spring.spring = springStrength;
        spring.damper = springDamping;
        spring.autoConfigureConnectedAnchor = true;
        spring.anchor = Vector3.zero;
        spring.connectedAnchor = Vector3.zero;
    }
}