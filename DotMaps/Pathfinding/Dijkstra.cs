using DotMaps.Datastructures;
using System.Collections.Generic;

namespace DotMaps.Pathfinding
{
    public class Dijkstra
    {
        public enum Pathtype { FASTEST, SHORTEST}
        public static List<Graph.GraphNode> FindShortestPath(Graph.GraphNode start, Graph.GraphNode goal)
        {
            return FindPath(start, goal, Pathtype.SHORTEST);
        }

        public static List<Graph.GraphNode> FindQuickestPath(Graph.GraphNode start, Graph.GraphNode goal)
        {
            return FindPath(start, goal, Pathtype.FASTEST);
        }

        public static List<Graph.GraphNode> FindPath(Graph.GraphNode start, Graph.GraphNode goal, Pathtype pathtype)
        {
            Queue<Graph.GraphNode> toExplore = new Queue<Graph.GraphNode>();
            toExplore.Enqueue(goal);
            Graph.GraphNode currentNode = toExplore.Peek();
            while (toExplore.Count > 0)
            {
                foreach (Graph.Connection connection in currentNode.connections)
                {
                    switch (pathtype)
                    {
                        case Pathtype.SHORTEST:
                            if (connection.neighbor.weight < currentNode.weight + connection.distance)
                            {
                                connection.neighbor.weight = currentNode.weight + connection.distance;
                                connection.neighbor.previous = currentNode;
                                if (currentNode != start)
                                    toExplore.Enqueue(connection.neighbor);
                            }
                            break;
                        case Pathtype.FASTEST:
                            if (connection.neighbor.weight < currentNode.weight + connection.timeNeeded)
                            {
                                connection.neighbor.weight = currentNode.weight + connection.timeNeeded;
                                connection.neighbor.previous = currentNode;
                                if (currentNode != start)
                                    toExplore.Enqueue(connection.neighbor);
                            }
                            break;
                    }
                    
                }
                currentNode = toExplore.Dequeue();
            }

            List<Graph.GraphNode> path = new List<Graph.GraphNode>();
            while (currentNode.previous != null)
            {
                path.Add(currentNode);
                currentNode = currentNode.previous;
            }
            return path;
        }
    }
}
