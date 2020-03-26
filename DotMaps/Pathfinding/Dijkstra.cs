using System;
using System.Collections.Generic;
using DotMaps.Datastructures;

namespace DotMaps.Pathfinding
{
    public class Dijkstra
    {

        public static List<Graph.Connection> FindShortestPathTime(Graph graph, Graph.Node start, Graph.Node finish)
        {
            Graph calculated = FindShortestPathTime(graph, finish);

            List<Graph.Connection> path = new List<Graph.Connection>();
            Graph.Node current = start;
            while (current != finish)
            {
                foreach (Graph.Connection connection in current.previousNode.GetConnections())
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

        public static Graph FindShortestPathTime(Graph graph, Graph.Node finish)
        {
            foreach (Graph.Node node in graph.nodes.Values)
            {
                node.previousNode = null;
                node.timeRequired = double.MaxValue;
            }
            finish.timeRequired = 0;
            DijkstraTime(ref graph, finish);
            return graph;
        }

        private static void DijkstraTime(ref Graph graph, Graph.Node current)
        {
            foreach (Graph.Connection connection in current.GetConnections())
            {
                if (connection.neighbor.timeRequired > current.timeRequired + connection.timeNeeded)
                {
                    connection.neighbor.timeRequired = current.timeRequired + connection.timeNeeded;
                    connection.neighbor.previousNode = current;
                    DijkstraTime(ref graph, connection.neighbor);
                }
            }
        }

        public static List<Graph.Connection> FindShortestPathDistance(Graph graph, Graph.Node start, Graph.Node finish)
        {
            Graph calculated = FindShortestPathDistance(graph, finish);

            List<Graph.Connection> path = new List<Graph.Connection>();
            Graph.Node current = start;
            while (current != finish)
            {
                foreach (Graph.Connection connection in current.previousNode.GetConnections())
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

        public static Graph FindShortestPathDistance(Graph graph, Graph.Node finish)
        {
            foreach (Graph.Node node in graph.nodes.Values)
            {
                node.previousNode = null;
                node.timeRequired = double.MaxValue;
            }
            finish.timeRequired = 0;
            DijkstraDistance(ref graph, finish);
            return graph;
        }

        private static void DijkstraDistance(ref Graph graph, Graph.Node current)
        {
            foreach (Graph.Connection connection in current.GetConnections())
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
