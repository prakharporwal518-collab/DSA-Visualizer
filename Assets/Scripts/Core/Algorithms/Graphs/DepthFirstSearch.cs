using System.Collections.Generic;

namespace DSAVisualizer.Core.Algorithms
{
    /// <summary>Iterative DFS with an explicit stack, so the stack can be shown as a tower.</summary>
    public sealed class DepthFirstSearch : IAlgorithm
    {
        public string Name { get { return "Depth-First Search"; } }
        public AlgoCategory Category { get { return AlgoCategory.Graphs; } }
        public Complexity TimeComplexity { get { return new Complexity("O(V + E)", "O(V + E)", "O(V + E)"); } }
        public string SpaceComplexity { get { return "O(V)"; } }
        public bool IsStable { get { return false; } } // not meaningful for traversals
        public string RealWorldUse { get { return "Maze solving, detecting cycles, and ordering build steps (topological sort)."; } }

        private static readonly string[] Lines =
        {
            "push start",                                // 0
            "while stack is not empty",                  // 1
            "  u = pop",                                 // 2
            "  if u is already visited: skip it",        // 3
            "  visit u",                                 // 4
            "  for each neighbor v of u",                // 5
            "    if v is not visited: push v"            // 6
        };
        public string[] Pseudocode { get { return Lines; } }

        public List<AlgoStep> Record(AlgoInput input)
        {
            var r = new StepRecorder(input);
            GraphData g = input.Graph;
            var visited = new bool[g.NodeCount];

            r.Push(input.StartNode, 0, "Start at " + GraphData.Label(input.StartNode) + ": push it onto the stack");

            while (r.State.Frontier.Count > 0)
            {
                string top = GraphData.Label(r.State.Frontier[r.State.Frontier.Count - 1]);
                int u = r.Pop(2, "Take " + top + " from the TOP of the stack (last in, first out)");
                if (visited[u])
                {
                    r.Highlight(u, -1, 3, top + " was already visited → skip it");
                    continue;
                }
                visited[u] = true;
                r.Visit(u, 4, "Visit " + top + " (#" + (r.State.Visited.Count + 1) + " in the order)");

                // Push in reverse so the lowest-lettered neighbor ends up on top and is explored first.
                IReadOnlyList<int> neighbors = g.Neighbors(u);
                for (int k = neighbors.Count - 1; k >= 0; k--)
                {
                    int v = neighbors[k];
                    string name = GraphData.Label(v);
                    if (!visited[v])
                        r.Push(v, 6, "Neighbor " + name + " is unvisited → push it on top");
                    else
                        r.Highlight(u, v, 6, "Neighbor " + name + " is already visited → skip it");
                }
            }
            r.Done(1, "Stack is empty. DFS went as deep as it could before backing up.");
            return r.Steps;
        }
    }
}
