using System;
using System.Collections.Generic;
using System.Linq;

namespace Navigation {
    // A* Algorithm: https://en.wikipedia.org/wiki/A*_search_algorithm
    public class AStar
    {
        // A list to track nodes that were modified during the search
        private List<Node> exploredNodes = new List<Node>();

        /// <summary>
        /// Finds the shortest path between the start and target nodes on a grid using the A* algorithm.
        /// </summary>
        /// <param name="grid">A two-dimensional array of nodes.</param>
        /// <param name="start">The starting node.</param>
        /// <param name="target">The target node.</param>
        /// <returns>A list of nodes representing the path from start to target, or null if no path is found.</returns>
        public List<Node> FindPath(Node[,] grid, Node start, Node target)
        {
            // Clear any previous exploration records.
            exploredNodes.Clear();

            // Nodes to be evaluated
            List<Node> openSet = new List<Node>();

            // Nodes already evaluated
            HashSet<Node> closedSet = new HashSet<Node>();

            // Initialize the start no de
            start.G = 0;
            start.H = GetDistance(start, target);
            openSet.Add(start);
            exploredNodes.Add(start);

            while (openSet.Count > 0)
            {
                // Get the node with the lowest F score
                Node currentNode = openSet.OrderBy(n => n.F).First();

                // If we reached the target, retrace the path, reset explored nodes, and return the path
                if (currentNode == target)
                {
                    List<Node> path = ReconstructPath(start, target);
                    ResetExploredNodes();
                    return path;
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                // Loop through all neighbors of the current node
                foreach (Node neighbor in GetNeighbors(currentNode, grid))
                {
                    // Skip if neighbor is already evaluated
                    if (closedSet.Contains(neighbor))
                        continue;

                    // Calculate the tentative G cost (using Euclidean distance)
                    float tentativeG = currentNode.G + GetDistance(currentNode, neighbor);

                    // If new path to neighbor is shorter
                    if (tentativeG < neighbor.G)
                    {
                        neighbor.G = tentativeG;
                        neighbor.H = GetDistance(neighbor, target);
                        neighbor.Parent = currentNode;

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                            // Track the neighbor as explored if not already
                            if (!exploredNodes.Contains(neighbor))
                                exploredNodes.Add(neighbor);
                        }
                    }
                }
            }
            // No path found, reset explored nodes and return null.
            ResetExploredNodes();
            return null;
        }

        /// <summary>
        /// Retraces the path from the target node back to the start node.
        /// </summary>
        private List<Node> ReconstructPath(Node start, Node end)
        {
            List<Node> path = new List<Node>();
            Node current = end;
            while (current != start)
            {
                path.Add(current);
                current = current.Parent;
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Calculates the Euclidean distance between two nodes.
        /// </summary>
        private float GetDistance(Node a, Node b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Returns a list of neighbor nodes (including diagonals) for a given node.
        /// </summary>
        private List<Node> GetNeighbors(Node node, Node[,] grid)
        {
            List<Node> neighbors = new List<Node>();
            int gridWidth = grid.GetLength(0);
            int gridHeight = grid.GetLength(1);

            // Check all adjacent positions
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    // Skip the current node
                    if (dx == 0 && dy == 0)
                        continue;

                    int checkX = node.X + dx;
                    int checkY = node.Y + dy;

                    // Ensure the neighbor is within grid bounds
                    if (checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight)
                    {
                        neighbors.Add(grid[checkX, checkY]);
                    }
                }
            }
            return neighbors;
        }

        // This method resets only nodes that were explored in the previous search.
        private void ResetExploredNodes()
        {
            foreach (Node node in exploredNodes)
            {
                node.G = float.MaxValue;  // Reset G cost to "infinity"
                node.H = 0;
                node.Parent = null;
            }
            exploredNodes.Clear();
        }
    }
}