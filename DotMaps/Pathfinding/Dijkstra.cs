using System.Collections.Generic;
using DotMaps.Datastructures;

namespace DotMaps.Pathfinding
{
    public class Dijkstra
    {

        public static List<Connection> FindShortestPathTime(Graph graph, _3DNode start, _3DNode finish)
        {
            CalculateGraphTime(graph, finish);

            List<Connection> path = new List<Connection>();
            _3DNode current = start;
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

        public static Graph CalculateGraphTime(Graph graph, _3DNode finish)
        {
            foreach (_3DNode node in graph.nodes.Values)
            {
                node.previousNode = null;
                node.timeRequired = double.MaxValue;
            }
            finish.timeRequired = 0;
            DijkstraTime(ref graph, finish);
            return graph;
        }

        private static void DijkstraTime(ref Graph graph, _3DNode current)
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

        public static List<_3DNode> FindShortestPathDistance(Graph graph, _3DNode start, _3DNode finish)
        {
            CalculateGraphDistance(graph, finish);

            List<_3DNode> path = new List<_3DNode>();
            _3DNode current = start;
            while(current != finish)
            {
                path.Add(current);
                current = current.previousNode;
            }
            path.Add(finish);
            return path;
        }

        public static Graph CalculateGraphDistance(Graph graph, _3DNode finish)
        {
            foreach (_3DNode node in graph.nodes.Values)
            {
                node.previousNode = null;
                node.timeRequired = double.MaxValue;
            }
            finish.timeRequired = 0;
            DijkstraDistance(ref graph, finish);
            return graph;
        }

        private static void DijkstraDistance(ref Graph graph, _3DNode current)
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
