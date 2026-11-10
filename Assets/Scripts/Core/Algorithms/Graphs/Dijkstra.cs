using System.Collections.Generic;

namespace DSAVisualizer.Core.Algorithms
{
    /// <summary>
    /// Dijkstra's shortest paths from StartNode. If Target is a valid node, the shortest path to it
    /// is highlighted at the end. Uses a simple linear scan for the minimum: fine for 8 nodes and
    /// much easier to follow than a heap.
    /// </summary>
    public sealed class Dijkstra : IAlgorithm
    {
        public string Name { get { return "Dijkstra's Shortest Path"; } }
        public AlgoCategory Category { get { return AlgoCategory.Graphs; } }
        public Complexity TimeComplexity { get { return new Complexity("O(V²)", "O(V²)", "O(V²)"); } }
        public string SpaceComplexity { get { return "O(V)"; } }
        public bool IsStable { get { return false; } } // not meaningful for graph algorithms
        public string RealWorldUse { get { return "Maps and navigation, and network routing (OSPF). With a heap it runs in O((V+E) log V)."; } }

        private static readonly string[] Lines =
        {
            "dist[start] = 0; all others = ∞",                   // 0
            "while unvisited nodes remain",                      // 1
            "  u = unvisited node with smallest dist",           // 2
            "  mark u visited (dist[u] is final)",               // 3
            "  for each unvisited neighbor v of u",              // 4
            "    if dist[u] + w(u,v) < dist[v]",                 // 5
            "      dist[v] = dist[u] + w(u,v); prev[v] = u",     // 6
            "follow prev[] back from the target"                 // 7
        };
        public string[] Pseudocode { get { return Lines; } }

        public List<AlgoStep> Record(AlgoInput input)
        {
            var r = new StepRecorder(input);
            GraphData g = input.Graph;
            int n = g.NodeCount;
            int[] dist = r.A; // starts as all Infinity
            var prev = new int[n];
            var visited = new bool[n];
            for (int i = 0; i < n; i++) prev[i] = -1;

            r.Overwrite(input.StartNode, 0, 0, "Distance to the start " + GraphData.Label(input.StartNode) + " is 0; everything else is ∞ (unknown)");

            for (int round = 0; round < n; round++)
            {
                int u = -1;
                for (int i = 0; i < n; i++)
                    if (!visited[i] && dist[i] != AlgoState.Infinity && (u < 0 || dist[i] < dist[u])) u = i;
                if (u < 0) break; // the rest cannot be reached

                visited[u] = true;
                string uName = GraphData.Label(u);
                r.Visit(u, 3, uName + " has the smallest tentative distance (" + dist[u] + "), so that distance is final");

                IReadOnlyList<int> neighbors = g.Neighbors(u);
                for (int k = 0; k < neighbors.Count; k++)
                {
                    int v = neighbors[k];
                    if (visited[v]) continue;
                    string vName = GraphData.Label(v);
                    int w = g.Weight(u, v);
                    int candidate = dist[u] + w;
                    string old = dist[v] == AlgoState.Infinity ? "∞" : dist[v].ToString();
                    if (candidate < dist[v])
                    {
                        r.CompareValues(u, v, candidate, dist[v], 5,
                            "Via " + uName + ": " + dist[u] + " + " + w + " = " + candidate + " → shorter than " + vName + "'s " + old);
                        r.Overwrite(v, candidate, 6, "Update " + vName + "'s distance: " + old + " → " + candidate, u);
                        prev[v] = u;
                    }
                    else
                    {
                        r.CompareValues(u, v, candidate, dist[v], 5,
                            "Via " + uName + ": " + dist[u] + " + " + w + " = " + candidate + " → not shorter than " + vName + "'s " + old + ", keep it");
                    }
                }
            }

            int target = input.Target;
            if (target >= 0 && target < n)
            {
                string tName = GraphData.Label(target);
                if (dist[target] == AlgoState.Infinity)
                {
                    r.NotFound(7, tName + " cannot be reached from " + GraphData.Label(input.StartNode));
                }
                else
                {
                    var path = new List<int>();
                    for (int at = target; at >= 0; at = prev[at]) path.Add(at);
                    path.Reverse();
                    var names = new System.Text.StringBuilder();
                    for (int i = 0; i < path.Count; i++)
                    {
                        if (i > 0) names.Append(" → ");
                        names.Append(GraphData.Label(path[i]));
                    }
                    r.HighlightPath(path.ToArray(), 7, "Shortest path to " + tName + ": " + names + " (total " + dist[target] + ")");
                    r.Found(target, 7, "Reached " + tName + " with total distance " + dist[target]);
                }
            }
            r.Done(1, "Done. Dijkstra always finalises the closest unvisited node next.");
            return r.Steps;
        }
    }
}
