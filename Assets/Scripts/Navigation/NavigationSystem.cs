using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Navigation;  // Assuming Node and AStar are in this namespace

public class NavigationSystem : MonoBehaviour
{
    // Number of nodes along the plane (set via Inspector)
    public int gridNodesX = 10;
    public int gridNodesZ = 10;
    public float verticalOffset = 0.01f;

    // Grid of nodes for pathfinding
    public Node[,] grid;

    void Start()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("MeshRenderer component not found on the plane.");
            return;
        }

        float planeWidth = meshRenderer.bounds.size.x;
        float planeHeight = meshRenderer.bounds.size.z;
        float spacingX = (gridNodesX > 1) ? planeWidth / (gridNodesX - 1) : planeWidth;
        float spacingZ = (gridNodesZ > 1) ? planeHeight / (gridNodesZ - 1) : planeHeight;
        Vector3 bottomLeft = transform.position - new Vector3(planeWidth / 2, 0, planeHeight / 2);

        grid = new Node[gridNodesX, gridNodesZ];

        for (int x = 0; x < gridNodesX; x++)
        {
            for (int z = 0; z < gridNodesZ; z++)
            {
                Vector3 position = bottomLeft + new Vector3(x * spacingX, verticalOffset, z * spacingZ);
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = position;
                cube.transform.parent = transform;

                // Remove the cube's collider.
                Collider col = cube.GetComponent<Collider>();
                if (col != null)
                {
                    Destroy(col);
                }

                Renderer renderer = cube.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }

                // Create corresponding node.
                Node node = new Node {
                    X = x,
                    Y = z,
                    WorldPosition = position,
                };

                grid[x, z] = node;
            }
        }
    }

    /// <summary>
    /// Finds the node in the grid that is closest to the given world position.
    /// </summary>
    public Node GetClosestNode(Vector3 worldPos)
    {
        Node closest = null;
        float minDist = Mathf.Infinity;

        int gridWidth = this.grid.GetLength(0);
        int gridHeight = this.grid.GetLength(1);
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                float dist = Vector3.Distance(worldPos, grid[x, z].WorldPosition);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = this.grid[x, z];
                }
            }
        }
        return closest;
    }
}