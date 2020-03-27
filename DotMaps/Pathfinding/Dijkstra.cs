using System.Collections.Generic;
using DotMaps.Datastructures;

namespace DotMaps.Pathfinding
{
    public class Dijkstra
    {

        public static List<Connection> FindShortestPathTime(Graph graph, Node start, Node finish)
        {
            CalculateGraphTime(graph, finish);

            List<Connection> path = new List<Connection>();
            Node current = start;
            while (current != finish)
            {
                foreach (Connection connection in current.previousNode.GetConnections())
                {
                    if (connection.neighbor == current)
                    {
                        path.Add(connection);
                        break;
                    }
                }
                current = current.previousNode;
            }
            return path;
        }

        public static Graph CalculateGraphTime(Graph graph, Node finish)
        {
            foreach (Node node in graph.nodes.Values)
            {
                node.previousNode = null;
                node.timeRequired = double.MaxValue;
            }
            finish.timeRequired = 0;
            DijkstraTime(ref graph, finish);
            return graph;
        }

        private static void DijkstraTime(ref Graph graph, Node current)
        {
            foreach (Connection connection in current.GetConnections())
            {
                if (connection.neighbor.timeRequired > current.timeRequired + connection.timeNeeded)
                {
                    connection.neighbor.timeRequired = current.timeRequired + connection.timeNeeded;
                    connection.neighbor.previousNode = current;
                    DijkstraTime(ref graph, connection.neighbor);
                }
            }
        }

        public static List<Node> FindShortestPathDistance(Graph graph, Node start, Node finish)
        {
            CalculateGraphDistance(graph, finish);

            List<Node> path = new List<Node>();
            Node current = start;
            while(current != finish)
            {
                path.Add(current);
                current = current.previousNode;
            }
            path.Add(finish);
            return path;
        }

        public static Graph CalculateGraphDistance(Graph graph, Node finish)
        {
            foreach (Node node in graph.nodes.Values)
            {
                node.previousNode = null;
                node.timeRequired = double.MaxValue;
            }
            finish.timeRequired = 0;
            DijkstraDistance(ref graph, finish);
            return graph;
        }

        private static void DijkstraDistance(ref Graph graph, Node current)
        {
            foreach (Connection connection in current.GetConnections())
            {
                if (connection.neighbor.timeRequired > current.timeRequired + connection.distance)
                {
                    connection.neighbor.timeRequired = current.timeRequired + connection.distance;
                    connection.neighbor.previousNode = current;
                    DijkstraTime(ref graph, connection.neighbor);
                }
            }
        }
    }
}
