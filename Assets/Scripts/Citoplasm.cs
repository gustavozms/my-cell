using System.Collections.Generic;
using UnityEngine;

public class Citoplasm : MonoBehaviour
{

    [SerializeField] private GameObject plasmPrefab;
    [SerializeField] private float plasmCount = 30;
    private List<GameObject> citoplasm = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < plasmCount; i++)
        {
            GameObject node = Instantiate(plasmPrefab, Vector2.zero, Quaternion.identity, transform);
            node.name = "Citoplasm" + i;

            citoplasm.Add(node);
        }
    }


}
