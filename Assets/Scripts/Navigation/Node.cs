using UnityEngine;

namespace Navigation {
    public class Node
    {
        public int X { get; set; }
        public int Y { get; set; }
        
        // For pathfinding: Parent node in the path
        public Node Parent { get; set; }
        
        // Cost from start node to this node
        public float G { get; set; } = float.MaxValue;
        
        // Heuristic cost estimate from this node to the target
        public float H { get; set; } = 0;
        
        // Total cost: f = g + h
        public float F => G + H;

        // The world position of this node.
        public Vector3 WorldPosition { get; set; }

        public override string ToString()
        {
            return "Node: " + X + ", " + Y;
        }
    }
}