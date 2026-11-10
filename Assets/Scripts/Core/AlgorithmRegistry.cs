using System;
using System.Collections.Generic;
using System.Reflection;
using DSAVisualizer.Core.Algorithms;

namespace DSAVisualizer.Core
{
    /// <summary>
    /// The single list of algorithms the app knows about. Menus, labs and tests all read from here,
    /// so a new algorithm shows up everywhere after adding one line to <see cref="RegisterBuiltIns"/>.
    /// </summary>
    public static class AlgorithmRegistry
    {
        private static readonly List<IAlgorithm> Items = new List<IAlgorithm>();
        private static bool _initialized;

        /// <summary>Every registered algorithm, in registration order.</summary>
        public static IReadOnlyList<IAlgorithm> All
        {
            get { EnsureInitialized(); return Items; }
        }

        /// <summary>Adds an algorithm. Registering the same name twice is ignored.</summary>
        public static void Register(IAlgorithm algorithm)
        {
            if (algorithm == null) throw new ArgumentNullException("algorithm");
            for (int i = 0; i < Items.Count; i++)
                if (Items[i].Name == algorithm.Name) return;
            Items.Add(algorithm);
        }

        /// <summary>Finds an algorithm by its display name, or returns null.</summary>
        public static IAlgorithm Get(string name)
        {
            EnsureInitialized();
            for (int i = 0; i < Items.Count; i++)
                if (Items[i].Name == name) return Items[i];
            return null;
        }

        /// <summary>All algorithms of one category, in registration order.</summary>
        public static List<IAlgorithm> GetByCategory(AlgoCategory category)
        {
            EnsureInitialized();
            var result = new List<IAlgorithm>();
            for (int i = 0; i < Items.Count; i++)
                if (Items[i].Category == category) result.Add(Items[i]);
            return result;
        }

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            RegisterBuiltIns();
            DiscoverOthers();
        }

        // Listed explicitly so IL2CPP code stripping can never remove them.
        // ADD NEW ALGORITHMS HERE.
        private static void RegisterBuiltIns()
        {
            Register(new BubbleSort());
            Register(new SelectionSort());
            Register(new InsertionSort());
            Register(new MergeSort());
            Register(new QuickSort());
            Register(new LinearSearch());
            Register(new BinarySearch());
            Register(new BreadthFirstSearch());
            Register(new DepthFirstSearch());
            Register(new Dijkstra());
        }

        // Convenience for the editor: also picks up any IAlgorithm in this assembly that was not listed above.
        private static void DiscoverOthers()
        {
            try
            {
                Type[] types = typeof(AlgorithmRegistry).Assembly.GetTypes();
                for (int i = 0; i < types.Length; i++)
                {
                    Type t = types[i];
                    if (t.IsAbstract || t.IsInterface || !typeof(IAlgorithm).IsAssignableFrom(t)) continue;
                    if (t.GetConstructor(Type.EmptyTypes) == null) continue;
                    if (IsRegisteredType(t)) continue;
                    Register((IAlgorithm)Activator.CreateInstance(t));
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // Built-ins are already registered; discovery is only a convenience.
            }
        }

        private static bool IsRegisteredType(Type t)
        {
            for (int i = 0; i < Items.Count; i++)
                if (Items[i].GetType() == t) return true;
            return false;
        }
    }
}
