using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Navigation;
using Unity.VisualScripting;  // For Node and AStar

namespace Unit {
    public class Unit : MonoBehaviour
    {
        // Movement speed in units per second.
        public float moveSpeed = 10f;

        // Collision push parameters.
        public float pushRadius = 1f;      // Radius for checking overlaps.
        public float pushForce = 1f;         // How strongly to push away.

        // The computed path as a list of nodes.
        private List<Node> path;
        private int currentPathIndex = 0;

        private AStar aStar;
        NavigationSystem navSystem;

        void Start()
        {
            this.aStar = new AStar();
            this.navSystem = FindObjectOfType<NavigationSystem>();
        }

        void Update()
        {
            // Move along the computed path if one exists.
            if (path != null && currentPathIndex < path.Count)
            {
                Vector3 currentPos = transform.position;
                // Get the target node position, preserving the unit's current Y value.
                Vector3 targetPos = path[currentPathIndex].WorldPosition;
                targetPos = new Vector3(targetPos.x, currentPos.y, targetPos.z);

                // Move only on the X and Z axes.
                transform.position = Vector3.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);

                // If close enough to the target node, move to the next one.
                if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                       new Vector3(targetPos.x, 0, targetPos.z)) < 0.1f)
                {
                    currentPathIndex++;
                }
            }
            // Reset the path if we've reached the end.
            else if (path != null && currentPathIndex >= path.Count)
            {
                path = null;
                currentPathIndex = 0;
            }

            // Resolve collisions by gently pushing away nearby units.
            ResolveCollisions();
        }

        /// <summary>
        /// Sets a destination for the unit. Uses the NavigationSystem and A* to compute a path.
        /// </summary>
        public void SetDestination(Vector3 destination)
        {
            if (navSystem == null)
            {
                Debug.LogError("NavigationSystem not found in scene!");
                return;
            }

            // Get the closest nodes to the unit's current position and the destination.
            Node startNode = navSystem.GetClosestNode(transform.position);
            Node targetNode = navSystem.GetClosestNode(destination);

            // Compute the path using A*.
            path = aStar.FindPath(navSystem.grid, startNode, targetNode);
            if (path == null) {
                Debug.Log("There is no path to " + destination);
            }
            
            currentPathIndex = 0;
        }

        /// <summary>
        /// Checks for nearby units and gently pushes this unit away.
        /// </summary>
        void ResolveCollisions()
        {
            // Find all colliders within the pushRadius.
            Collider[] colliders = Physics.OverlapSphere(transform.position, pushRadius);

            foreach (Collider col in colliders)
            {
                if (col.gameObject != gameObject && col.CompareTag("Unit"))
                {
                    // Compute a normalized direction away from the other unit.
                    Vector3 pushDir = (transform.position - col.transform.position).normalized;
                    // Adjust position based on push force.
                    transform.position += pushDir * pushForce * Time.deltaTime;
                }
            }
        }
    }
}