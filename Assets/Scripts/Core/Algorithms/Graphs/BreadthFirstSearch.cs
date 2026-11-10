using System.Collections.Generic;

namespace DSAVisualizer.Core.Algorithms
{
    public sealed class BreadthFirstSearch : IAlgorithm
    {
        public string Name { get { return "Breadth-First Search"; } }
        public AlgoCategory Category { get { return AlgoCategory.Graphs; } }
        public Complexity TimeComplexity { get { return new Complexity("O(V + E)", "O(V + E)", "O(V + E)"); } }
        public string SpaceComplexity { get { return "O(V)"; } }
        public bool IsStable { get { return false; } } // not meaningful for traversals
        public string RealWorldUse { get { return "Fewest-hops routes: friend suggestions, GPS on unweighted maps, web crawlers."; } }

        private static readonly string[] Lines =
        {
            "mark start discovered; enqueue start",      // 0
            "while queue is not empty",                  // 1
            "  u = dequeue",                             // 2
            "  visit u",                                 // 3
            "  for each neighbor v of u",                // 4
            "    if v is not discovered",                // 5
            "      mark v discovered; enqueue v"         // 6
        };
        public string[] Pseudocode { get { return Lines; } }

        public List<AlgoStep> Record(AlgoInput input)
        {
            var r = new StepRecorder(input);
            GraphData g = input.Graph;
            var discovered = new bool[g.NodeCount];

            discovered[input.StartNode] = true;
            r.Enqueue(input.StartNode, 0, "Start at " + GraphData.Label(input.StartNode) + ": put it in the queue");

            while (r.State.Frontier.Count > 0)
            {
                string front = GraphData.Label(r.State.Frontier[0]);
                int u = r.Dequeue(2, "Take " + front + " from the FRONT of the queue (first in, first out)");
                r.Visit(u, 3, "Visit " + front + " (#" + (r.State.Visited.Count + 1) + " in the order)");

                IReadOnlyList<int> neighbors = g.Neighbors(u);
                for (int k = 0; k < neighbors.Count; k++)
                {
                    int v = neighbors[k];
                    string name = GraphData.Label(v);
                    if (!discovered[v])
                    {
                        r.Highlight(u, v, 5, "Neighbor " + name + " is new");
                        discovered[v] = true;
                        r.Enqueue(v, 6, "Add " + name + " to the BACK of the queue");
                    }
                    else
                    {
                        r.Highlight(u, v, 5, "Neighbor " + name + " was already discovered → skip it");
                    }
                }
            }
            r.Done(1, "Queue is empty. BFS explored level by level, nearest nodes first.");
            return r.Steps;
        }
    }
}
