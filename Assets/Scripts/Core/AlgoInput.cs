using System.Collections.Generic;

namespace DSAVisualizer.Core
{
    /// <summary>
    /// Everything an algorithm might need. Sorting reads <see cref="Values"/>;
    /// searching adds <see cref="Target"/>; graph algorithms read <see cref="Graph"/>,
    /// <see cref="StartNode"/> and (for Dijkstra) <see cref="Target"/> as the destination node.
    /// </summary>
    public sealed class AlgoInput
    {
        public int[] Values = new int[0];
        public int Target = -1;
        public GraphData Graph;
        public int StartNode;

        public static AlgoInput FromValues(params int[] values)
        {
            return new AlgoInput { Values = (int[])values.Clone() };
        }

        public static AlgoInput ForSearch(int[] sortedValues, int target)
        {
            return new AlgoInput { Values = (int[])sortedValues.Clone(), Target = target };
        }

        public static AlgoInput ForGraph(GraphData graph, int start, int target = -1)
        {
            return new AlgoInput { Graph = graph, StartNode = start, Target = target };
        }
    }

    public struct GraphEdge
    {
        public int From;
        public int To;
        public int Weight;

        public GraphEdge(int from, int to, int weight)
        {
            From = from;
            To = to;
            Weight = weight;
        }
    }

    /// <summary>Small undirected weighted graph. Node ids are 0..NodeCount-1.</summary>
    public sealed class GraphData
    {
        public readonly int NodeCount;
        public readonly List<GraphEdge> Edges = new List<GraphEdge>();

        private List<int>[] _neighbors; // built lazily, sorted by node id for deterministic traversals

        public GraphData(int nodeCount)
        {
            NodeCount = nodeCount;
        }

        public GraphData AddEdge(int from, int to, int weight = 1)
        {
            Edges.Add(new GraphEdge(from, to, weight));
            _neighbors = null;
            return this;
        }

        /// <summary>Neighbors of a node in ascending id order.</summary>
        public IReadOnlyList<int> Neighbors(int node)
        {
            if (_neighbors == null)
            {
                _neighbors = new List<int>[NodeCount];
                for (int i = 0; i < NodeCount; i++) _neighbors[i] = new List<int>();
                for (int e = 0; e < Edges.Count; e++)
                {
                    _neighbors[Edges[e].From].Add(Edges[e].To);
                    _neighbors[Edges[e].To].Add(Edges[e].From);
                }
                for (int i = 0; i < NodeCount; i++) _neighbors[i].Sort();
            }
            return _neighbors[node];
        }

        /// <summary>Weight of the edge between two nodes, or -1 if they are not connected.</summary>
        public int Weight(int a, int b)
        {
            for (int e = 0; e < Edges.Count; e++)
            {
                GraphEdge edge = Edges[e];
                if ((edge.From == a && edge.To == b) || (edge.From == b && edge.To == a)) return edge.Weight;
            }
            return -1;
        }

        /// <summary>Human-friendly node name: 0 -> "A", 1 -> "B", ...</summary>
        public static string Label(int node)
        {
            return ((char)('A' + node)).ToString();
        }

        /// <summary>The 8-node demo graph used by the Graph Lab and the tests.</summary>
        public static GraphData CreateDemo()
        {
            return new GraphData(8)
                .AddEdge(0, 1, 4).AddEdge(0, 2, 2).AddEdge(1, 2, 1).AddEdge(1, 3, 5)
                .AddEdge(2, 4, 8).AddEdge(3, 4, 2).AddEdge(3, 5, 6).AddEdge(4, 6, 3)
                .AddEdge(5, 6, 1).AddEdge(5, 7, 2).AddEdge(6, 7, 7);
        }
    }
}
